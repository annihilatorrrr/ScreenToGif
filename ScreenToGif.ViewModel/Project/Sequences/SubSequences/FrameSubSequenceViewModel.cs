using ScreenToGif.Domain.Models.Project.Cached.Sequences.SubSequences;
using ScreenToGif.Domain.Models.Project.Recording;

namespace ScreenToGif.ViewModel.Project.Sequences.SubSequences;

public class FrameSubSequenceViewModel : RasterSubSequenceViewModel
{
    /// <summary>
    /// The position of the data stream after the headers of this sub sequence.
    /// The size of the headers is 55 bytes.
    /// </summary>
    public override long DataStreamPosition => StreamPosition + 51; //55;

    public static FrameSubSequenceViewModel FromModel(FrameSubSequence sequence)
    {
        return new FrameSubSequenceViewModel
        {
            TimeStampInTicks = sequence.TimeStampInTicks,
            ExpectedDelay = sequence.ExpectedDelay,
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
            DataLength = sequence.DataLength,
        };
    }

    public static FrameSubSequenceViewModel FromModel(RecordingProject project, RecordingFrame frame, bool adjustTiming)
    {
        return new FrameSubSequenceViewModel
        {
            TimeStampInTicks = adjustTiming ? 0 : frame.TimeStampInTicks,
            ExpectedDelay = frame.ExpectedDelay,
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
            DataLength = frame.DataLength,
        };
    }
}