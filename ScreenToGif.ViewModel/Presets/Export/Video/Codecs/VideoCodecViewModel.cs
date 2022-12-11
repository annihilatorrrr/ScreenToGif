using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Models;
using ScreenToGif.Domain.ViewModels;

namespace ScreenToGif.ViewModel.Presets.Export.Video.Codecs;

public class VideoCodecViewModel : BaseViewModel
{
    private VideoCodecs _type;
    private string _name;
    private string _command;
    private string _parameters;
    private bool _isHardwareAccelerated;
    private bool _canSetCrf;
    private int _minimumCrf;
    private int _maximumCrf;
    private List<EnumItem<VideoCodecPresets>> _codecPresets;
    private List<EnumItem<VideoPixelFormats>> _pixelFormats;

    public VideoCodecs Type
    {
        get => _type;
        internal set => SetProperty(ref _type, value);
    }

    public string Name
    {
        get => _name;
        internal set => SetProperty(ref _name, value);
    }

    public string Command
    {
        get => _command;
        internal set => SetProperty(ref _command, value);
    }

    public string Parameters
    {
        get => _parameters;
        internal set => SetProperty(ref _parameters, value);
    }

    public bool IsHardwareAccelerated
    {
        get => _isHardwareAccelerated;
        internal set => SetProperty(ref _isHardwareAccelerated, value);
    }

    public bool CanSetCrf
    {
        get => _canSetCrf;
        internal set => SetProperty(ref _canSetCrf, value);
    }

    public int MinimumCrf
    {
        get => _minimumCrf;
        internal set => SetProperty(ref _minimumCrf, value);
    }

    public int MaximumCrf
    {
        get => _maximumCrf;
        internal set => SetProperty(ref _maximumCrf, value);
    }

    public List<EnumItem<VideoCodecPresets>> CodecPresets
    {
        get => _codecPresets;
        internal set => SetProperty(ref _codecPresets, value);
    }

    public List<EnumItem<VideoPixelFormats>> PixelFormats
    {
        get => _pixelFormats;
        internal set => SetProperty(ref _pixelFormats, value);
    }
}