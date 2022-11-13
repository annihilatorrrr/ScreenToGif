using ScreenToGif.Domain.Enums;

namespace ScreenToGif.Domain.Models.Preset.Export.AnimatedImage.Gif;

public class GifskiGifPreset : GifPreset
{
    public bool Fast { get; set; }

    public int Quality { get; set; }
    
    public GifskiGifPreset()
    {
        Encoder = EncoderTypes.Gifski;
    }
}