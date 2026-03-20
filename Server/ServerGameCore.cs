using ClientServerTcpGame.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ClientServerTcpGame.Server;

public sealed class ServerGameCore
{
    private readonly ClientSession _s1;
    private readonly ClientSession _s2;
    private readonly RatingRepository _repo;
    private readonly GameConfig _cfg;

    private readonly Dictionary<int, Ball> _balls = new();
    private int _nextBallId = 100;
    private int _pairCount;

    private readonly Random _rng;
    private DateTime _startedAtUtc;
    private CancellationTokenSource? _cts;


    private int _hitsOwn1, _hitsEnemy1, _hitsNeutral1, _miss1;
    private int _hitsOwn2, _hitsEnemy2, _hitsNeutral2, _miss2;

    private int _roundNo = 1;
    private bool _tournamentInited;
    private bool _stopping;

    private int _tTotal1, _tTotal2;
    private readonly int[] _tRounds1 = new int[3];
    private readonly int[] _tRounds2 = new int[3];

    private static readonly string[] GamePalette =
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

    public ServerGameCore(ClientSession s1, ClientSession s2, RatingRepository repo, GameConfig cfg, int seed)
    {
        _s1 = s1;
        _s2 = s2;
        _repo = repo;
        _cfg = cfg;
        _rng = new Random(seed);
    }

    public async Task StartAsync()
    {
        _cts = new CancellationTokenSource();
        _startedAtUtc = DateTime.UtcNow;
        _stopping = false;

        if (_cfg.TournamentMode && !_tournamentInited)
        {
            _tournamentInited = true;
            _roundNo = 1;
            _tTotal1 = _tTotal2 = 0;
            Array.Clear(_tRounds1);
            Array.Clear(_tRounds2);

            _repo.ClearTournament(); 
        }

        var startAtMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 300; // small sync delay
        await BroadcastAsync(new StartMsg
        {
            roundMs = _cfg.RoundMs,
            seed = 0,
            startAtMs = startAtMs,
            fieldW = _cfg.FieldW,
            fieldH = _cfg.FieldH,
            level = _cfg.Level,
            tournament = _cfg.TournamentMode,
            roundNo = _cfg.RoundNo,
            totalRounds = _cfg.TournamentMode ? _cfg.TournamentRounds : 1
        });

        _ = Task.Run(() => SpawnLoopAsync(_cts.Token));
        _ = Task.Run(() => HeartbeatLoopAsync(_cts.Token));
        _ = Task.Run(() => RoundEndAsync(_cts.Token));
    }

    public async Task StopAsync(string reason)
    {
        if (_cts == null) return;
        if (_stopping) return;
        _stopping = true;
        _cts.Cancel();

        var duration = (int)(DateTime.UtcNow - _startedAtUtc).TotalMilliseconds;

        var p1 = _s1.Player;
        var p2 = _s2.Player;

        long matchId = 0;

        var s1 = new PlayerMatchStats { id = 1, hitsOwn = _hitsOwn1, hitsEnemy = _hitsEnemy1, hitsNeutral = _hitsNeutral1, miss = _miss1 };
        var s2 = new PlayerMatchStats { id = 2, hitsOwn = _hitsOwn2, hitsEnemy = _hitsEnemy2, hitsNeutral = _hitsNeutral2, miss = _miss2 };

        if (string.Equals(reason, "TIME", StringComparison.OrdinalIgnoreCase))
        {
            matchId = _repo.InsertMatch(_startedAtUtc, duration, reason, p1, p2, s1, s2);
            if (_cfg.TournamentMode)
            {
                _repo.InsertTournamentRound(_roundNo, 1, p1.Name, p1.Color, p1.Score, DateTime.UtcNow);
                _repo.InsertTournamentRound(_roundNo, 2, p2.Name, p2.Color, p2.Score, DateTime.UtcNow);

                int idx = Math.Clamp(_roundNo - 1, 0, 2);
                _tRounds1[idx] = p1.Score;
                _tRounds2[idx] = p2.Score;
                _tTotal1 += p1.Score;
                _tTotal2 += p2.Score;
            }
        }
        await BroadcastAsync(new EndMsg
        {
            reason = reason,
            matchId = matchId,
            final = new[]
            {
                new ScoreItem{ id=1, value=p1.Score },
                new ScoreItem{ id=2, value=p2.Score }
            },
            stats = new[] { s1, s2 }
        });

        if (_cfg.TournamentMode && string.Equals(reason, "TIME", StringComparison.OrdinalIgnoreCase))
        {
            if (_roundNo < _cfg.TournamentRounds)
            {
                _roundNo++;
                ResetRoundState();

                await Task.Delay(1500);
                _cfg.RoundNo = _roundNo;
                await StartAsync(); // старт следующего раунда
            }
            else
            {
                // финал турнира
                var rows = BuildTournamentRows();
                await BroadcastAsync(new TournamentResultMsg { rows = rows });
            }
        }
    }

    public async Task HandlePopAsync(int byPlayerId, PopMsg msg)
    {
        if (!_balls.TryGetValue(msg.ballId, out var ball) || ball.Popped)
        {
            IncMiss(byPlayerId);
            await BroadcastAsync(new PopResultMsg
            {
                ballId = msg.ballId,
                byPlayerId = byPlayerId,
                ok = false,
                delta = 0,
                score = ScoreArray()
            });
            return;
        }

        ball.Popped = true;

        int delta = ScoreDelta(byPlayerId, ball.Owner);
        ApplyScore(byPlayerId, delta);
        IncHit(byPlayerId, ball.Owner);

        await BroadcastAsync(new PopResultMsg
        {
            ballId = ball.Id,
            byPlayerId = byPlayerId,
            ok = true,
            delta = delta,
            score = ScoreArray()
        });
    }

    private int ScoreDelta(int playerId, int owner)
    {
        if (owner == playerId) return +5;
        if (owner == 0) return -1;
        return -5;
    }

    private void ApplyScore(int playerId, int delta)
    {
        if (playerId == 1) _s1.Player.Score += delta;
        else _s2.Player.Score += delta;
    }

    private void IncMiss(int pid)
    {
        if (pid == 1) _miss1++;
        else _miss2++;
    }

    private void IncHit(int pid, int owner)
    {
        if (pid == 1)
        {
            if (owner == 1) _hitsOwn1++;
            else if (owner == 2) _hitsEnemy1++;
            else _hitsNeutral1++;
        }
        else
        {
            if (owner == 2) _hitsOwn2++;
            else if (owner == 1) _hitsEnemy2++;
            else _hitsNeutral2++;
        }
    }

    private ScoreItem[] ScoreArray() => new[]
    {
        new ScoreItem{ id=1, value=_s1.Player.Score },
        new ScoreItem{ id=2, value=_s2.Player.Score }
    };

    private async Task SpawnLoopAsync(CancellationToken ct)
    {

        while (!ct.IsCancellationRequested)
        {
            var spawnedAll = new List<Ball>();

            int pairs = Math.Clamp(_cfg.Level, 1, 5); // L1..L5 => 1..5 пар
            for (int i = 0; i < pairs; i++)
            {
                var spawned = SpawnPairWithOptionalNeutral();
                spawnedAll.AddRange(spawned);
            }

            await BroadcastAsync(new SpawnMsg { balls = spawnedAll.ToArray() });

            // чем выше уровень, тем меньше задержка => больше шаров
            int min = Math.Max(120, 400 / _cfg.Level);
            int max = Math.Max(180, 700 / _cfg.Level);
            await Task.Delay(_rng.Next(min, max + 1), ct);
        }
    }
    private Ball[] SpawnPairWithOptionalNeutral()
    {
        _pairCount++;

        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var b1 = MakeRandomBall(owner: 1, color: _s1.Player.Color, nowMs: now);
        var b2 = MakeRandomBall(owner: 2, color: _s2.Player.Color, nowMs: now);

        var n = MakeNeutralBall(now);

        _balls[b1.Id] = b1;
        _balls[b2.Id] = b2;
        _balls[n.Id] = n;

        return [ b1, b2, n ];
    }
    private Ball MakeNeutralBall(long nowMs)
    {
        string p1 = _s1.Player.Color;
        string p2 = _s2.Player.Color;

        var available = GamePalette
            .Where(c =>
                !c.Equals(p1, StringComparison.OrdinalIgnoreCase) &&
                !c.Equals(p2, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        string color = available[_rng.Next(available.Length)];

        return MakeRandomBall(owner: 0, color: color, nowMs: nowMs);
    }
    private Ball MakeRandomBall(int owner, string color, long nowMs)
    {
        double r = _rng.Next(14, 23);

        // старт сверху, но чуть рандомнее по Y (чтобы выглядело живее)
        double x = _rng.Next((int)r, _cfg.FieldW - (int)r);
        double y = _rng.Next(0, 11);

        // под разными углами vx широкий, vy всегда вниз и достаточно большой
        double vx = _rng.Next(-180, 181);     // -120..120,
        double vy = _rng.Next(180, 321);      // вниз (положительное)
        double speedMult = 1.0 + 0.35 * (_cfg.Level - 1);
        vx *= speedMult;
        vy *= speedMult;
        return new Ball
        {
            Id = ++_nextBallId,
            Owner = owner,
            Color = color,
            X = x,
            Y = y,
            Vx = vx,
            Vy = vy,
            R = r,
            SpawnAtMs = nowMs
        };
    }

    private async Task HeartbeatLoopAsync(CancellationToken ct)
    {
        int n = 0;
        while (!ct.IsCancellationRequested)
        {
            n++;
            await BroadcastAsync(new PingMsg { n = n });

            // timeout check (3s)
            var now = DateTime.UtcNow;
            if ((now - _s1.LastPongUtc).TotalSeconds > 3 || (now - _s2.LastPongUtc).TotalSeconds > 3)
            {
                await StopAsync("DISCONNECT");
                return;
            }

            await Task.Delay(1000, ct);
        }
    }

    private async Task RoundEndAsync(CancellationToken ct)
    {
        try
        {
            await Task.Delay(_cfg.RoundMs, ct);
            await StopAsync("TIME");
        }
        catch (OperationCanceledException) { }
    }

    //private Task BroadcastAsync(object msg)
    //    => Task.WhenAll(_s1.Conn.SendAsync(msg), _s2.Conn.SendAsync(msg));
    private async Task BroadcastAsync(object msg)
    {
        await SafeSendAsync(_s1, msg);
        await SafeSendAsync(_s2, msg);
    }
    private static async Task SafeSendAsync(ClientSession s, object msg)
    {
        try { await s.Conn.SendAsync(msg); }
        catch { /*  */ }
    }

    private void ResetRoundState()
    {
        _balls.Clear();

        _hitsOwn1 = _hitsEnemy1 = _hitsNeutral1 = _miss1 = 0;
        _hitsOwn2 = _hitsEnemy2 = _hitsNeutral2 = _miss2 = 0;

        _s1.Player.Score = 0;
        _s2.Player.Score = 0;
    }

    private TournamentRow[] BuildTournamentRows()
    {
        var p1 = _s1.Player;
        var p2 = _s2.Player;

        var a = new TournamentRow
        {
            playerId = 1,
            name = p1.Name,
            color = p1.Color,
            totalScore = _tTotal1,
            r1 = _tRounds1[0],
            r2 = _tRounds1[1],
            r3 = _tRounds1[2]
        };

        var b = new TournamentRow
        {
            playerId = 2,
            name = p2.Name,
            color = p2.Color,
            totalScore = _tTotal2,
            r1 = _tRounds2[0],
            r2 = _tRounds2[1],
            r3 = _tRounds2[2]
        };

        var ordered = new[] { a, b }
            .OrderByDescending(x => x.totalScore)
            .ThenBy(x => x.playerId)
            .ToArray();

        for (int i = 0; i < ordered.Length; i++)
            ordered[i].place = i + 1;

        return ordered;
    }
}
