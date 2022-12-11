using ScreenToGif.Domain.Enums;
using System.Runtime.Serialization;

namespace ScreenToGif.Domain.Models.Preset.Export.AnimatedImage.Apng;

public class FfmpegApngPreset : ApngPreset
{
    public VideoSettingsModes SettingsMode { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public string Parameters { get; set; }

    public PredictionMethods PredictionMethod { get; set; }

    public VideoPixelFormats PixelFormat { get; set; }

    public Framerates Framerate { get; set; }

    public decimal CustomFramerate { get; set; }

    public Vsyncs Vsync { get; set; }
    
    public FfmpegApngPreset()
    {
        Encoder = EncoderTypes.FFmpeg;
    }
}