using ClientServerTcpGame;
using ClientServerTcpGame.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ClientServerTcpGame.Views;

public partial class GameView : UserControl
{
    private readonly MainWindow _mw;

    // ===== server time / score =====
    private long _serverStartAtMs;
    private int _roundMs;

    private int _score1;
    private int _score2;

    private bool _isTournament;
    private int _roundNo = 1;
    private int _totalRounds = 1;

    // ===== animation =====
    private sealed class BallState
    {
        public int Id;
        public double X, Y, Vx, Vy, R;
        public Ellipse Shape = null!;
    }

    private readonly Dictionary<int, BallState> _balls = new();

    private readonly DispatcherTimer _timer =
        new() { Interval = TimeSpan.FromMilliseconds(16) }; // ~60 FPS

    private readonly Stopwatch _sw = new();
    private long _lastMs;

    public void ApplyStart(StartMsg msg)
    {
        _serverStartAtMs = msg.startAtMs;
        _roundMs = msg.roundMs;

        _isTournament = msg.tournament;
        _roundNo = msg.roundNo <= 0 ? 1 : msg.roundNo;
        _totalRounds = msg.totalRounds <= 0 ? 1 : msg.totalRounds;

        // сброс счёта/визуалов при новом старте (на всякий)
        _score1 = 0;
        _score2 = 0;
    }
    public GameView(MainWindow mw)
    {
        InitializeComponent();
        _mw = mw;

        // network events
        _mw.Client!.OnSpawn += OnSpawn;
        _mw.Client.OnPopResult += OnPopResult;
        _mw.Client.OnStart += OnStart;

        Loaded += (_, _) => StartAnim();
        Unloaded += (_, _) => StopAnim();

        _timer.Tick += (_, _) => TickAnim();
    }

    // ================= animation lifecycle =================

    private void StartAnim()
    {
        _sw.Restart();
        _lastMs = _sw.ElapsedMilliseconds;
        _timer.Start();
    }

    private void StopAnim()
    {
        _timer.Stop();
        _sw.Stop();

        // unsubscribe to avoid duplicates
        if (_mw.Client != null)
        {
            _mw.Client.OnSpawn -= OnSpawn;
            _mw.Client.OnPopResult -= OnPopResult;
            _mw.Client.OnStart -= OnStart;
        }
    }

    private void TickAnim()
    {
        var now = _sw.ElapsedMilliseconds;
        var dt = (now - _lastMs) / 1000.0;
        _lastMs = now;

        UpdateTopBar();

        double w = Field.ActualWidth > 0 ? Field.ActualWidth : 800;
        double h = Field.ActualHeight > 0 ? Field.ActualHeight : 450;

        List<int>? toRemove = null;

        foreach (var kv in _balls)
        {
            var b = kv.Value;

            b.X += b.Vx * dt;
            b.Y += b.Vy * dt;

            Canvas.SetLeft(b.Shape, b.X - b.R);
            Canvas.SetTop(b.Shape, b.Y - b.R);

            // out of bounds
            if (b.Y > h + b.R || b.X < -b.R || b.X > w + b.R)
            {
                (toRemove ??= new()).Add(b.Id);
            }
        }

        if (toRemove != null)
        {
            foreach (var id in toRemove)
            {
                if (_balls.TryGetValue(id, out var st))
                {
                    Field.Children.Remove(st.Shape);
                    _balls.Remove(id);
                }
            }
        }
    }

    // ================= UI (timer + score) =================

    private void UpdateTopBar()
    {
        if (_serverStartAtMs <= 0 || _roundMs <= 0) return;

        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long elapsed = nowMs - _serverStartAtMs;
        long left = _roundMs - elapsed;
        if (left < 0) left = 0;

        TimerText.Text = $"Time: {left / 1000.0:0.0}";
        ScoreText.Text = $"Score: {_score1} : {_score2}";
        if (_isTournament && _totalRounds > 1)
        {
            RoundText.Visibility = Visibility.Visible;
            RoundText.Text = $"Round: {_roundNo}/{_totalRounds}";
        }
        else
        {
            RoundText.Visibility = Visibility.Collapsed;
            RoundText.Text = "";
        }
    }

    // ================= network handlers =================

    private void OnStart(StartMsg msg)
    {
        ApplyStart(msg);
    }

    private void OnSpawn(SpawnMsg msg)
    {
        Dispatcher.Invoke(() =>
        {
            foreach (var b in msg.balls)
            {
                if (_balls.ContainsKey(b.Id)) continue;

                var e = new Ellipse
                {
                    Width = b.R * 2,
                    Height = b.R * 2,
                    Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(b.Color)
                };

                var st = new BallState
                {
                    Id = b.Id,
                    X = b.X,
                    Y = b.Y,
                    Vx = b.Vx,
                    Vy = b.Vy,
                    R = b.R,
                    Shape = e
                };

                _balls[b.Id] = st;
                Field.Children.Add(e);

                Canvas.SetLeft(e, st.X - st.R);
                Canvas.SetTop(e, st.Y - st.R);
            }
        });
    }

    private void OnPopResult(PopResultMsg msg)
    {
        if (!msg.ok) return;

        // update score from server
        if (msg.score != null)
        {
            foreach (var s in msg.score)
            {
                if (s.id == 1) _score1 = s.value;
                else if (s.id == 2) _score2 = s.value;
            }
        }

        Dispatcher.Invoke(() =>
        {
            if (_balls.TryGetValue(msg.ballId, out var st))
            {
                Field.Children.Remove(st.Shape);
                _balls.Remove(msg.ballId);
            }
        });
    }

    // ================= input =================

    private async void Field_Click(object sender, MouseButtonEventArgs e)
    {
        var pos = e.GetPosition(Field);

        foreach (var kv in _balls)
        {
            var b = kv.Value;

            var dx = pos.X - b.X;
            var dy = pos.Y - b.Y;

            if (dx * dx + dy * dy <= b.R * b.R)
            {
                await _mw.Client!.PopAsync(b.Id, pos.X, pos.Y);
                break;
            }
        }
    }
}
