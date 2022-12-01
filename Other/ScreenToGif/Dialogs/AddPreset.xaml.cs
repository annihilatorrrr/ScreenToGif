using ScreenToGif.Controls;
using ScreenToGif.Domain.Enums;
using ScreenToGif.ViewModel.Dialogs;
using ScreenToGif.ViewModel.Presets.Export;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ScreenToGif.Dialogs;

public partial class AddPreset : ExWindow
{
    private readonly AddPresetViewModel _viewModel = new();

    public ExportPresetViewModel Preset => _viewModel.AssembledPreset;

    public AddPreset(ExportFormats format, List<EncoderTypes> availableEncoders)
    {
        InitializeComponent();

        DataContext = _viewModel;

        _viewModel.Format = format;
        _viewModel.AvailableEncoders = availableEncoders;
        _viewModel.Encoder = availableEncoders.First();
    }
    
    private void Dialog_Loaded(object sender, RoutedEventArgs e)
    {
        TitleTextBox.Focus();
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