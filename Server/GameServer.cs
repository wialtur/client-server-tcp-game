using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ClientServerTcpGame.Network;
using ClientServerTcpGame.Shared;

namespace ClientServerTcpGame.Server;

public sealed class GameServer
{
    private TcpListener? _listener;
    private ClientSession? _s1;
    private ClientSession? _s2;
    private ServerGameCore? _game;

    private readonly RatingRepository _repo = new();
    private readonly GameConfig _cfg = new();


    public int Level { get; set; } = 1;
    public bool TournamentMode { get; set; } = false;
    public int TournamentRounds { get; set; } = 3;
    public async Task StartAsync(int port, CancellationToken ct = default)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();

        // accept exactly 2 clients
        var c1 = await _listener.AcceptTcpClientAsync(ct);
        _s1 = new ClientSession(1, c1);
        await _s1.Conn.SendAsync(new WelcomeMsg { playerId = 1 });

        var c2 = await _listener.AcceptTcpClientAsync(ct);
        _s2 = new ClientSession(2, c2);
        await _s2.Conn.SendAsync(new WelcomeMsg { playerId = 2 });

        //_ = _s1.Conn.ReadLoopAsync(line => OnLineAsync(_s1, line), ct);
        //_ = _s2.Conn.ReadLoopAsync(line => OnLineAsync(_s2, line), ct);
        _ = RunClientLoopAsync(_s1, ct);
        _ = RunClientLoopAsync(_s2, ct);

        await BroadcastLobbyAsync();
    }
    private async Task RunClientLoopAsync(ClientSession s, CancellationToken ct)
    {
        try
        {
            await s.Conn.ReadLoopAsync(line => OnLineAsync(s, line), ct);
        }
        catch
        {
            
        }
        finally
        {
            await OnDisconnectedAsync(s);
        }
    }
    private async Task OnDisconnectedAsync(ClientSession s)
    {
        // если игра уже идёт — завершить её
        if (_game != null)
        {
            try { await _game.StopAsync("DISCONNECT"); } catch { }
            _game = null;
            return;
        }

        // если в лобби — просто обновить/сообщить второму
        try { await BroadcastLobbyAsync(); } catch { }
    }
    private async Task OnLineAsync(ClientSession s, string json)
    {
        var msg = MessageCodec.Decode(json);
        if (msg == null) return;

        switch (msg.t)
        {
            case "HELLO":
                {
                    var m = (HelloMsg)msg;
                    s.Player.Name = string.IsNullOrWhiteSpace(m.name) ? $"P{s.Slot}" : m.name.Trim();
                    await BroadcastLobbyAsync();
                    break;
                }
            case "COLOR_PICK":
                {
                    var m = (ColorPickMsg)msg;
                    var color = (m.color ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(color)) break;

                    // unique color rule
                    var other = (s.Slot == 1 ? _s2 : _s1);
                    if (other != null && other.Player.Color.Equals(color, StringComparison.OrdinalIgnoreCase))
                    {
                        await s.Conn.SendAsync(new ErrorMsg { code = "COLOR_TAKEN", msg = "Color is already used" });
                        break;
                    }

                    s.Player.Color = color;
                    await BroadcastLobbyAsync();
                    break;
                }
            case "READY":
                {
                    var m = (ReadyMsg)msg;
                    s.Player.Ready = m.ready;
                    await BroadcastLobbyAsync();
                    await TryStartGameAsync();
                    break;
                }
            case "POP":
                {
                    if (_game == null) break;
                    await _game.HandlePopAsync(s.Slot, (PopMsg)msg);
                    break;
                }
            case "PONG":
                {
                    s.LastPongUtc = DateTime.UtcNow;
                    break;
                }
            case "GET_RATING":
                {
                    var top = ((GetRatingMsg)msg).top;
                    var list = _repo.GetRating(top <= 0 ? 20 : top);
                    await s.Conn.SendAsync(new RatingMsg { items = list.ToArray() });
                    break;
                }
        }
    }

    private async Task BroadcastLobbyAsync()
    {
        if (_s1 == null || _s2 == null) return;

        var m = new LobbyMsg
        {
            players = new[]
            {
                new PlayerInfo{ Id=1, Name=_s1.Player.Name, Color=_s1.Player.Color, Ready=_s1.Player.Ready, Score=_s1.Player.Score },
                new PlayerInfo{ Id=2, Name=_s2.Player.Name, Color=_s2.Player.Color, Ready=_s2.Player.Ready, Score=_s2.Player.Score },
            }
        };
        await Task.WhenAll(_s1.Conn.SendAsync(m), _s2.Conn.SendAsync(m));
    }

    private async Task TryStartGameAsync()
    {
        if (_s1 == null || _s2 == null) return;
        if (_game != null) return;

        if (!_s1.Player.Ready || !_s2.Player.Ready) return;
        if (string.IsNullOrWhiteSpace(_s1.Player.Color) || string.IsNullOrWhiteSpace(_s2.Player.Color)) return;
        if (_s1.Player.Color.Equals(_s2.Player.Color, StringComparison.OrdinalIgnoreCase)) return;

        _s1.LastPongUtc = DateTime.UtcNow;
        _s2.LastPongUtc = DateTime.UtcNow;
        // start
        _cfg.RoundMs = 30_000;
        _cfg.FieldW = 800;
        _cfg.FieldH = 450;
        _cfg.Level = Level;
        _cfg.TournamentMode = TournamentMode;
        _cfg.TournamentRounds = TournamentRounds;
        _game = new ServerGameCore(_s1, _s2, _repo, _cfg, seed: Environment.TickCount);
        await _game.StartAsync();
    }
}
