using System.Runtime.Serialization;
using ScreenToGif.Domain.Enums;
using ScreenToGif.ViewModel.Presets.Upload;

namespace ScreenToGif.ViewModel.UploadPresets.Yandex;

public class YandexPresetViewModel : UploadPresetViewModel
{
    private string _oAuthToken = "";

    [DataMember(EmitDefaultValue = false)]
    public string OAuthToken
    {
        get => _oAuthToken;
        set => SetProperty(ref _oAuthToken, value);
    }

    public YandexPresetViewModel()
    {
        Type = UploadDestinations.Yandex;
        AllowedTypes = new List<ExportFormats>();
    }
}