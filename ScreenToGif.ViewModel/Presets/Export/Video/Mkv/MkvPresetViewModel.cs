using ScreenToGif.Domain.Enums;

namespace ScreenToGif.ViewModel.Presets.Export.Video.Mkv;

public class MkvPresetViewModel : VideoPresetViewModel
{
    public MkvPresetViewModel()
    {
        Type = ExportFormats.Mkv;
        Extension = ".mkv";
    }
}