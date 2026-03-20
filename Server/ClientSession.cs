using System;
using System.Net.Sockets;
using ClientServerTcpGame.Network;
using ClientServerTcpGame.Shared;

namespace ClientServerTcpGame.Server;

public sealed class ClientSession
{
    public int Slot { get; }                 // 1 or 2
    public TcpClient Tcp { get; }
    public TcpJsonConnection Conn { get; }
    public PlayerInfo Player { get; } = new();

    public DateTime LastPongUtc { get; set; } = DateTime.UtcNow;

    public ClientSession(int slot, TcpClient tcp)
    {
        Slot = slot;
        Tcp = tcp;
        Conn = new TcpJsonConnection(tcp);
        Player.Id = slot;
    }
}
