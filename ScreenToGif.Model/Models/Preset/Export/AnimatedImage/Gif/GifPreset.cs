using ScreenToGif.Domain.Enums;

namespace ScreenToGif.Domain.Models.Preset.Export.AnimatedImage.Gif;

public class GifPreset : AnimatedImagePreset
{
    public GifPreset()
    {
        Type = ExportFormats.Gif;
    }
}