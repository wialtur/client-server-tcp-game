namespace ClientServerTcpGame.Shared;

public interface IMsg { string t { get; set; } }

// --- connect / lobby ---
public sealed class HelloMsg : IMsg
{
    public string t { get; set; } = "HELLO";
    public string name { get; set; } = "";
}

public sealed class WelcomeMsg : IMsg
{
    public string t { get; set; } = "WELCOME";
    public int playerId { get; set; }
    public string serverVersion { get; set; } = "1";
}

public sealed class ColorPickMsg : IMsg
{
    public string t { get; set; } = "COLOR_PICK";
    public string color { get; set; } = "#FFFFFF";
}

public sealed class ReadyMsg : IMsg
{
    public string t { get; set; } = "READY";
    public bool ready { get; set; }
}

public sealed class LobbyMsg : IMsg
{
    public string t { get; set; } = "LOBBY";
    public PlayerInfo[] players { get; set; } = [];
}

public sealed class ErrorMsg : IMsg
{
    public string t { get; set; } = "ERROR";
    public string code { get; set; } = "";
    public string msg { get; set; } = "";
}

// --- game ---
public sealed class StartMsg : IMsg
{
    public string t { get; set; } = "START";
    public int roundMs { get; set; }
    public int seed { get; set; }
    public long startAtMs { get; set; }
    public int fieldW { get; set; }
    public int fieldH { get; set; }
    public int level { get; set; } = 1;
    public bool tournament { get; set; } = false;
    public int roundNo { get; set; } = 1;
    public int totalRounds { get; set; } = 1;
}

public sealed class SpawnMsg : IMsg
{
    public string t { get; set; } = "SPAWN";
    public Ball[] balls { get; set; } = [];
}

public sealed class PopMsg : IMsg
{
    public string t { get; set; } = "POP";
    public int ballId { get; set; }
    public double clickX { get; set; }
    public double clickY { get; set; }
}

public sealed class ScoreItem
{
    public int id { get; set; }
    public int value { get; set; }
}

public sealed class PopResultMsg : IMsg
{
    public string t { get; set; } = "POP_RESULT";
    public int ballId { get; set; }
    public int byPlayerId { get; set; }
    public bool ok { get; set; }
    public int delta { get; set; }
    public ScoreItem[] score { get; set; } = [];
}

public sealed class EndMsg : IMsg
{
    public string t { get; set; } = "END";
    public string reason { get; set; } = "TIME"; // TIME / DISCONNECT
    public ScoreItem[] final { get; set; } = [];
    public long matchId { get; set; }
    public PlayerMatchStats[] stats { get; set; } = [];
}

public sealed class PlayerMatchStats
{
    public int id { get; set; }
    public int hitsOwn { get; set; }
    public int hitsEnemy { get; set; }
    public int hitsNeutral { get; set; }
    public int miss { get; set; }
}

// --- heartbeat ---
public sealed class PingMsg : IMsg
{
    public string t { get; set; } = "PING";
    public int n { get; set; }
}

public sealed class PongMsg : IMsg
{
    public string t { get; set; } = "PONG";
    public int n { get; set; }
}

// --- rating ---
public sealed class GetRatingMsg : IMsg
{
    public string t { get; set; } = "GET_RATING";
    public int top { get; set; } = 20;
}

public sealed class RatingItem
{
    public string name { get; set; } = "";
    public int games { get; set; }
    public double avgScore { get; set; }
    public int wins { get; set; }
    public int totalScore { get; set; }

    public int totalClicks { get; set; }      // sum(own+enemy+neutral)
    public int ownClicks { get; set; }        // sum(own)
    public int enemyClicks { get; set; }      // sum(enemy)
    public int neutralClicks { get; set; }    // sum(neutral)

    public double avgTotalClicks { get; set; } // avg per game
    public double avgOwnClicks { get; set; }
    public double avgEnemyClicks { get; set; }
    public double avgNeutralClicks { get; set; }
}

public sealed class RatingMsg : IMsg
{
    public string t { get; set; } = "RATING";
    public RatingItem[] items { get; set; } = [];
}

public sealed class TournamentRow
{
    public int place { get; set; }
    public int playerId { get; set; }
    public string name { get; set; } = "";
    public string color { get; set; } = "";
    public int totalScore { get; set; }
    public int r1 { get; set; }
    public int r2 { get; set; }
    public int r3 { get; set; }
}

public sealed class TournamentResultMsg : IMsg
{
    public string t { get; set; } = "TOURNAMENT_RESULT";
    public TournamentRow[] rows { get; set; } = [];
}