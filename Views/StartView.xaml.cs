using ClientServerTcpGame;
using ClientServerTcpGame.Client;
using ClientServerTcpGame.Server;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ClientServerTcpGame.Views;

public partial class StartView : UserControl
{
    private readonly MainWindow _mw;

    public StartView(MainWindow mw)
    {
        InitializeComponent();
        _mw = mw;
    }

    private async void Host_Click(object sender, RoutedEventArgs e)
    {
        int port = int.Parse(PortBox.Text);
        int level = LevelBox.SelectedIndex + 1;
        bool tour = TournamentBox.IsChecked == true;

        _mw.Server = new GameServer { Level = level };
        _mw.Server = new GameServer { Level = level, TournamentMode = tour, TournamentRounds = 3 };
        _ = Task.Run(() => _mw.Server.StartAsync(port));

        await JoinInternal();
    }

    private async void Join_Click(object sender, RoutedEventArgs e)
        => await JoinInternal();

    private async Task JoinInternal()
    {
        _mw.Client = new GameClient();

        //_mw.Client.OnLobby += _ => Dispatcher.Invoke(_mw.ShowLobby);
        //_mw.Client.OnStart += _ => Dispatcher.Invoke(_mw.ShowGame);
        //_mw.Client.OnEnd += _ => Dispatcher.Invoke(_mw.ShowResult);
        _mw.Client.OnLobby += msg => Dispatcher.Invoke(() =>
        {
            _mw.LastLobby = msg;
            // показать Lobby только если мы ещё не в LobbyView
            if (_mw.Host.Content is not LobbyView)
                _mw.ShowLobby();

            // обновить данные лобби внутри существующего view
            if (_mw.Host.Content is LobbyView lv)
                lv.ApplyLobby(msg);
        });
        _mw.Client.OnStart += msg => Dispatcher.Invoke(() =>
        {
            if (_mw.Host.Content is not GameView)
                _mw.ShowGame();

            if (_mw.Host.Content is GameView gv)
                gv.ApplyStart(msg);
        });

        _mw.Client.OnEnd += msg => Dispatcher.Invoke(() =>
        {
            if (_mw.Host.Content is not ResultView)
                _mw.ShowResult();

            if (_mw.Host.Content is ResultView rv)
                rv.ApplyEnd(msg);
        });

        _mw.Client.OnRating += msg => Dispatcher.Invoke(() =>
        {
            if (_mw.Host.Content is not RatingView)
                _mw.ShowRating();

            if (_mw.Host.Content is RatingView rv)
                rv.ApplyRating(msg);
        });

        _mw.Client.OnTournamentResult += msg => Dispatcher.Invoke(() =>
        {
            if (_mw.Host.Content is not TournamentView)
                _mw.ShowTournament();

            if (_mw.Host.Content is TournamentView tv)
                tv.ApplyTournament(msg);
        });

        await _mw.Client.ConnectAsync(
            IpBox.Text,
            int.Parse(PortBox.Text),
            NameBox.Text
        );
    }
}
