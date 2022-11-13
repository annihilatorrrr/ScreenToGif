using ScreenToGif.Domain.Enums;

namespace ScreenToGif.Domain.Models.Preset.Export.Video.Mov;

public class FfmpegMovPreset : MovPreset
{
    public FfmpegMovPreset()
    {
        Encoder = EncoderTypes.FFmpeg;
    }
}