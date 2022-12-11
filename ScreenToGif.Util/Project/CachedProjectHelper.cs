using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Models.Project.Cached;
using ScreenToGif.Domain.Models.Project.Cached.Sequences;
using ScreenToGif.Domain.Models.Project.Cached.Sequences.SubSequences;
using ScreenToGif.Domain.Models.Project.Recording;
using ScreenToGif.Domain.Models.Project.Recording.Events;
using ScreenToGif.Util.Settings;
using System.IO;
using System.Text;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;

namespace ScreenToGif.Util.Project;

public static class CachedProjectHelper
{
    public static CachedProject Create(DateTime? creationDate = null)
    {
        var date = creationDate ?? DateTime.Now;
        var path = Path.Combine(UserSettings.All.TemporaryFolderResolved, "ScreenToGif", "Projects", date.ToString("yyyy-MM-dd HH-mm-ss"));

        //What else create paths for?

        var project = new CachedProject
        {
            CacheRootPath = path,
            PropertiesCachePath = Path.Combine(path, "Properties.cache"),
            UndoCachePath = Path.Combine(path, "Undo.cache"),
            RedoCachePath = Path.Combine(path, "Redo.cache"),

            CreationDate = date,
            LastModificationDate = date
        };

        Directory.CreateDirectory(path);

        //TODO: How am I going to store the action stack, as a single cache?
        //Makes sense as it's a stack, LiFo, I just need a file specification for the actions that wil be stored.
        //Some actions have frame data, others just action parameters.

        return project;
    }

    public static async Task<Track> CreateTrack(CachedProject project, string name)
    {
        var trackId = project.Tracks.Count + 1;

        var track = new Track
        {
            Id = (ushort)trackId,
            Name = name,
            CachePath = Path.Combine(project.CacheRootPath, $"Track-{trackId}.cache")
        };

        await using var trackStream = new FileStream(track.CachePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await using var binaryWriter = new ExBinaryWriter(trackStream);

        //Track details.
        binaryWriter.Write(track.Id); //2 bytes.
        binaryWriter.WritePascalString(track.Name); //1 byte + (0 -> 255) bytes.
        binaryWriter.Write(track.IsVisible); //1 byte.
        binaryWriter.Write(track.IsLocked); //1 byte.
        binaryWriter.Write((ushort)1); //Sequence count, 2 bytes.

        return track;
    }

    public static async Task ConvertFrameTrack(RecordingProject recording, CachedProject project)
    {
        //Track.
        var track = await CreateTrack(project, "Frames");

        //Sequence. BUG: Sometimes, the last frame comes with Ticks == 0;
        var lastFrame = recording.Frames.LastOrDefault(l => l.TimeStampInTicks > 0) ?? recording.Frames.First();

        var sequence = new FrameSequence
        {
            Id = 1,
            StartTime = TimeSpan.Zero,
            EndTime = TimeSpan.FromTicks(lastFrame.TimeStampInTicks),
            StreamPosition = 0,
            CachePath = Path.Combine(project.CacheRootPath, $"Sequence-{track.Id}-1.cache"),
            Width = project.Width,
            Height = project.Height,
            Origin = project.CreatedBy == ProjectSources.ScreenRecorder ? RasterSequenceSources.Screen : RasterSequenceSources.Webcam,
            OriginalWidth = project.Width,
            OriginalHeight = project.Height,
            ChannelCount = project.ChannelCount,
            BitsPerChannel = project.BitsPerChannel,
            HorizontalDpi = project.HorizontalDpi,
            VerticalDpi = project.VerticalDpi
        };

        //Moves the 
        File.Move(recording.FramesCachePath, sequence.CachePath, true);

        await using var writeStream = new FileStream(sequence.CachePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);

        //Sequence details.
        writeStream.WriteUInt16(sequence.Id); //2 bytes.
        writeStream.WriteByte((byte)sequence.Type); //1 bytes.
        writeStream.WriteInt64(sequence.StartTime.Ticks); //8 bytes.
        writeStream.WriteInt64(sequence.EndTime.Ticks); //8 bytes.
        writeStream.WriteBytes(BitConverter.GetBytes(Convert.ToSingle(sequence.Opacity))); //4 bytes.
        writeStream.WritePascalStringUInt32(await sequence.Background.ToXamlStringAsync()); //4 bytes + (0 - 2^32)

        //Sequence effects.
        writeStream.WriteByte(0); //Effect count, 1 bytes.

        //Rect sequence.
        writeStream.WriteInt32(sequence.Left); //4 bytes.
        writeStream.WriteInt32(sequence.Top); //4 bytes.
        writeStream.WriteUInt16(sequence.Width); //2 bytes.
        writeStream.WriteUInt16(sequence.Height); //2 bytes.
        writeStream.WriteBytes(BitConverter.GetBytes(Convert.ToSingle(sequence.Angle))); //4 bytes.

        //Raster sequence. Should it be type of raster?
        writeStream.WriteByte((byte)sequence.Origin); //1 byte.
        writeStream.WriteUInt16(sequence.OriginalWidth); //2 bytes.
        writeStream.WriteUInt16(sequence.OriginalHeight); //2 bytes.
        writeStream.WriteBytes(BitConverter.GetBytes(Convert.ToSingle(sequence.HorizontalDpi))); //4 bytes.
        writeStream.WriteBytes(BitConverter.GetBytes(Convert.ToSingle(sequence.VerticalDpi))); //4 bytes.
        writeStream.WriteByte(sequence.ChannelCount); //1 byte.
        writeStream.WriteByte(sequence.BitsPerChannel); //1 byte.
        writeStream.WriteUInt32((uint)recording.Frames.Count); //1 byte.

        //Frames sub-sequence.
        foreach (var frame in recording.Frames)
        {
            var sub = new FrameSubSequence
            {
                TimeStampInTicks = frame.TimeStampInTicks,
                StreamPosition = writeStream.Position,
                Width = sequence.Width,
                Height = sequence.Height,
                OriginalWidth = sequence.OriginalWidth,
                OriginalHeight = sequence.OriginalHeight,
                HorizontalDpi = sequence.HorizontalDpi,
                VerticalDpi = sequence.VerticalDpi,
                ChannelCount = sequence.ChannelCount,
                BitsPerChannel = sequence.BitsPerChannel,
                DataLength = frame.DataLength,
                CompressedDataLength = frame.CompressedDataLength
            };

            sequence.Frames.Add(sub);
        }

        track.Sequences.Add(sequence);
        project.Tracks.Add(track);
    }

    public static async Task ConvertCursorTrack(RecordingProject recording, CachedProject project)
    {
        var lastEvent = recording.MouseEvents.LastOrDefault(l => l.EventType is RecordingEvents.Cursor or RecordingEvents.CursorData);

        if (lastEvent == null)
            return;

        var track = await CreateTrack(project, "Cursor Events");

        var sequence = new CursorSequence
        {
            Id = 1,
            StartTime = TimeSpan.Zero,
            EndTime = TimeSpan.FromTicks(lastEvent.TimeStampInTicks), //TODO: Decide for how long to display last cursor.
            //Opacity = 1,
            //Background = null,
            //Effects = new(),
            StreamPosition = 0,
            CachePath = Path.Combine(project.CacheRootPath, $"Sequence-{track.Id}-1.cache"),
            //Left = 0,
            //Top = 0,
            Width = project.Width,
            Height = project.Height,
            //Angle = 0
        };

        await using var writeStream = new FileStream(sequence.CachePath, FileMode.Create, FileAccess.Write, FileShare.None);
        //TODO: Use ExBinaryWriter

        //Sequence details.
        writeStream.WriteUInt16(sequence.Id); //2 bytes.
        writeStream.WriteByte((byte)sequence.Type); //1 bytes.
        writeStream.WriteInt64(sequence.StartTime.Ticks); //8 bytes.
        writeStream.WriteInt64(sequence.EndTime.Ticks); //8 bytes.
        writeStream.WriteBytes(BitConverter.GetBytes(Convert.ToSingle(sequence.Opacity))); //4 bytes.
        writeStream.WritePascalStringUInt32(await sequence.Background.ToXamlStringAsync()); //4 bytes + (0 - 2^32)

        //Sequence effects.
        writeStream.WriteByte(0); //Effect count, 1 bytes.

        //Rect sequence.
        writeStream.WriteInt32(sequence.Left); //4 bytes.
        writeStream.WriteInt32(sequence.Top); //4 bytes.
        writeStream.WriteUInt16(sequence.Width); //2 bytes.
        writeStream.WriteUInt16(sequence.Height); //2 bytes.
        writeStream.WriteBytes(BitConverter.GetBytes(Convert.ToSingle(sequence.Angle))); //4 bytes.

        //Cursor sequence.
        writeStream.WriteUInt32((uint)recording.MouseEvents.Count); //4 bytes.

        await using var readStream = new FileStream(recording.MouseEventsCachePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        var cursorData = recording.MouseEvents.OfType<CursorDataEvent>().FirstOrDefault();

        //Cursor sub-sequence.
        foreach (var entry in recording.MouseEvents)
        {
            var sub = new CursorSubSequence
            {
                TimeStampInTicks = entry?.TimeStampInTicks ?? 0,
                StreamPosition = writeStream.Position
            };

            //If only data, ignore press states
            if (entry is CursorEvent state)
            {
                sub.Left = state.Left;
                sub.Top = state.Top;
                sub.Width = (ushort)(cursorData?.Width ?? 32);
                sub.Height = (ushort)Math.Abs(cursorData?.Height ?? 32);
                sub.OriginalWidth = (ushort)(cursorData?.Width ?? 32);
                sub.OriginalHeight = (ushort)Math.Abs(cursorData?.Height ?? 32);
                sub.HorizontalDpi = 96; //How to get this information? Does it change for high DPI screens?
                sub.VerticalDpi = 96;
                sub.ChannelCount = 4;
                sub.BitsPerChannel = 8;
                sub.DataLength = (ushort)(cursorData?.PixelsLength ?? 0);
                sub.CursorType = (byte)(cursorData?.CursorType ?? 0);
                sub.XHotspot = (ushort)(cursorData?.XHotspot ?? 0);
                sub.YHotspot = (ushort)(cursorData?.YHotspot ?? 0);
                sub.IsLeftButtonDown = state.LeftButton == MouseButtonState.Pressed;
                sub.IsRightButtonDown = state.RightButton == MouseButtonState.Pressed;
                sub.IsMiddleButtonDown = state.MiddleButton == MouseButtonState.Pressed;
                sub.IsFirstExtraButtonDown = state.FirstExtraButton == MouseButtonState.Pressed;
                sub.IsSecondExtraButtonDown = state.SecondExtraButton == MouseButtonState.Pressed;
                sub.MouseWheelDelta = state.MouseDelta;
            }
            else if (entry is CursorDataEvent data)
            {
                sub.Left = data.Left;
                sub.Top = data.Top;
                sub.Width = (ushort)data.Width;
                sub.Height = (ushort)Math.Abs(data.Height);
                sub.OriginalWidth = (ushort)data.Width;
                sub.OriginalHeight = (ushort)Math.Abs(data.Height);
                sub.HorizontalDpi = 96; //How to get this information? Does it change for high DPI screens?
                sub.VerticalDpi = 96;
                sub.ChannelCount = 4;
                sub.BitsPerChannel = 8;
                sub.DataLength = (ushort)data.PixelsLength;
                sub.CursorType = (byte)data.CursorType;
                sub.XHotspot = (ushort)data.XHotspot;
                sub.YHotspot = (ushort)data.YHotspot;

                cursorData = data;
            }

            //Sub-sequence details.
            writeStream.WriteByte((byte)sub.Type); //1 byte.
            writeStream.WriteInt64(sub.TimeStampInTicks); //8 bytes.

            //Rect sub-sequence details.
            writeStream.WriteInt32(sub.Left); //4 bytes.
            writeStream.WriteInt32(sub.Top); //4 bytes.
            writeStream.WriteUInt16(sub.Width); //2 bytes.
            writeStream.WriteUInt16(sub.Height); //2 bytes.
            writeStream.WriteBytes(BitConverter.GetBytes(Convert.ToSingle(sub.Angle))); //4 bytes.

            //Raster sub-sequence details.
            writeStream.WriteUInt16(sub.OriginalWidth); //2 bytes.
            writeStream.WriteUInt16(sub.OriginalHeight); //2 bytes.
            writeStream.WriteBytes(BitConverter.GetBytes(Convert.ToSingle(sub.HorizontalDpi))); //4 bytes.
            writeStream.WriteBytes(BitConverter.GetBytes(Convert.ToSingle(sub.VerticalDpi))); //4 bytes.
            writeStream.WriteByte(sub.ChannelCount); //1 byte.
            writeStream.WriteByte(sub.BitsPerChannel); //1 byte.
            writeStream.WriteInt64(sub.DataLength); //8 bytes.

            //Cursor sub-sequence details.
            writeStream.WriteByte(sub.CursorType); //1 byte.
            writeStream.WriteUInt16(sub.XHotspot); //2 bytes.
            writeStream.WriteUInt16(sub.YHotspot); //2 bytes.
            writeStream.WriteBoolean(sub.IsLeftButtonDown); //1 byte.
            writeStream.WriteBoolean(sub.IsRightButtonDown); //1 byte.
            writeStream.WriteBoolean(sub.IsMiddleButtonDown); //1 byte.
            writeStream.WriteBoolean(sub.IsFirstExtraButtonDown); //1 byte.
            writeStream.WriteBoolean(sub.IsSecondExtraButtonDown); //1 byte.
            writeStream.WriteInt16(sub.MouseWheelDelta); //2 bytes.

            if (sub.DataStreamPosition != writeStream.Position)
                System.Diagnostics.Debugger.Break();

            //The pixel location is 42 bytes after the start of the event stream position.
            await using (var part = new SubStream(readStream, 42L + (cursorData?.StreamPosition ?? 0L), sub.DataLength))
                await part.CopyToAsync(writeStream);

            sequence.CursorEvents.Add(sub);
        }

        track.Sequences.Add(sequence);
        project.Tracks.Add(track);
    }

    public static async Task ConvertKeyTrack(RecordingProject recording, CachedProject project)
    {
        var lastEvent = recording.KeyboardEvents.LastOrDefault();

        if (lastEvent == null)
            return;

        var track = await CreateTrack(project, "Key Events");

        var sequence = new KeySequence
        {
            Id = 1,
            Width = project.Width,
            Height = project.Height,
            StartTime = TimeSpan.Zero,
            EndTime = TimeSpan.FromTicks(lastEvent.TimeStampInTicks),
            CachePath = Path.Combine(project.CacheRootPath, $"Sequence-{track.Id}-1.cache")
        };

        await using var writeStream = new FileStream(sequence.CachePath, FileMode.Create, FileAccess.Write, FileShare.None);
        //TODO: Use ExBinaryWriter

        //Sequence details.
        writeStream.WriteUInt16(sequence.Id); //2 bytes.
        writeStream.WriteByte((byte)sequence.Type); //1 bytes.
        writeStream.WriteUInt64((ulong)sequence.StartTime.Ticks); //8 bytes.
        writeStream.WriteUInt64((ulong)sequence.EndTime.Ticks); //8 bytes.
        writeStream.WriteBytes(BitConverter.GetBytes(Convert.ToSingle(sequence.Opacity))); //4 bytes.
        writeStream.WritePascalStringUInt32(await sequence.Background.ToXamlStringAsync()); //4 bytes + (0 - 2^32)

        //Sequence effects.
        writeStream.WriteByte(0); //Effect count, 1 bytes.

        //Sizeable sequence.
        writeStream.WriteInt32(sequence.Left); //4 bytes.
        writeStream.WriteInt32(sequence.Top); //4 bytes.
        writeStream.WriteUInt16(sequence.Width); //2 bytes.
        writeStream.WriteUInt16(sequence.Height); //2 bytes.
        writeStream.WriteBytes(BitConverter.GetBytes(Convert.ToSingle(sequence.Angle))); //4 bytes.

        //Key sequence.
        //TODO: More details in here.
        writeStream.WriteUInt32((uint)recording.KeyboardEvents.Count); //4 bytes.

        foreach (var keyEvent in recording.KeyboardEvents)
        {
            var sub = new KeySubSequence
            {
                TimeStampInTicks = keyEvent.TimeStampInTicks,
                Key = keyEvent.Key,
                Modifiers = keyEvent.Modifiers,
                IsUppercase = keyEvent.IsUppercase,
                WasInjected = keyEvent.WasInjected,
                StreamPosition = writeStream.Position
            };

            writeStream.WriteByte((byte)sub.Type); //1 byte.
            writeStream.WriteInt64(sub.TimeStampInTicks); //8 bytes.
            writeStream.WriteByte((byte)sub.Key); //1 byte.
            writeStream.WriteByte((byte)sub.Modifiers); //1 byte.
            writeStream.WriteBoolean(sub.IsUppercase); //1 byte.
            writeStream.WriteBoolean(sub.WasInjected); //1 byte.
            
            sequence.KeyEvents.Add(sub);
        }

        track.Sequences.Add(sequence);
        project.Tracks.Add(track);
    }

    public static async Task<CachedProject> ConvertToCachedProject(this RecordingProject recording)
    {
        var project = Create(recording.CreationDate);
        project.Width = (ushort)recording.Width;
        project.Height = (ushort)recording.Height;
        project.VerticalDpi = recording.Dpi;
        project.HorizontalDpi = recording.Dpi;
        project.Background = Brushes.White;
        project.ChannelCount = recording.ChannelCount;
        project.BitsPerChannel = recording.BitsPerChannel;
        project.Version = UserSettings.All.Version;
        project.CreatedBy = recording.CreatedBy;

        //Properties.
        await using var writeStream = new FileStream(project.PropertiesCachePath, FileMode.Create, FileAccess.Write, FileShare.Read);
        writeStream.WriteBytes(Encoding.ASCII.GetBytes("stgC")); //Signature, 4 bytes.
        writeStream.WriteUInt16(1); //File version, 2 bytes.
        writeStream.WriteUInt16(project.Width); //Width, 2 bytes.
        writeStream.WriteUInt16(project.Height); //Height, 2 bytes.
        writeStream.WriteBytes(BitConverter.GetBytes(Convert.ToSingle(project.HorizontalDpi))); //DPI, 4 bytes.
        writeStream.WriteBytes(BitConverter.GetBytes(Convert.ToSingle(project.VerticalDpi))); //DPI, 4 bytes.
        writeStream.WritePascalStringUInt32(await project.Background.ToXamlStringAsync()); //4 bytes + X
        writeStream.WriteByte(project.ChannelCount); //Number of channels, 1 byte.
        writeStream.WriteByte(project.BitsPerChannel); //Bits per channels, 1 byte.
        writeStream.WritePascalString("ScreenToGif"); //App name, 1 byte + X bytes (255 max).
        writeStream.WritePascalString(UserSettings.All.VersionText); //App version, 1 byte + X bytes (255 max).
        writeStream.WriteByte((byte)project.CreatedBy); //Recording source, 1 byte.
        writeStream.WriteInt64(project.CreationDate.Ticks); //Creation date, 8 bytes.
        writeStream.WritePascalString(project.Name); //Project's name, 1 byte + X bytes (255 max).
        writeStream.WritePascalStringUInt16(project.Path); //Project's last used path, 2 bytes + X bytes (32_767 max).
        await writeStream.FlushAsync();

        //Tracks (Frames and Cursor/Key events).
        if (project.CreatedBy != ProjectSources.SketchboardRecorder)
        {
            await ConvertFrameTrack(recording, project);
            await ConvertCursorTrack(recording, project);
            await ConvertKeyTrack(recording, project);
        }
        else
        {
            //await CreateStrokeTrack(recording.FramesCachePath, project);
        }

        await Task.Run(recording.Discard);

        return project;
    }

    public static CachedProject ReadFromPath(string path)
    {
        var propertiesPath = Path.Combine(path, "Properties.cache");

        using var readStream = new FileStream(propertiesPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new ExBinaryReader(readStream);

        var sign = Encoding.ASCII.GetString(binaryReader.ReadBytes(4)); //Signature, 4 bytes.

        if (sign != "stgC") //Signature, 4 bytes.
            throw new Exception($"Unsupported file format. Signature: '{sign}'.");

        var version = binaryReader.ReadUInt16(); //File version, 2 bytes.

        if (version != 1)
            throw new Exception($"Unsupported file version. Version: '{version}'.");

        var project = new CachedProject
        {
            CacheRootPath = path,
            PropertiesCachePath = propertiesPath
        };

        project.Width = binaryReader.ReadUInt16(); //2 bytes.
        project.Height = binaryReader.ReadUInt16(); //2 bytes.
        project.HorizontalDpi = Convert.ToDouble(binaryReader.ReadSingle()); //4 bytes.
        project.VerticalDpi = Convert.ToDouble(binaryReader.ReadSingle()); //4 bytes.
        var backgroundXaml = binaryReader.ReadPascalStringUInt32(); //2 bytes + X bytes.
        project.Background = backgroundXaml != null ? (Brush)XamlReader.Load(new XmlTextReader(new StringReader(backgroundXaml))) : null;
        project.Background?.Freeze();

        project.ChannelCount = binaryReader.ReadByte(); //1 byte.
        project.BitsPerChannel = binaryReader.ReadByte(); //1 byte.
        readStream.ReadPascalString(); //1 byte + X bytes (255 max).
        readStream.ReadPascalString(); //1 byte + X bytes (255 max).
        project.CreatedBy = (ProjectSources)binaryReader.ReadByte(); //1 byte.
        project.CreationDate = new DateTime(binaryReader.ReadInt64()); //8 bytes.
        project.Name = binaryReader.ReadPascalString(); //1 byte + X bytes (255 max).
        project.Path = binaryReader.ReadPascalStringUInt16(); //2 bytes + X bytes (32_767 max).

        //Tracks.
        var tracks = Directory.EnumerateFiles(path, "Track-*");

        foreach (var trackPath in tracks)
        {
            var track = ReadTrackFromPath(path, trackPath);

            if (track != null)
                project.Tracks.Add(track);
        }

        return project;
    }

    private static Track ReadTrackFromPath(string path, string trackPath)
    {
        var track = new Track
        {
            CachePath = trackPath
        };

        using var readStream = new FileStream(trackPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new ExBinaryReader(readStream);

        track.Id = binaryReader.ReadUInt16();
        track.Name = binaryReader.ReadPascalString();
        track.IsVisible = binaryReader.ReadBoolean();
        track.IsLocked = binaryReader.ReadBoolean();
        
        var sequences = Directory.EnumerateFiles(path, $"Sequence-{track.Id}-*");

        foreach (var sequencePath in sequences)
        {
            var sequence = ReadSequenceFromPath(sequencePath);

            if (sequence != null)
                track.Sequences.Add(sequence);
        }

        return track;
    }

    private static Sequence ReadSequenceFromPath(string path)
    {
        using var readStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new ExBinaryReader(readStream);

        var streamPosition = binaryReader.BaseStream.Position;
        var id = binaryReader.ReadUInt16(); //2 bytes.
        var type = (SequenceTypes)binaryReader.ReadByte(); //1 byte.
        var startTicks = binaryReader.ReadInt64(); //8 bytes.
        var endTicks = binaryReader.ReadInt64(); //8 bytes.
        var opacity = Convert.ToDouble(binaryReader.ReadSingle()); //4 bytes.
        var backgroundXaml = binaryReader.ReadPascalStringUInt32();//4 bytes + (0 - 2^32)
        var background = backgroundXaml != null ? (Brush)XamlReader.Load(new XmlTextReader(new StringReader(backgroundXaml))) : null; 
        
        switch (type)
        {
            case SequenceTypes.Brush:
            {
                //Single brush sequence.
                //Maybe replace by shape, since they will work the same.
                break;
            }

            case SequenceTypes.Frame:
            {
                var frame = new FrameSequence
                {
                    Id = id,
                    StartTime = TimeSpan.FromTicks(startTicks),
                    EndTime = TimeSpan.FromTicks(endTicks),
                    Opacity = opacity,
                    Background = background,
                    CachePath = path
                };

                frame.StreamPosition = streamPosition;
                frame.Effects = ReadEffects(binaryReader);
                frame.Left = binaryReader.ReadInt32();
                frame.Top = binaryReader.ReadInt32();
                frame.Width = binaryReader.ReadUInt16();
                frame.Height = binaryReader.ReadUInt16();
                frame.Angle = binaryReader.ReadSingle();
                frame.Origin = (RasterSequenceSources)binaryReader.ReadByte();
                frame.OriginalWidth = binaryReader.ReadUInt16();
                frame.OriginalHeight = binaryReader.ReadUInt16();
                frame.HorizontalDpi = binaryReader.ReadSingle();
                frame.VerticalDpi = binaryReader.ReadSingle();
                frame.ChannelCount = binaryReader.ReadByte();
                frame.BitsPerChannel = binaryReader.ReadByte();
                var subSequenceCount = binaryReader.ReadUInt32();
                
                //Read subsequences
                while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                {
                    var sub = new FrameSubSequence();
                    sub.StreamPosition = binaryReader.BaseStream.Position;
                    binaryReader.ReadByte(); //Type, 1 byte.
                    sub.TimeStampInTicks = binaryReader.ReadInt64();
                    sub.ExpectedDelay = binaryReader.ReadInt32();
                    sub.Left = binaryReader.ReadInt32();
                    sub.Top = binaryReader.ReadInt32();
                    sub.Width = binaryReader.ReadUInt16();
                    sub.Height = binaryReader.ReadUInt16();
                    sub.Angle = binaryReader.ReadSingle();
                    sub.OriginalWidth = binaryReader.ReadUInt16();
                    sub.OriginalHeight = binaryReader.ReadUInt16();
                    sub.HorizontalDpi = binaryReader.ReadSingle();
                    sub.VerticalDpi = binaryReader.ReadSingle();
                    sub.ChannelCount = binaryReader.ReadByte();
                    sub.BitsPerChannel = binaryReader.ReadByte();
                    
                    sub.DataLength = binaryReader.ReadInt64(); //8 bytes, uncompressed data length.
                    sub.CompressedDataLength = binaryReader.ReadInt64(); //8 bytes, compressed data length.

                    binaryReader.BaseStream.Position += sub.CompressedDataLength;

                    frame.Frames.Add(sub);
                }

                return frame;
            }

            case SequenceTypes.Cursor:
            {
                var cursor = new CursorSequence
                {
                    Id = id,
                    StartTime = TimeSpan.FromTicks(startTicks),
                    EndTime = TimeSpan.FromTicks(endTicks),
                    Opacity = opacity,
                    Background = background,
                    CachePath = path
                };

                cursor.StreamPosition = streamPosition;
                cursor.Effects = ReadEffects(binaryReader);
                cursor.Left = binaryReader.ReadInt32();
                cursor.Top = binaryReader.ReadInt32();
                cursor.Width = binaryReader.ReadUInt16();
                cursor.Height = binaryReader.ReadUInt16();
                cursor.Angle = binaryReader.ReadSingle();
                var subSequenceCount = binaryReader.ReadUInt32();

                //Read subsequences
                while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                {
                    var sub = new CursorSubSequence();
                    sub.StreamPosition = binaryReader.BaseStream.Position;
                    binaryReader.ReadByte(); //Type, 1 byte.
                    sub.TimeStampInTicks = binaryReader.ReadInt64();
                    sub.Left = binaryReader.ReadInt32();
                    sub.Top = binaryReader.ReadInt32();
                    sub.Width = binaryReader.ReadUInt16();
                    sub.Height = binaryReader.ReadUInt16();
                    sub.Angle = binaryReader.ReadSingle();
                    sub.OriginalWidth = binaryReader.ReadUInt16();
                    sub.OriginalHeight = binaryReader.ReadUInt16();
                    sub.HorizontalDpi = binaryReader.ReadSingle();
                    sub.VerticalDpi = binaryReader.ReadSingle();
                    sub.ChannelCount = binaryReader.ReadByte();
                    sub.BitsPerChannel = binaryReader.ReadByte();
                    sub.DataLength = binaryReader.ReadInt64(); //8 bytes, uncompressed data length.
                    sub.CursorType = binaryReader.ReadByte();
                    sub.XHotspot = binaryReader.ReadUInt16();
                    sub.YHotspot = binaryReader.ReadUInt16();
                    sub.IsLeftButtonDown = binaryReader.ReadBoolean();
                    sub.IsRightButtonDown = binaryReader.ReadBoolean();
                    sub.IsMiddleButtonDown = binaryReader.ReadBoolean();
                    sub.IsFirstExtraButtonDown = binaryReader.ReadBoolean();
                    sub.IsSecondExtraButtonDown = binaryReader.ReadBoolean();
                    sub.MouseWheelDelta = binaryReader.ReadInt16();

                    binaryReader.BaseStream.Position += sub.DataLength;

                    cursor.CursorEvents.Add(sub);
                }

                return cursor;
            }

            case SequenceTypes.Key:
            {
                var key = new KeySequence
                {
                    Id = id,
                    StartTime = TimeSpan.FromTicks(startTicks),
                    EndTime = TimeSpan.FromTicks(endTicks),
                    Opacity = opacity,
                    Background = background,
                    CachePath = path
                };

                key.StreamPosition = streamPosition;
                key.Effects = ReadEffects(binaryReader);
                key.Left = binaryReader.ReadInt32();
                key.Top = binaryReader.ReadInt32();
                key.Width = binaryReader.ReadUInt16();
                key.Height = binaryReader.ReadUInt16();
                key.Angle = binaryReader.ReadSingle();
                var subSequenceCount = binaryReader.ReadUInt32();

                //Read subsequences
                while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                {
                    var sub = new KeySubSequence();
                    sub.StreamPosition = binaryReader.BaseStream.Position;
                    binaryReader.ReadByte(); //Type, 1 byte.
                    sub.TimeStampInTicks = binaryReader.ReadInt64();
                    sub.Key = (Key)binaryReader.ReadByte();
                    sub.Modifiers = (ModifierKeys)binaryReader.ReadByte();
                    sub.IsUppercase = binaryReader.ReadBoolean();
                    sub.WasInjected = binaryReader.ReadBoolean();
                    
                    key.KeyEvents.Add(sub);
                }

                return key;
            }

            case SequenceTypes.Text:
                break;
            case SequenceTypes.Shape:
                break;
            case SequenceTypes.Drawing:
                break;
            case SequenceTypes.Progress:
                break;
            case SequenceTypes.Obfuscation:
                break;
            case SequenceTypes.Image:
                break;
            case SequenceTypes.TitleFrame:
                break;
            case SequenceTypes.Cinemagraph:
                break;
        }

        return null;
    }

    private static List<object> ReadEffects(BinaryReader reader)
    {
        reader.ReadByte();

        //TODO: Read effect count and parse each effect.

        return new List<object>();
    }

    //Discard?

    //Save to StorageProject.
}