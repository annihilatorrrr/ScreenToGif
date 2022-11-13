using ScreenToGif.Domain.Enums;

namespace ScreenToGif.Domain.Models.Preset.Export.Video.Webm;

public class WebmPreset : VideoPreset
{
    public WebmPreset()
    {
        Type = ExportFormats.Webm;
    }
}