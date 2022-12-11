using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Models.Project.Recording;
using ScreenToGif.Domain.Models.Project.Recording.Events;
using ScreenToGif.Util.Settings;
using System.IO;
using System.Text;
using System.Windows.Input;

namespace ScreenToGif.Util.Project;

public static class RecordingProjectHelper
{
    public static RecordingProject Create(ProjectSources source)
    {
        var date = DateTime.Now;
        var path = Path.Combine(UserSettings.All.TemporaryFolderResolved, "ScreenToGif", "Recordings", date.ToString("yyyy-MM-dd HH-mm-ss"));

        var project = new RecordingProject
        {
            RootCachePath = path,
            PropertiesCachePath = Path.Combine(path, "Properties.cache"),
            FramesCachePath = Path.Combine(path, "Frames.cache"),
            MouseEventsCachePath = Path.Combine(path, "MouseEvents.cache"),
            KeyboardEventsCachePath = Path.Combine(path, "KeyboardEvents.cache"),
            CreatedBy = source,
            CreationDate = date
        };

        Directory.CreateDirectory(path);
        
        return project;
    }

    public static void WritePropertiesToDisk(this RecordingProject project)
    {
        using var fileStream = new FileStream(project.PropertiesCachePath, FileMode.Create, FileAccess.Write, FileShare.None);
        using var binaryWriter = new ExBinaryWriter(fileStream);

        binaryWriter.Write(Encoding.ASCII.GetBytes("stgR")); //Signature, 4 bytes.
        binaryWriter.Write((ushort)1); //File version, 2 bytes.
        binaryWriter.Write((ushort) project.Width); //Width, 2 bytes.
        binaryWriter.Write((ushort) project.Height); //Height, 2 bytes.
        binaryWriter.Write(BitConverter.GetBytes(Convert.ToSingle(project.Dpi))); //DPI, 4 bytes.
        binaryWriter.Write(project.ChannelCount); //Number of channels, 1 byte.
        binaryWriter.Write(project.BitsPerChannel); //Bits per channels, 1 byte.
        binaryWriter.WritePascalString("ScreenToGif"); //App name, 1 byte + X bytes (255 max).
        binaryWriter.WritePascalString(UserSettings.All.VersionText); //App version, 1 byte + X bytes (255 max).
        binaryWriter.Write((byte) project.CreatedBy); //Recording source, 1 byte.
        binaryWriter.Write(project.CreationDate.Ticks); //Creation date, 8 bytes.
    }

    public static void WriteEmptyFrameSequenceHeader(this RecordingProject project)
    {
        using var fileStream = new FileStream(project.FramesCachePath, FileMode.Create, FileAccess.Write, FileShare.None);
        using var binaryWriter = new ExBinaryWriter(fileStream);

        //Sequence details.
        binaryWriter.Write((ushort)1); //2 bytes, ID.
        binaryWriter.Write((byte)SequenceTypes.Frame); //1 bytes.
        binaryWriter.Write(0); //8 bytes, start time in ticks.
        binaryWriter.Write(0); //8 bytes, end time in ticks (unknown for now).
        binaryWriter.Write(BitConverter.GetBytes(1F)); //4 bytes, opacity.
        binaryWriter.Write(0); //4 bytes, no background.

        //Sequence effects.
        binaryWriter.Write((byte)0); //Effect count, 1 bytes.

        //Rect sequence.
        binaryWriter.Write(0); //4 bytes, left/X.
        binaryWriter.Write(0); //4 bytes, top/Y.
        binaryWriter.Write((ushort)project.Width); //2 bytes.
        binaryWriter.Write((ushort)project.Height); //2 bytes.
        binaryWriter.Write(BitConverter.GetBytes(0F)); //4 bytes, angle.

        //Raster sequence. Should it be type of raster?
        binaryWriter.Write((byte)RasterSequenceSources.Screen); //1 byte.
        binaryWriter.Write((ushort)project.Width); //2 bytes.
        binaryWriter.Write((ushort)project.Height); //2 bytes.
        binaryWriter.WriteTwice(BitConverter.GetBytes(Convert.ToSingle(project.Dpi))); //4 bytes.
        binaryWriter.WriteTwice(BitConverter.GetBytes(Convert.ToSingle(project.Dpi))); //4 bytes.
        binaryWriter.Write(project.ChannelCount); //1 byte.
        binaryWriter.Write(project.BitsPerChannel); //1 byte.
        binaryWriter.Write((uint)0); //4 byte, frame count (unknown for now).
    }

    public static RecordingProject ReadFromPath(string path)
    {
        var propertiesPath = Path.Combine(path, "Properties.cache");
        var framesPath = Path.Combine(path, "Frames.cache");
        var strokesPath = Path.Combine(path, "Strokes.cache");
        var cursorEventsPath = Path.Combine(path, "MouseEvents.cache");
        var keyEventsPath = Path.Combine(path, "KeyboardEvents.cache");

        //Properties.
        using var readStream = new FileStream(propertiesPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var bynaryReader = new BinaryReader(readStream);

        var sign = Encoding.ASCII.GetString(bynaryReader.ReadBytes(4)); //Signature, 4 bytes.

        if (sign != "stgR") //Signature, 4 bytes.
            throw new Exception($"Unsupported file format. Signature: '{sign}'.");

        var version = bynaryReader.ReadUInt16(); //File version, 2 bytes.

        if (version != 1)
            throw new Exception($"Unsupported file version. Version: '{version}'.");

        var project = new RecordingProject
        {
            RootCachePath = path,
            PropertiesCachePath = propertiesPath,
            FramesCachePath = framesPath,
            //StrokesCachePath = strokesPath,
            MouseEventsCachePath = cursorEventsPath,
            KeyboardEventsCachePath = keyEventsPath
        };

        project.Width = bynaryReader.ReadUInt16(); //Width, 2 bytes.
        project.Height = bynaryReader.ReadUInt16(); //Height, 2 bytes.
        project.Dpi = Convert.ToDouble(bynaryReader.ReadSingle()); //DPI, 4 bytes.
        project.ChannelCount = bynaryReader.ReadByte(); //Number of channels, 1 byte.
        project.BitsPerChannel = bynaryReader.ReadByte(); //Bits per channels, 1 byte.
        
        readStream.ReadPascalString(); //App name, 1 byte + X bytes (255 max).
        readStream.ReadPascalString(); //App version, 1 byte + X bytes (255 max).

        project.CreatedBy = (ProjectSources)bynaryReader.ReadByte(); //Recording source, 1 byte.
        project.CreationDate = new DateTime(bynaryReader.ReadInt64()); //Creation date, 8 bytes.

        //Frames.
        if (File.Exists(framesPath))
        {
            using var readFramesStream = new FileStream(framesPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var binaryReader = new BinaryReader(readFramesStream);

            readFramesStream.Position += 63;

            while (readFramesStream.Position < readFramesStream.Length)
            {
                var frame = new RecordingFrame
                {
                    StreamPosition = readFramesStream.Position
                };

                readFramesStream.Position += 1;
                
                frame.TimeStampInTicks = binaryReader.ReadInt64();
                frame.ExpectedDelay = binaryReader.ReadInt32();

                readFramesStream.Position += 30;

                frame.DataLength = binaryReader.ReadInt64();
                frame.CompressedDataLength = binaryReader.ReadInt64();
                var currentPosition = readFramesStream.Position;

                //using (var compressStream = new DeflateStream(readFramesStream, CompressionMode.Decompress, true))
                //    compressStream.ReadBytesUntilFull(frame.DataLength);
                
                readFramesStream.Position = currentPosition + frame.CompressedDataLength;

                project.Frames.Add(frame);
            }
        }

        if (File.Exists(strokesPath))
        {
            using var readFramesStream = new FileStream(strokesPath, FileMode.Open, FileAccess.Read, FileShare.Read);

            //TODO: Read strokes.
            //How are they going to get stored?
        }

        if (File.Exists(cursorEventsPath))
        {
            using var readFramesStream = new FileStream(cursorEventsPath, FileMode.Open, FileAccess.Read, FileShare.Read);

            //TODO: Read is wrong

            while (readFramesStream.Position < readFramesStream.Length)
            {
                if (readFramesStream.ReadByte() == 0) //Event Type
                {
                    //Cursor
                    var cursor = new CursorEvent();
                    cursor.StreamPosition = readFramesStream.Position - 1;
                    cursor.TimeStampInTicks = readFramesStream.ReadInt64();
                    cursor.Left = readFramesStream.ReadInt32();
                    cursor.Top = readFramesStream.ReadInt32();
                    cursor.LeftButton = readFramesStream.ReadByte() == 1 ? MouseButtonState.Pressed : MouseButtonState.Released;
                    cursor.RightButton = readFramesStream.ReadByte() == 1 ? MouseButtonState.Pressed : MouseButtonState.Released;
                    cursor.MiddleButton = readFramesStream.ReadByte() == 1 ? MouseButtonState.Pressed : MouseButtonState.Released;
                    cursor.FirstExtraButton = readFramesStream.ReadByte() == 1 ? MouseButtonState.Pressed : MouseButtonState.Released;
                    cursor.SecondExtraButton = readFramesStream.ReadByte() == 1 ? MouseButtonState.Pressed : MouseButtonState.Released;
                    cursor.MouseDelta = readFramesStream.ReadInt16();

                    project.MouseEvents.Add(cursor);
                }
                else
                {
                    //Cursor data.
                    var cursorData = new CursorDataEvent();
                    cursorData.StreamPosition = readFramesStream.Position - 1;
                    cursorData.TimeStampInTicks = readFramesStream.ReadInt64();
                    cursorData.CursorType = readFramesStream.ReadByte();
                    cursorData.Left = readFramesStream.ReadInt32();
                    cursorData.Top = readFramesStream.ReadInt32();

                    cursorData.Width = readFramesStream.ReadInt32();
                    cursorData.Height = readFramesStream.ReadInt32();
                    cursorData.XHotspot = readFramesStream.ReadInt32();
                    cursorData.YHotspot = readFramesStream.ReadInt32();
                    cursorData.PixelsLength = readFramesStream.ReadInt64();
                    readFramesStream.Position += cursorData.PixelsLength;

                    project.MouseEvents.Add(cursorData);
                }
            }
        }

        if (File.Exists(keyEventsPath))
        {
            using var readFramesStream = new FileStream(keyEventsPath, FileMode.Open, FileAccess.Read, FileShare.Read);

            if (readFramesStream.Length > 0)
            {
                while (readFramesStream.Position < readFramesStream.Length)
                {
                    var key = new KeyEvent();
                    key.StreamPosition = readFramesStream.Position;
                    readFramesStream.Position += 1; //Type.
                    key.TimeStampInTicks = readFramesStream.ReadInt64();
                    key.Key = (Key)readFramesStream.ReadInt32();
                    key.Modifiers = (ModifierKeys)readFramesStream.ReadByte();
                    key.IsUppercase = readFramesStream.ReadByte() == 1;
                    key.WasInjected = readFramesStream.ReadByte() == 1;

                    project.KeyboardEvents.Add(key);
                }
            }
        }

        return project;
    }

    public static bool Discard(this RecordingProject project)
    {
        try
        {
            File.Delete(project.PropertiesCachePath);
            File.Delete(project.FramesCachePath);
            File.Delete(project.MouseEventsCachePath);
            File.Delete(project.KeyboardEventsCachePath);
            File.Delete(project.RootCachePath);

            project.Frames.Clear();
            project.MouseEvents.Clear();
            project.KeyboardEvents.Clear();

            return true;
        }
        catch (Exception e)
        {
            LogWriter.Log(e, "Not possible to discard the recording");
            return true;
        }
    }
}