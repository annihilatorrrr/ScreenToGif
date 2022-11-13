using ScreenToGif.Domain.Enums;

namespace ScreenToGif.Domain.Models.Preset.Export.Image;

public class PngPreset : ImagePreset
{
    public PngPreset()
    {
        Type = ExportFormats.Png;
    }
}