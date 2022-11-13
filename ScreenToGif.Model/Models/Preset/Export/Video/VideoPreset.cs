using ScreenToGif.Domain.Enums;
using System.Runtime.Serialization;

namespace ScreenToGif.Domain.Models.Preset.Export.Video;

public class VideoPreset : ExportPreset
{
    public VideoSettingsModes SettingsMode { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public string Parameters { get; set; }
    
    public VideoCodecs VideoCodec { get; set; }

    public VideoCodecPresets CodecPreset { get; set; }

    /// <summary>
    /// Hardware acceleration mode.
    /// https://trac.ffmpeg.org/wiki/HWAccelIntro
    /// </summary>
    public HardwareAccelerationModes HardwareAcceleration { get; set; }

    public int Pass { get; set; }

    public bool IsVariableBitRate { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public int? ConstantRateFactor { get; set; }

    public decimal BitRate { get; set; }

    /// <summary>
    /// Quality level (-q:v, -qscale:v), in use when having the bitrate mode set to variable.
    /// </summary>
    public int QualityLevel { get; set; }

    public RateUnits BitRateUnit { get; set; }

    public decimal MinimumBitRate { get; set; }

    public RateUnits MinimumBitRateUnit { get; set; }

    public decimal MaximumBitRate { get; set; }

    public RateUnits MaximumBitRateUnit { get; set; }

    public decimal RateControlBuffer { get; set; }

    public RateUnits RateControlBufferUnit { get; set; }

    public VideoPixelFormats PixelFormat { get; set; }

    public Framerates Framerate { get; set; }

    public decimal CustomFramerate { get; set; }

    public Vsyncs Vsync { get; set; }

    public bool IsAncientContainer { get; set; }
}