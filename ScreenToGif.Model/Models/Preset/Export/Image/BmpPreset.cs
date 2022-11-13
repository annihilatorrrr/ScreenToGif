using ScreenToGif.Domain.Enums;

namespace ScreenToGif.Domain.Models.Preset.Export.Image;

public class BmpPreset : ImagePreset
{
    public BmpPreset()
    {
        Type = ExportFormats.Bmp;
    }
}