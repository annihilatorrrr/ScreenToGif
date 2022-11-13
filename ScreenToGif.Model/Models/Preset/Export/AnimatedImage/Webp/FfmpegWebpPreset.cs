using ScreenToGif.Domain.Enums;
using System.Runtime.Serialization;

namespace ScreenToGif.Domain.Models.Preset.Export.AnimatedImage.Webp;

public class FfmpegWebpPreset : WebpPreset
{
    public VideoSettingsModes SettingsMode { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public string Parameters { get; set; }

    public VideoCodecPresets CodecPreset { get; set; }

    public int Quality { get; set; }

    public bool Lossless { get; set; }

    public VideoPixelFormats PixelFormat { get; set; }

    public Framerates Framerate { get; set; }

    public double CustomFramerate { get; set; }

    public Vsyncs Vsync { get; set; }
    
    public FfmpegWebpPreset()
    {
        Encoder = EncoderTypes.FFmpeg;
    }
}