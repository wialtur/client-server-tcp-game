using ClientServerTcpGame;
using ClientServerTcpGame.Shared;
using System.Windows;
using System.Windows.Controls;

namespace ClientServerTcpGame.Views;

public partial class RatingView : UserControl
{
    private readonly MainWindow _mw;

    public RatingView(MainWindow mw)
    {
        InitializeComponent();
        _mw = mw;
    }

    public void ApplyRating(RatingMsg msg)
    {
        Grid.ItemsSource = msg.items;
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        _mw.ShowResult();
    }
}
