using ScreenToGif.Domain.Enums;

namespace ScreenToGif.Domain.Models.Preset.Export.AnimatedImage.Gif;

public class SystemGifPreset : GifPreset
{
    public SystemGifPreset()
    {
        Encoder = EncoderTypes.System;
    }
}