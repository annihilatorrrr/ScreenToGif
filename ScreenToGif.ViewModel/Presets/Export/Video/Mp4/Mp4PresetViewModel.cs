using ScreenToGif.Domain.Enums;

namespace ScreenToGif.ViewModel.Presets.Export.Video.Mp4;

public class Mp4PresetViewModel : VideoPresetViewModel
{
    public Mp4PresetViewModel()
    {
        Type = ExportFormats.Mp4;
        Extension = ".mp4";
    }
}