using ScreenToGif.Domain.Enums;

namespace ScreenToGif.Domain.Models.Preset.Export.Image;

public class JpegPreset : ImagePreset
{
    public JpegPreset()
    {
        Type = ExportFormats.Jpeg;
    }
}