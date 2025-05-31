using BallMusic.Infrastructure;
using System.Windows;

namespace BallMusic.Player;
public partial class MessageWindow : Window
{
    private readonly ServerConnection _server;

    public MessageWindow(ServerConnection _server)
    {
        InitializeComponent();
        this._server = _server;
    }

    private void SendNews_Click(object sender, RoutedEventArgs e)
    {
        _ = _server.SendNews(NewsBox.Text);
    }

    private void SendMessage_Click(object sender, RoutedEventArgs e)
    {
        _ = _server.SendMessage(MessageBox.Text.Replace("\n", "<br>"));
    }
}
