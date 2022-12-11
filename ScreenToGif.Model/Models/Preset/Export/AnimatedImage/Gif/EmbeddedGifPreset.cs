using ScreenToGif.Domain.Enums;
using System.Windows.Media;

namespace ScreenToGif.Domain.Models.Preset.Export.AnimatedImage.Gif;

public class EmbeddedGifPreset: GifPreset
{
    public bool UseGlobalColorTable { get; set; }
    
    public ColorQuantizationTypes Quantizer { get; set; }

    public int SamplingFactor { get; set; }

    public int MaximumColorCount { get; set; }

    public bool EnableTransparency { get; set; }

    public bool SelectTransparencyColor { get; set; }

    public Color TransparencyColor { get; set; }

    public bool DetectUnchanged { get; set; }

    public bool PaintTransparent { get; set; }

    public Color ChromaKey { get; set; }
    
    public EmbeddedGifPreset()
    {
        Encoder = EncoderTypes.ScreenToGif;
    }
}