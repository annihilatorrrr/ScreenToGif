using ScreenToGif.Domain.Enums;

namespace ScreenToGif.Domain.Models.Project.Cached.Sequences.SubSequences;

public class FrameSubSequence : RasterSubSequence
{
    /// <summary>
    /// The position of the data stream after the headers of this sub sequence.
    /// The size of the headers is 55 bytes.
    /// </summary>
    public override long DataStreamPosition => StreamPosition + 55;

    public FrameSubSequence()
    {
        Type = SubSequenceTypes.Frame;
    }
}