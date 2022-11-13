using ScreenToGif.Domain.Enums;

namespace ScreenToGif.Domain.Models.Preset.Export.Video.Mp4;

public class Mp4Preset : VideoPreset
{
    public Mp4Preset()
    {
        Type = ExportFormats.Mp4;
    }
}