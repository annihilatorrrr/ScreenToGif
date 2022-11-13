using ScreenToGif.Domain.Enums;

namespace ScreenToGif.Domain.Models.Preset.Export.Video.Mkv;

public class FfmpegMkvPreset : MkvPreset
{
    public FfmpegMkvPreset()
    {
        Encoder = EncoderTypes.FFmpeg;
    }
}