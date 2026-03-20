//namespace ClientServerTcpGame.Shared
using System;

namespace ClientServerTcpGame.Shared;

public sealed class PlayerInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Color { get; set; } = "#FFFFFF";
    public bool Ready { get; set; }
    public int Score { get; set; }
}

public sealed class Ball
{
    public int Id { get; set; }
    public int Owner { get; set; }          // 1,2 or 0(neutral optional)
    public string Color { get; set; } = "#FFFFFF";

    public double X { get; set; }
    public double Y { get; set; }
    public double Vx { get; set; }
    public double Vy { get; set; }
    public double R { get; set; }

    public bool Popped { get; set; }
    public long SpawnAtMs { get; set; }     // server timestamp (unix ms)
}

public sealed class GameConfig
{
    public int RoundMs { get; set; } = 30_000;
    public int FieldW { get; set; } = 800;
    public int FieldH { get; set; } = 450;
    public int Level { get; set; } = 1;
    public bool TournamentMode { get; set; } = false;
    public int TournamentRounds { get; set; } = 3;
    public int RoundNo { get; set; } = 1;
}

