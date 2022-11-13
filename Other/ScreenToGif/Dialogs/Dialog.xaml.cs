using ScreenToGif.Controls;
using ScreenToGif.Domain.Enums;
using ScreenToGif.Util;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ScreenToGif.Dialogs;

public partial class Dialog : ExWindow
{
    public Dialog()
    {
        InitializeComponent();
    }

    public static bool Ok(string titleId, string detailsId)
    {
        return ShowStatic(titleId, detailsId, "S.Ok");
    }

    public static bool OkStatic(string title, string details)
    {
        return ShowStatic(title, details, LocalizationHelper.Get("S.Ok"));
    }

    public static bool OkCancel(string titleId, string detailsId, int main = 0)
    {
        return Show(titleId, detailsId, "S.Ok", main, "S.Cancel");
    }

    public static bool OkCancelStatic(string title, string details, int main = 0)
    {
        return ShowStatic(title, details, LocalizationHelper.Get("S.Ok"), main, LocalizationHelper.Get("S.Cancel"));
    }

    public static bool Ask(string titleId, string detailsId, string yesId = "S.Yes", string noId = "S.No")
    {
        return Show(titleId, detailsId, yesId, 0, noId);
    }

    public static bool AskStatic(string title, string details, string yes = null, string no = null)
    {
        return ShowStatic(title, details, yes ?? LocalizationHelper.Get("S.No"), 0, no ?? LocalizationHelper.Get("S.Yes"));
    }

    public static bool Show(string titleId, string detailsId, string positiveId, int main = 0, string negativeId = null)
    {
        var dialog = new Dialog();
        dialog.SetData(titleId, detailsId, positiveId, main, negativeId);

        return dialog.ShowDialog() ?? false;
    }

    public static bool ShowStatic(string title, string details, string positive, int main = 0, string negative = null)
    {
        var dialog = new Dialog();
        dialog.SetDataStatic(title, details, positive, main, negative);

        return dialog.ShowDialog() ?? false;
    }

    private void SetData(string title, string details, string positiveId, int main = 0, string negativeId = null)
    {
        TitleTextBlock.SetResourceReference(TextBlock.TextProperty, title);
        DetailsTextBlock.SetResourceReference(TextBlock.TextProperty, details);

        PositiveButton.SetResourceReference(ContentProperty, positiveId);
        PositiveButton.IsAccented = main == 0;
        PositiveButton.Focus();

        if (negativeId == null)
        {
            NegativeButton.Visibility = Visibility.Collapsed;
            Grid.SetColumn(PositiveButton, 1);
            return;
        }

        NegativeButton.SetResourceReference(ContentProperty, negativeId);
        NegativeButton.IsAccented = main == 1;
    }

    private void SetDataStatic(string title, string details, string positive, int main = 0, string negative = null)
    {
        TitleTextBlock.Text = title;
        DetailsTextBlock.Text = details;

        PositiveButton.Content = positive;
        PositiveButton.IsAccented = main == 0;
        PositiveButton.Focus();

        if (negative == null)
        {
            NegativeButton.Visibility = Visibility.Collapsed;
            Grid.SetColumn(PositiveButton, 1);
            return;
        }

        NegativeButton.Content = negative;
        NegativeButton.IsAccented = main == 1;
    }

    private void Dialog_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Y:
                DialogResult = true; //[Y] will answer 'Yes' to ask-dialog
                break;
            case Key.Escape:
            case Key.N:
                DialogResult = false; //[ESC] or [N] will answer 'No' to ask-dialog
                break;
        }
    }

    private void NegativeActionButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void PositiveActionButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}