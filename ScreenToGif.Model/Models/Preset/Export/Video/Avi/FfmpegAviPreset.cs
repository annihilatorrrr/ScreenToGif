using ScreenToGif.Domain.Enums;

namespace ScreenToGif.Domain.Models.Preset.Export.Video.Avi;

public class FfmpegAviPreset : AviPreset
{
    public FfmpegAviPreset()
    {
        Encoder = EncoderTypes.FFmpeg;
    }
}