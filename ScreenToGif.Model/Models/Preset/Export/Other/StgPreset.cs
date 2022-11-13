using ScreenToGif.Domain.Enums;
using System.IO.Compression;

namespace ScreenToGif.Domain.Models.Preset.Export.Other;

public class StgPreset : ExportPreset
{
    public CompressionLevel CompressionLevel { get; set; }

    public StgPreset()
    {
        Type = ExportFormats.Stg;
    }
}