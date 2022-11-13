using ScreenToGif.Domain.Enums;

namespace ScreenToGif.ViewModel.Presets.Export.Image;

public class BmpPresetViewModel : ImagePresetViewModel
{
    public BmpPresetViewModel()
    {
        Type = ExportFormats.Bmp;
        Extension = ".bmp";
    }
    
    public static BmpPresetViewModel Default => new()
    {
        TitleKey = "S.Preset.Default.Title",
        DescriptionKey = "S.Preset.Default.Description",
        HasAutoSave = true,
        IsSelectedForEncoder = true,
        IsDefault = true,
        CreationDate = new DateTime(2021, 02, 20)
    };
}