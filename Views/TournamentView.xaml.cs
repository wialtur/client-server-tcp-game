using System.Windows;
using System.Windows.Controls;
using ClientServerTcpGame.Shared;

namespace ClientServerTcpGame.Views;

public partial class TournamentView : UserControl
{
    private readonly MainWindow _mw;

    public TournamentView(MainWindow mw)
    {
        InitializeComponent();
        _mw = mw;
    }

    public void ApplyTournament(TournamentResultMsg msg)
        => Grid.ItemsSource = msg.rows;

    private void Back_Click(object sender, RoutedEventArgs e)
        => _mw.ShowResult();
}
