using ScreenToGif.Domain.Models.Project.Cached.Sequences.SubSequences;

namespace ScreenToGif.ViewModel.Project.Sequences.SubSequences;

public class KeySubSequenceViewModel : SubSequenceViewModel
{
    public static KeySubSequenceViewModel FromModel(KeySubSequence sequence)
    {
        return new KeySubSequenceViewModel
        {
            Type = sequence.Type,
            TimeStampInTicks = sequence.TimeStampInTicks,
            StreamPosition = sequence.StreamPosition,
            //Left = sequence.Left,
            //Top = sequence.Top,
            //Width = sequence.Width,
            //Height = sequence.Height,
            //Angle = sequence.Angle,
            //OriginalWidth = sequence.OriginalWidth,
            //OriginalHeight = sequence.OriginalHeight,
            //HorizontalDpi = sequence.HorizontalDpi,
            //VerticalDpi = sequence.VerticalDpi,
            //ChannelCount = sequence.ChannelCount,
            //BitsPerChannel = sequence.BitsPerChannel,
            //DataLength = sequence.DataLength,
            //CursorType = sequence.CursorType,
            //XHotspot = sequence.XHotspot,
            //YHotspot = sequence.YHotspot,
            //IsLeftButtonDown = sequence.IsLeftButtonDown,
            //IsRightButtonDown = sequence.IsRightButtonDown,
            //IsMiddleButtonDown = sequence.IsMiddleButtonDown,
            //IsFirstExtraButtonDown = sequence.IsFirstExtraButtonDown,
            //IsSecondExtraButtonDown = sequence.IsSecondExtraButtonDown
        };
    }
}
