using ScreenToGif.Domain.Enums;

namespace ScreenToGif.Domain.Models.Preset.Export.Video.Webm;

public class FfmpegWebmPreset : WebmPreset
{
    public FfmpegWebmPreset()
    {
        Encoder = EncoderTypes.FFmpeg;
    }
}