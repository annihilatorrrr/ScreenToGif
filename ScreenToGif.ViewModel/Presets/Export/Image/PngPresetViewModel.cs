using ScreenToGif.Domain.Enums;

namespace ScreenToGif.ViewModel.Presets.Export.Image;

public class PngPresetViewModel : ImagePresetViewModel
{
    public PngPresetViewModel()
    {
        Type = ExportFormats.Png;
        Extension = ".png";
    }
    
    public static PngPresetViewModel Default => new()
    {
        TitleKey = "S.Preset.Default.Title",
        DescriptionKey = "S.Preset.Default.Description",
        HasAutoSave = true,
        IsSelectedForEncoder = true,
        IsDefault = true,
        CreationDate = new DateTime(2021, 02, 20)
    };
}