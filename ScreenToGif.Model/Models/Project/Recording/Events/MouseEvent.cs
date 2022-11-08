namespace ScreenToGif.Domain.Models.Project.Recording.Events;

public abstract class MouseEvent : RecordingEvent
{
    /// <summary>
    /// Horizontal axis position.
    /// </summary>
    public int Left { get; set; }

    /// <summary>
    /// Vertical axis position.
    /// </summary>
    public int Top { get; set; }
}