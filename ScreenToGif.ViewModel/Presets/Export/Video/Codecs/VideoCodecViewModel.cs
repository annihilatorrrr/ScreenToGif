using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.ViewModels;

namespace ScreenToGif.ViewModel.Presets.Export.Video.Codecs;

public class VideoCodecViewModel : BaseViewModel
{
    public VideoCodecs Type { get; internal set; }

    public string Name { get; internal set; }

    public string Command { get; internal set; }

    public string Parameters { get; internal set; }

    public bool IsHardwareAccelerated { get; internal set; }

    public bool CanSetCrf { get; internal set; }

    public int MinimumCrf { get; internal set; }

    public int MaximumCrf { get; internal set; }

    public List<EnumItem<VideoCodecPresets>> CodecPresets { get; internal set; }

    public List<EnumItem<VideoPixelFormats>> PixelFormats { get; internal set; }
}

public class EnumItem<T> where T : Enum
{
    public T Type { get; set; }

    public string NameKey { get; set; }

    public string Name { get; set; }

    public string Parameter { get; set; }

    public EnumItem()
    { }

    public EnumItem(T type, string nameKey, string name, string parameter)
    {
        Type = type;
        NameKey = nameKey;
        Name = name;
        Parameter = parameter;
    }

    public EnumItem(T type, string nameKey, string parameter)
    {
        Type = type;
        NameKey = nameKey;
        Parameter = parameter;
    }
}