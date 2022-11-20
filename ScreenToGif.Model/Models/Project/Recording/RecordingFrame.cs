namespace ScreenToGif.Domain.Models.Project.Recording;

public class RecordingFrame
{
    /// <summary>
    /// The position of the frame in the stream.
    /// </summary>
    public long StreamPosition { get; set; }

    /// <summary>
    /// Ticks since the start of the recording.
    /// </summary>
    public long TimeStampInTicks { get; set; }
    
    /// <summary>
    /// The capture content.
    /// </summary>
    public byte[] Pixels { get; set; }

    /// <summary>
    /// The number of bytes of the capture content.
    /// </summary>
    public long DataLength { get; set; }

    /// <summary>
    /// For some reason, the frame capture failed.
    /// </summary>
    public bool WasFrameSkipped { get; set; }
}