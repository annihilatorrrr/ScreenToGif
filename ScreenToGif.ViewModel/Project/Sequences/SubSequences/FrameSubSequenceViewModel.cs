using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Project.Cached.Sequences.SubSequences;
using ScreenToGif.Domain.Models.Project.Recording;

namespace ScreenToGif.ViewModel.Project.Sequences.SubSequences;

public class FrameSubSequenceViewModel : RasterSubSequenceViewModel
{
    private long _delay; //TODO: remove
    
    /// <summary>
    /// Frame delay in milliseconds.
    /// </summary>
    public long Delay
    {
        get => _delay;
        set => SetProperty(ref _delay, value);
    }

    /// <summary>
    /// The position of the data stream after the headers of this sub sequence.
    /// The size of the headers is 55 bytes.
    /// </summary>
    public override ulong DataStreamPosition => StreamPosition + 47; //55;

    public static FrameSubSequenceViewModel FromModel(FrameSubSequence sequence, IEditorViewModel baseViewModel)
    {
        return new FrameSubSequenceViewModel
        {
            TimeStampInTicks = sequence.TimeStampInTicks,
            StreamPosition = sequence.StreamPosition,
            Left = sequence.Left,
            Top = sequence.Top,
            Width = sequence.Width,
            Height = sequence.Height,
            Angle = sequence.Angle,
            OriginalWidth = sequence.OriginalWidth,
            OriginalHeight = sequence.OriginalHeight,
            HorizontalDpi = sequence.HorizontalDpi,
            VerticalDpi = sequence.VerticalDpi,
            ChannelCount = sequence.ChannelCount,
            BitsPerChannel = sequence.BitsPerChannel,
            DataLength = sequence.DataLength
        };
    }

    public static FrameSubSequenceViewModel FromModel(RecordingProject project, RecordingFrame frame, IEditorViewModel baseViewModel)
    {
        return new FrameSubSequenceViewModel
        {
            TimeStampInTicks = (ulong)frame.Ticks,
            StreamPosition = frame.StreamPosition,
            Left = 0,
            Top = 0,
            Width = (ushort)project.Width,
            Height = (ushort)project.Height,
            Angle = 0,
            OriginalWidth = (ushort)project.Width,
            OriginalHeight = (ushort)project.Height,
            HorizontalDpi = project.Dpi,
            VerticalDpi = project.Dpi,
            ChannelCount = project.ChannelCount,
            BitsPerChannel = project.BitsPerChannel,
            DataLength = frame.DataLength
        };
    }
}