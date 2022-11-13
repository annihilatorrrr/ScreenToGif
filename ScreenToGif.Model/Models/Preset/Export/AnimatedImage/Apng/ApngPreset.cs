using ScreenToGif.Domain.Enums;

namespace ScreenToGif.Domain.Models.Preset.Export.AnimatedImage.Apng;

public class ApngPreset : AnimatedImagePreset
{
    public ApngPreset()
    {
        Type = ExportFormats.Apng;
        DefaultExtension = ".apng";
        Extension = ".apng";
    }
}