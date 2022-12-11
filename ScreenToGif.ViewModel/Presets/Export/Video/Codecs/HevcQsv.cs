using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Models;

namespace ScreenToGif.ViewModel.Presets.Export.Video.Codecs;

public class HevcQsv : VideoCodecViewModel
{
    public HevcQsv()
    {
        Type = VideoCodecs.HevcQsv;
        Name = "HEVC QSV";
        Command = "hevc_qsv";

        IsHardwareAccelerated = true;
        CanSetCrf = true;
        MinimumCrf = 0;
        MaximumCrf = 51;
        CodecPresets = new List<EnumItem<VideoCodecPresets>>
        {
            new(VideoCodecPresets.None, "S.SaveAs.VideoOptions.CodecPreset.None", ""),
            new(VideoCodecPresets.VerySlow, "S.SaveAs.VideoOptions.CodecPreset.VerySlow", "veryslow"),
            new(VideoCodecPresets.Slower, "S.SaveAs.VideoOptions.CodecPreset.Slower", "slower"),
            new(VideoCodecPresets.Slow, "S.SaveAs.VideoOptions.CodecPreset.Slow", "slow"),
            new(VideoCodecPresets.Medium, "S.SaveAs.VideoOptions.CodecPreset.Medium", "medium"),
            new(VideoCodecPresets.Fast, "S.SaveAs.VideoOptions.CodecPreset.Fast", "fast"),
            new(VideoCodecPresets.Faster, "S.SaveAs.VideoOptions.CodecPreset.Faster", "faster"),
            new(VideoCodecPresets.VeryFast, "S.SaveAs.VideoOptions.CodecPreset.VeryFast", "veryfast")
        };
        PixelFormats = new List<EnumItem<VideoPixelFormats>>
        {
            new(VideoPixelFormats.Auto, "S.SaveAs.VideoOptions.PixelFormat.Auto", ""),
            new(VideoPixelFormats.Nv12, "", "Nv12", "nv12"),
            new(VideoPixelFormats.P010Le, "", "P010Le", "p010le"),
            new(VideoPixelFormats.Qsv, "", "Qsv", "qsv")
        };
    }
}