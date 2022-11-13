using ScreenToGif.Domain.Enums;

namespace ScreenToGif.Domain.Models.Preset.Export.Video.Mkv;

public class MkvPreset : VideoPreset
{
    public MkvPreset()
    {
        Type = ExportFormats.Mkv;
    }
}