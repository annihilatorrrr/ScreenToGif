using ScreenToGif.Domain.Enums;
using System.Windows;

namespace ScreenToGif.Controls;

public class ExporterComboBoxItem : ExComboBoxItem
{
    public static readonly DependencyProperty FileTypeProperty = DependencyProperty.Register(nameof(FileType), typeof(ExportFormats), typeof(ExporterComboBoxItem), new PropertyMetadata(default(ExportFormats)));
    
    public ExportFormats FileType
    {
        get => (ExportFormats)GetValue(FileTypeProperty);
        set => SetValue(FileTypeProperty, value);
    }
}