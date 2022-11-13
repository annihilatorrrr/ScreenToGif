using ScreenToGif.Domain.Enums;
using System.Runtime.Serialization;

namespace ScreenToGif.Domain.Models.Preset.Export.AnimatedImage.Gif;

public class FfmpegGifPreset : GifPreset
{
    public VideoSettingsModes SettingsMode { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public string Parameters { get; set; }

    public DitherMethods Dither { get; set; }

    public int BayerScale { get; set; }

    public VideoPixelFormats PixelFormat { get; set; }

    public Framerates Framerate { get; set; }

    public double CustomFramerate { get; set; }

    public Vsyncs Vsync { get; set; }

    public FfmpegGifPreset()
    {
        Encoder = EncoderTypes.FFmpeg;
    }
}