using ScreenToGif.Domain.Enums;

namespace ScreenToGif.ViewModel.Presets.Export.Video.Mov;

public class MovPresetViewModel : VideoPresetViewModel
{
    public MovPresetViewModel()
    {
        Type = ExportFormats.Mov;
        Extension = ".mov";
    }
}