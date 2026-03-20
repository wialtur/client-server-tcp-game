using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ClientServerTcpGame.Network;
using ClientServerTcpGame.Shared;

namespace ClientServerTcpGame.Client;

public sealed class GameClient : IDisposable
{
    private readonly TcpClient _tcp = new();
    private TcpJsonConnection? _conn;
    private CancellationTokenSource? _cts;

    public int PlayerId { get; private set; }

    public event Action<LobbyMsg>? OnLobby;
    public event Action<StartMsg>? OnStart;
    public event Action<SpawnMsg>? OnSpawn;
    public event Action<PopResultMsg>? OnPopResult;
    public event Action<EndMsg>? OnEnd;
    public event Action<RatingMsg>? OnRating;
    public event Action<ErrorMsg>? OnError;
    public event Action<TournamentResultMsg>? OnTournamentResult;


    public async Task ConnectAsync(string host, int port, string name, CancellationToken ct = default)
    {
        await _tcp.ConnectAsync(host, port, ct);
        _conn = new TcpJsonConnection(_tcp);

        _cts = new CancellationTokenSource();

        _ = _conn.ReadLoopAsync(HandleLineAsync, _cts.Token);

        await _conn.SendAsync(new HelloMsg { name = name }, ct);
    }

    private async Task HandleLineAsync(string json)
    {
        var msg = MessageCodec.Decode(json);
        if (msg == null || _conn == null) return;

        switch (msg.t)
        {
            case "WELCOME":
                PlayerId = ((WelcomeMsg)msg).playerId;
                break;

            case "LOBBY":
                OnLobby?.Invoke((LobbyMsg)msg);
                break;

            case "START":
                OnStart?.Invoke((StartMsg)msg);
                break;

            case "SPAWN":
                OnSpawn?.Invoke((SpawnMsg)msg);
                break;

            case "POP_RESULT":
                OnPopResult?.Invoke((PopResultMsg)msg);
                break;

            case "END":
                OnEnd?.Invoke((EndMsg)msg);
                break;

            case "RATING":
                OnRating?.Invoke((RatingMsg)msg);
                break;

            case "ERROR":
                OnError?.Invoke((ErrorMsg)msg);
                break;

            case "PING":
                var n = ((PingMsg)msg).n;
                await _conn.SendAsync(new PongMsg { n = n });
                break;
            case "TOURNAMENT_RESULT":
                OnTournamentResult?.Invoke((TournamentResultMsg)msg);
                break;
        }
    }

    public Task PickColorAsync(string color)
        => _conn!.SendAsync(new ColorPickMsg { color = color });

    public Task SetReadyAsync(bool ready)
        => _conn!.SendAsync(new ReadyMsg { ready = ready });

    public Task PopAsync(int ballId, double x, double y)
        => _conn!.SendAsync(new PopMsg { ballId = ballId, clickX = x, clickY = y });

    public Task GetRatingAsync(int top = 20)
        => _conn!.SendAsync(new GetRatingMsg { top = top });

    public void Dispose()
    {
        try { _cts?.Cancel(); } catch { }
        try { _conn?.Dispose(); } catch { }
        try { _tcp.Close(); } catch { }
    }
}
