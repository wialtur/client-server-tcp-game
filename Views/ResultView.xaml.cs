using ClientServerTcpGame;
using ClientServerTcpGame.Shared;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClientServerTcpGame.Views;

public partial class ResultView : UserControl
{
    private readonly MainWindow _mw;

    public ResultView(MainWindow mw)
    {
        InitializeComponent();
        _mw = mw;
    }

    public void ApplyEnd(EndMsg msg)
    {
        ReasonText.Text = $"Reason: {msg.reason}";

        // длительность 
        DurationText.Text = "Duration: 30 sec";

        // финальный счёт
        int s1 = msg.final?.FirstOrDefault(x => x.id == 1)?.value ?? 0;
        int s2 = msg.final?.FirstOrDefault(x => x.id == 2)?.value ?? 0;
        FinalText.Text = $"Final score: {s1} : {s2}";

        // цвета (из последнего Lobby, которое  в MainWindow.LastLobby)
        SetColorRect(P1ColorRect, GetLobbyColor(1));
        SetColorRect(P2ColorRect, GetLobbyColor(2));

        // статистика кликов 
        var p1 = msg.stats?.FirstOrDefault(x => x.id == 1);
        var p2 = msg.stats?.FirstOrDefault(x => x.id == 2);

        if (p1 != null)
        {
            int total1 = p1.hitsOwn + p1.hitsEnemy + p1.hitsNeutral;
            P1StatsText.Text =
                $"Total: {total1}, " +
                $"Own: {p1.hitsOwn}, " +
                $"Enemy: {p1.hitsEnemy}, " +
                $"Neutral: {p1.hitsNeutral}";
        }
        else
        {
            P1StatsText.Text = "Total: 0, Own: 0, Enemy: 0, Neutral: 0";
        }

        if (p2 != null)
        {
            int total2 = p2.hitsOwn + p2.hitsEnemy + p2.hitsNeutral;
            P2StatsText.Text =
                $"Total: {total2}, " +
                $"Own: {p2.hitsOwn}, " +
                $"Enemy: {p2.hitsEnemy}, " +
                $"Neutral: {p2.hitsNeutral}";
        }
        else
        {
            P2StatsText.Text = "Total: 0, Own: 0, Enemy: 0, Neutral: 0";
        }
    }

    private string GetLobbyColor(int playerId)
    {
        var p = _mw.LastLobby?.players?.FirstOrDefault(x => x.Id == playerId);
        return string.IsNullOrWhiteSpace(p?.Color) ? "#000000" : p!.Color!;
    }

    private static void SetColorRect(System.Windows.Shapes.Rectangle rect, string hex)
    {
        try
        {
            rect.Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(hex);
        }
        catch
        {
            rect.Fill = Brushes.Transparent;
        }
    }

    private async void Rating_Click(object sender, RoutedEventArgs e)
        => await _mw.Client!.GetRatingAsync();

    private void Exit_Click(object sender, RoutedEventArgs e)
        => Application.Current.Shutdown();
}
