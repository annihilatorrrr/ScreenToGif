using ScreenToGif.Domain.Enums;

namespace ScreenToGif.Domain.Models.Preset.Export.Video.Avi;

public class AviPreset : VideoPreset
{
    public AviPreset()
    {
        Type = ExportFormats.Avi;
    }
}