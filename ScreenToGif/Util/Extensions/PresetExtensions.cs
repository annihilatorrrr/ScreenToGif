using System.Linq;
using ScreenToGif.Domain.Events;
using System.Threading.Tasks;
using ScreenToGif.Cloud;
using ScreenToGif.Domain.Enums;
using ScreenToGif.Util.Settings;
using ScreenToGif.ViewModel.UploadPresets.Gfycat;
using ScreenToGif.ViewModel.UploadPresets.Imgur;
using ScreenToGif.ViewModel.UploadPresets.Yandex;
using ScreenToGif.Windows;
using ScreenToGif.ViewModel.Presets.Upload;

namespace ScreenToGif.Util.Extensions;

internal static class PresetExtensions
{
    internal static void Persist(this UploadPresetViewModel preset, string previousTitle = null)
    {
        var current = UserSettings.All.UploadPresets.OfType<UploadPresetViewModel>().FirstOrDefault(f => f.Title == (previousTitle ?? preset.Title));

        if (current != null)
            UserSettings.All.UploadPresets.Remove(current);

        UserSettings.All.UploadPresets.Add(preset);
        UserSettings.Save();
    }

    public static async Task<ValidatedEventArgs> IsValid(UploadPresetViewModel preset)
    {
        switch (preset)
        {
            case GfycatPresetViewModel gfycat:
                return await IsValid(gfycat);

            case ImgurPresetViewModel imgur:
                return await IsValid(imgur);

            case YandexPresetViewModel yandex:
                return await IsValid(yandex);
        }

        return await preset.IsValid();
    }

    public static async Task<ValidatedEventArgs> IsValid(GfycatPresetViewModel preset)
    {
        if (!preset.IsAnonymous && !await Gfycat.IsAuthorized(preset))
            return new ValidatedEventArgs("S.SaveAs.Warning.Upload.NotAuthorized", StatusReasons.UploadServiceUnauthorized, () => App.MainViewModelOld.OpenOptions.Execute(Options.UploadIndex));

        return await preset.IsValid();
    }

    public static async Task<ValidatedEventArgs> IsValid(ImgurPresetViewModel preset)
    {
        if (!preset.IsAnonymous && !await Imgur.IsAuthorized(preset))
            return new ValidatedEventArgs("S.SaveAs.Warning.Upload.NotAuthorized", StatusReasons.UploadServiceUnauthorized, () => App.MainViewModelOld.OpenOptions.Execute(Options.UploadIndex));

        return await preset.IsValid();
    }

    public static async Task<ValidatedEventArgs> IsValid(YandexPresetViewModel preset)
    {
        if (!preset.IsAnonymous && !YandexDisk.IsAuthorized(preset))
            return new ValidatedEventArgs("S.SaveAs.Warning.Upload.NotAuthorized", StatusReasons.UploadServiceUnauthorized, () => App.MainViewModelOld.OpenOptions.Execute(Options.UploadIndex));

        return await preset.IsValid();
    }
}