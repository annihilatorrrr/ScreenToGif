using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Models.Project.Recording;
using ScreenToGif.Domain.Models.Project.Recording.Events;
using ScreenToGif.Util.Codification;
using ScreenToGif.Util.Extensions;
using ScreenToGif.Util.JsonConverters;
using ScreenToGif.Util.Settings;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Windows.Input;

namespace ScreenToGif.Util.Project;

public static class LegacyProjectHelper
{
    public static RecordingProject ReadFromPath(string path, bool deleteOld = true)
    {
        var jsonPath = Path.Combine(path, "Project.json");

        if (!File.Exists(jsonPath))
            throw new Exception("The file with project properties was not found.");

        using var fileStream = new FileStream(jsonPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var json = JsonDocument.Parse(fileStream);

        var deserializeOptions = new JsonSerializerOptions();
        deserializeOptions.Converters.Add(new UnixEpochDateTimeOffsetConverter());
        deserializeOptions.Converters.Add(new UnixEpochDateTimeConverter());

        var date = json.RootElement.GetProperty("CreationDate").Deserialize<DateTime>(deserializeOptions);
        var basePath = Path.Combine(UserSettings.All.TemporaryFolderResolved, "ScreenToGif", "Recordings", date.ToString("yyyy-MM-dd HH-mm-ss"));

        Directory.CreateDirectory(basePath);

        var project = new RecordingProject
        {
            PropertiesCachePath = Path.Combine(basePath, "Properties.cache"),
            FramesCachePath = Path.Combine(basePath, "Frames.cache"),
            KeyboardEventsCachePath = Path.Combine(basePath, "KeyboardEvents.cache"),
            CreatedBy = (ProjectSources)json.RootElement.GetProperty("CreatedBy").GetInt32(),
            CreationDate = date,
            Width = json.RootElement.GetProperty("Width").GetInt32(),
            Height = json.RootElement.GetProperty("Height").GetInt32(),
            Dpi = json.RootElement.GetProperty("Dpi").GetDouble(),
            BitsPerChannel = 8,
            ChannelCount = (byte)(json.RootElement.GetProperty("BitDepth").GetInt32() == 24 ? 3 : 4)
        };

        //Properties.
        project.WritePropertiesToDisk();

        //Track (frames).
        project.WriteEmptyFrameSequenceHeader();

        var timeStamp = 0L;

        using var framesFileStream = new FileStream(project.FramesCachePath, FileMode.Create, FileAccess.Write, FileShare.None);
        using var framesBinaryWriter = new ExBinaryWriter(framesFileStream);

        using var keysFileStream = new FileStream(project.KeyboardEventsCachePath, FileMode.Create, FileAccess.Write, FileShare.None);
        using var keysBinaryWriter = new ExBinaryWriter(keysFileStream);

        //Frames and Key events.
        foreach (var frame in json.RootElement.GetProperty("Frames").EnumerateArray())
        {
            //Project may have been moved, so assemble true path manually.
            var persistedPath = frame.GetProperty("Path").GetString();
            var frameName = Path.GetFileName(persistedPath);
            var framePath = Path.Combine(path, frameName ?? "");

            var delay = frame.GetProperty("Delay").GetInt32();
            timeStamp += TimeSpan.FromMilliseconds(delay).Ticks;

            //Frames.
            if (File.Exists(framePath))
            {
                var reader = new PixelUtil(framePath.SourceFrom());
                reader.LockBits();

                project.Frames.Add(new RecordingFrame
                {
                    StreamPosition = framesFileStream.Position,
                    TimeStampInTicks = timeStamp,
                    ExpectedDelay = delay,
                    DataLength = reader.Pixels.LongLength
                });

                //Sub-sequence.
                framesBinaryWriter.Write((byte)SubSequenceTypes.Frame); //1 byte.
                framesBinaryWriter.Write(timeStamp); //8 bytes.
                framesBinaryWriter.Write(delay); //4 bytes, expected delay.

                //Rect sub-sequence.
                framesBinaryWriter.Write(0); //4 bytes, left.
                framesBinaryWriter.Write(0); //4 bytes, top.
                framesBinaryWriter.Write((ushort)reader.Width); //2 bytes, width.
                framesBinaryWriter.Write((ushort)reader.Height); //2 bytes, height.
                framesBinaryWriter.Write(BitConverter.GetBytes(0F)); //4 bytes, angle.

                //Raster sub-sequence. 
                framesBinaryWriter.Write((ushort)reader.Width); //2 bytes, original width.
                framesBinaryWriter.Write((ushort)reader.Height); //2 bytes, original height.
                framesBinaryWriter.WriteTwice(BitConverter.GetBytes(Convert.ToSingle(project.Dpi))); //4+4 bytes, dpi.
                framesBinaryWriter.Write((byte)reader.ChannelsPerPixel); //1 byte.
                framesBinaryWriter.Write((byte)(reader.Depth / reader.ChannelsPerPixel)); //1 byte.

                framesBinaryWriter.Write(reader.Pixels.LongLength); //8 bytes, uncompressed length.

                var start = framesFileStream.Position;
                framesBinaryWriter.Write(0L); //8 bytes.

                using (var compressStream = new DeflateStream(framesFileStream, UserSettings.All.CaptureCompression, true))
                {
                    compressStream.Write(reader.Pixels);
                    compressStream.Flush();
                }

                var end = framesFileStream.Position;
                var compressedLength = end - start - 8;

                framesFileStream.Position = start;
                framesBinaryWriter.Write(compressedLength); //8 bytes, compressed length.
                framesFileStream.Position = end;

                reader.UnlockBitsWithoutCommit();
            }

            //Key events.
            foreach (var key in frame.GetProperty("Keys").EnumerateArray())
            {
                var keyEvent = new KeyEvent
                {
                    TimeStampInTicks = timeStamp,
                    Key = (Key)key.GetProperty("Key").GetInt32(),
                    Modifiers = key.TryGetProperty("Mod", out var modProp) ? modProp.TryGetInt32(out var mod) ? (ModifierKeys)mod : ModifierKeys.None : ModifierKeys.None,
                    IsUppercase = key.GetProperty("IsUppercase").GetBoolean(),
                    WasInjected = key.GetProperty("IsInjected").GetBoolean(),
                    StreamPosition = keysBinaryWriter.Position
                };

                keysBinaryWriter.Write((byte)RecordingEvents.Key); //Key event type.
                keysBinaryWriter.Write(keyEvent.TimeStampInTicks); //TimeStamp since capture start.
                keysBinaryWriter.Write((int)keyEvent.Key);
                keysBinaryWriter.Write((byte)keyEvent.Modifiers);
                keysBinaryWriter.Write(keyEvent.IsUppercase);
                keysBinaryWriter.Write(keyEvent.WasInjected);
            }
        }

        //Remove old project.
        if (deleteOld)
            Directory.Delete(path, true);

        return project;
    }
}