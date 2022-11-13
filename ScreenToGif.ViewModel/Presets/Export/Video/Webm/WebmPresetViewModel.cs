using ScreenToGif.Domain.Enums;

namespace ScreenToGif.ViewModel.Presets.Export.Video.Webm;

public class WebmPresetViewModel : VideoPresetViewModel
{
    public WebmPresetViewModel()
    {
        Type = ExportFormats.Webm;
        Extension = ".webm";
    }
}