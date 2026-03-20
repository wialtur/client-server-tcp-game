using ClientServerTcpGame;
using ClientServerTcpGame.Shared;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClientServerTcpGame.Views;

public partial class LobbyView : UserControl
{
    private readonly MainWindow _mw;

    private bool _uiLock;
    private string _myColor = "";
    private string _enemyColor = "";

    private static readonly string[] Palette =
    {
        "#FF1744", // red
        "#2979FF", // blue
        "#00E676", // green
        "#FFEA00", // yellow
        "#FF9100", // orange
        "#D500F9", // purple
        "#00B0FF", // light blue
        "#FFFFFF", // white
    };

    public LobbyView(MainWindow mw)
    {
        InitializeComponent();
        _mw = mw;

        BuildPaletteButtons();
        SetSelectedColor(Palette[0]);
    }

    private void BuildPaletteButtons()
    {
        PaletteGrid.Children.Clear();

        foreach (var hex in Palette)
        {
            var btn = new Button
            {
                Tag = hex,
                Margin = new Thickness(4),
                Height = 36,
                BorderThickness = new Thickness(2),
                BorderBrush = Brushes.Transparent,
                Background = (SolidColorBrush)new BrushConverter().ConvertFromString(hex),
                ToolTip = hex
            };

            btn.Click += PaletteButton_Click;
            PaletteGrid.Children.Add(btn);
        }
    }

   
    public void ApplyLobby(LobbyMsg msg)
    {
        int me = _mw.Client?.PlayerId ?? 0;

        var myPlayer = msg.players?.FirstOrDefault(p => p.Id == me);
        var enemyPlayer = msg.players?.FirstOrDefault(p => p.Id != me);

        _uiLock = true;
        try
        {
            if (myPlayer != null)
                ReadyBox.IsChecked = myPlayer.Ready;

            _myColor = myPlayer?.Color ?? "";
            _enemyColor = enemyPlayer?.Color ?? "";

            // обновить preview моего цвета
            if (!string.IsNullOrWhiteSpace(_myColor))
                UpdatePreview(_myColor);
            else
                UpdatePreview("#000000"); // если ещё не выбран

            // обновить рамки палитры:
            UpdatePaletteSelectionBorder(_myColor, _enemyColor);
        }
        finally
        {
            _uiLock = false;
        }
    }

    private async void PaletteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_uiLock) return;

        if (sender is Button b && b.Tag is string hex)
        {
            SetSelectedColor(hex);
            _myColor = hex;
            UpdatePaletteSelectionBorder(_myColor, _enemyColor);
            await _mw.Client!.PickColorAsync(hex);
        }
    }

    private void SetSelectedColor(string hex)
    {
        _myColor = hex;
        UpdatePreview(hex);
        UpdatePaletteSelectionBorder(_myColor, _enemyColor);
    }

    private void UpdatePreview(string hex)
    {
        try
        {
            ColorPreview.Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(hex);
        }
        catch
        {
            ColorPreview.Fill = Brushes.Transparent;
        }
    }

    private void UpdatePaletteSelectionBorder(string myHex, string enemyHex)
    {
        foreach (var child in PaletteGrid.Children.OfType<Button>())
        {
            var hex = child.Tag as string ?? "";

            // default
            child.BorderThickness = new Thickness(2);
            child.BorderBrush = Brushes.Transparent;

            // enemy color (occupied) - gray border
            if (!string.IsNullOrWhiteSpace(enemyHex) &&
                hex.Equals(enemyHex, StringComparison.OrdinalIgnoreCase))
            {
                child.BorderBrush = Brushes.Gray;
            }

            // my color - black border 
            if (!string.IsNullOrWhiteSpace(myHex) &&
                hex.Equals(myHex, StringComparison.OrdinalIgnoreCase))
            {
                child.BorderBrush = Brushes.Black;
            }
        }
    }

    private async void Ready_Checked(object sender, RoutedEventArgs e)
        => await _mw.Client!.SetReadyAsync(true);

    private async void Ready_Unchecked(object sender, RoutedEventArgs e)
        => await _mw.Client!.SetReadyAsync(false);
}
