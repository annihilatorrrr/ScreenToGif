using ScreenToGif.Controls;
using ScreenToGif.Domain.Enums;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
        return Show(titleId, detailsId, DialogButtons.Ok);
    }

    public static bool OkStatic(string title, string details)
    {
        return ShowStatic(title, details, DialogButtons.Ok);
    }

    public static bool OkCancel(string titleId, string detailsId)
    {
        return Show(titleId, detailsId, DialogButtons.OkCancel);
    }

    public static bool OkCancelStatic(string title, string details)
    {
        return ShowStatic(title, details, DialogButtons.OkCancel);
    }

    public static bool Ask(string titleId, string detailsId)
    {
        return Show(titleId, detailsId, DialogButtons.YesNo);
    }

    public static bool AskStatic(string title, string details)
    {
        return ShowStatic(title, details, DialogButtons.YesNo);
    }

    public static bool Show(string titleId, string detailsId, DialogButtons buttons)
    {
        var dialog = new Dialog();
        dialog.SetData(titleId, detailsId, buttons);

        return dialog.ShowDialog() ?? false;
    }

    public static bool ShowStatic(string title, string details, DialogButtons buttons)
    {
        var dialog = new Dialog();
        dialog.SetData(title, details, buttons, true);

        return dialog.ShowDialog() ?? false;
    }

    private void SetData(string title, string details, DialogButtons buttons, bool isStatic = false)
    {
        if (isStatic)
        {
            TitleTextBlock.Text = title;
            DetailsTextBlock.Text = details;
        }
        else
        {
            TitleTextBlock.SetResourceReference(TextBlock.TextProperty, title);
            DetailsTextBlock.SetResourceReference(TextBlock.TextProperty, details);
        }

        switch (buttons)
        {
            case DialogButtons.Ok:
            {
                NegativeButton.Visibility = Visibility.Collapsed;
                Grid.SetColumn(PositiveButton, 1);
                break;
            }

            case DialogButtons.YesNo:
            {
                PositiveButton.SetResourceReference(ContentProperty, "S.Yes");
                NegativeButton.SetResourceReference(ContentProperty, "S.No");
                break;
            }
        }
        
        PositiveButton.Focus();
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