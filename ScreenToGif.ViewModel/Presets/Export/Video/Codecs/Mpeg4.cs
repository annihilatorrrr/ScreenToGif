using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Models;

namespace ScreenToGif.ViewModel.Presets.Export.Video.Codecs;

/// <summary>
/// https://trac.ffmpeg.org/wiki/Encode/MPEG-4
/// </summary>
public class Mpeg4 : VideoCodecViewModel
{
    public Mpeg4()
    {
        Type = VideoCodecs.Mpeg4;
        Name = "MPEG-4";
        Command = "mpeg4";
        Parameters = "-vtag xvid";

        CodecPresets = new List<EnumItem<VideoCodecPresets>>
        {
            new(VideoCodecPresets.None, "S.SaveAs.VideoOptions.CodecPreset.None", "")
        };
        PixelFormats = new List<EnumItem<VideoPixelFormats>>
        {
            new(VideoPixelFormats.Auto, "S.SaveAs.VideoOptions.PixelFormat.Auto", ""),
            new(VideoPixelFormats.Yuv420p, "", "Yuv420p", "yuv420p")
        };
    }
}