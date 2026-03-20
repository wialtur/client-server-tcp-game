using System.Windows;
using ClientServerTcpGame.Client;
using ClientServerTcpGame.Server;
using ClientServerTcpGame.Shared;

namespace ClientServerTcpGame;

public partial class MainWindow : Window
{
    public GameClient? Client;
    public GameServer? Server;
    public LobbyMsg? LastLobby;

    public MainWindow()
    {
        InitializeComponent();
        ShowStart();
    }

    public void ShowStart()
        => Host.Content = new Views.StartView(this);

    public void ShowLobby()
        => Host.Content = new Views.LobbyView(this);

    public void ShowGame()
        => Host.Content = new Views.GameView(this);

    public void ShowResult()
        => Host.Content = new Views.ResultView(this);
    public void ShowRating()
    => Host.Content = new Views.RatingView(this);
    public void ShowTournament()
    => Host.Content = new Views.TournamentView(this);
}
