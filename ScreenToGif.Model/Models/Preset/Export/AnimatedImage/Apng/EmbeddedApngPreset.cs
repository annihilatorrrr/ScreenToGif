using ScreenToGif.Domain.Enums;

namespace ScreenToGif.Domain.Models.Preset.Export.AnimatedImage.Apng;

public class EmbeddedApngPreset : ApngPreset
{
    public bool DetectUnchanged { get; set; }

    public bool PaintTransparent { get; set; }
    
    public EmbeddedApngPreset()
    {
        Encoder = EncoderTypes.ScreenToGif;
    }
}