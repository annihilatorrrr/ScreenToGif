using ScreenToGif.Domain.Enums;

namespace ScreenToGif.Domain.Models.Preset.Export.Video.Mov;

public class MovPreset : VideoPreset
{
    public MovPreset()
    {
        Type = ExportFormats.Mov;
    }
}