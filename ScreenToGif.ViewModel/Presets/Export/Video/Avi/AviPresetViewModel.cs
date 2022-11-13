using ScreenToGif.Domain.Enums;

namespace ScreenToGif.ViewModel.Presets.Export.Video.Avi;

public class AviPresetViewModel : VideoPresetViewModel
{
    public AviPresetViewModel()
    {
        Type = ExportFormats.Avi;
        Extension = ".avi";
    }
}