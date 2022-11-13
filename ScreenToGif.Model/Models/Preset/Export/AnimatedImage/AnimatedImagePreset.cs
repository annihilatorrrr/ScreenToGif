namespace ScreenToGif.Domain.Models.Preset.Export.AnimatedImage;

public class AnimatedImagePreset : ExportPreset
{
    public bool Looped { get; set; }

    public bool RepeatForever { get; set; }

    public int RepeatCount { get; set; }
}