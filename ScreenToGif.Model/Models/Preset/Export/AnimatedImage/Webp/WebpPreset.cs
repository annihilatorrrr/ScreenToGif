using ScreenToGif.Domain.Enums;

namespace ScreenToGif.Domain.Models.Preset.Export.AnimatedImage.Webp;

public class WebpPreset : AnimatedImagePreset
{
    public WebpPreset()
    {
        Type = ExportFormats.Webp;
    }
}