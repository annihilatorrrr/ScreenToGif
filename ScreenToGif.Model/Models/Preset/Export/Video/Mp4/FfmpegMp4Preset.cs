using ScreenToGif.Domain.Enums;

namespace ScreenToGif.Domain.Models.Preset.Export.Video.Mp4;

public class FfmpegMp4Preset : Mp4Preset
{
    public FfmpegMp4Preset()
    {
        Encoder = EncoderTypes.FFmpeg;
    }
}