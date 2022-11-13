using ScreenToGif.Domain.Enums;

namespace ScreenToGif.Domain.Models.Preset.Export.AnimatedImage.Gif;

public class GifPreset : AnimatedImagePreset
{
    public bool UseGlobalColorTable { get; set; }

    public GifPreset()
    {
        Type = ExportFormats.Gif;
    }
}