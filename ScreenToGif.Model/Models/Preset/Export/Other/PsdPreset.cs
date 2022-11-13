using ScreenToGif.Domain.Enums;

namespace ScreenToGif.Domain.Models.Preset.Export.Other;

public class PsdPreset : ExportPreset
{
    public bool CompressImage { get; set; }

    public bool SaveTimeline { get; set; }

    public bool MaximizeCompatibility { get; set; }

    public PsdPreset()
    {
        Type = ExportFormats.Psd;
    }
}