using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using ClientServerTcpGame.Shared;

namespace ClientServerTcpGame.Server;

public sealed class RatingRepository
{
    private readonly string _cs;

    public RatingRepository(string dbPath = "stats.sqlite")
    {
        _cs = new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString();
        Init();
    }

    private void Init()
    {
        using var con = new SqliteConnection(_cs);
        con.Open();

        var cmd = con.CreateCommand();
        cmd.CommandText =
        """
        CREATE TABLE IF NOT EXISTS matches(
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            startedAt TEXT NOT NULL,
            durationMs INTEGER NOT NULL,
            reason TEXT NOT NULL,
            p1Name TEXT NOT NULL,
            p2Name TEXT NOT NULL,
            p1Color TEXT NOT NULL,
            p2Color TEXT NOT NULL,
            p1Score INTEGER NOT NULL,
            p2Score INTEGER NOT NULL
        );

        CREATE TABLE IF NOT EXISTS match_stats(
            matchId INTEGER NOT NULL,
            playerId INTEGER NOT NULL,
            hitsOwn INTEGER NOT NULL,
            hitsEnemy INTEGER NOT NULL,
            hitsNeutral INTEGER NOT NULL,
            miss INTEGER NOT NULL
        );

        CREATE TABLE IF NOT EXISTS tournament_rounds(
            roundNo INTEGER NOT NULL,
            playerId INTEGER NOT NULL,
            playerName TEXT NOT NULL,
            playerColor TEXT NOT NULL,
            score INTEGER NOT NULL,
            endedAt TEXT NOT NULL
        );
        """;
        cmd.ExecuteNonQuery();
    }

    public long InsertMatch(DateTime startedAtUtc, int durationMs, string reason,
        PlayerInfo p1, PlayerInfo p2, PlayerMatchStats s1, PlayerMatchStats s2)
    {
        using var con = new SqliteConnection(_cs);
        con.Open();

        using var tx = con.BeginTransaction();

        var m = con.CreateCommand();
        m.Transaction = tx;
        m.CommandText =
        """
        INSERT INTO matches(startedAt,durationMs,reason,p1Name,p2Name,p1Color,p2Color,p1Score,p2Score)
        VALUES($startedAt,$durationMs,$reason,$p1Name,$p2Name,$p1Color,$p2Color,$p1Score,$p2Score);
        SELECT last_insert_rowid();
        """;
        m.Parameters.AddWithValue("$startedAt", startedAtUtc.ToString("O"));
        m.Parameters.AddWithValue("$durationMs", durationMs);
        m.Parameters.AddWithValue("$reason", reason);
        m.Parameters.AddWithValue("$p1Name", p1.Name);
        m.Parameters.AddWithValue("$p2Name", p2.Name);
        m.Parameters.AddWithValue("$p1Color", p1.Color);
        m.Parameters.AddWithValue("$p2Color", p2.Color);
        m.Parameters.AddWithValue("$p1Score", p1.Score);
        m.Parameters.AddWithValue("$p2Score", p2.Score);

        var matchId = (long)(m.ExecuteScalar() ?? 0L);

        void InsertStats(PlayerMatchStats s)
        {
            var c = con.CreateCommand();
            c.Transaction = tx;
            c.CommandText =
            """
            INSERT INTO match_stats(matchId,playerId,hitsOwn,hitsEnemy,hitsNeutral,miss)
            VALUES($mid,$pid,$ho,$he,$hn,$m);
            """;
            c.Parameters.AddWithValue("$mid", matchId);
            c.Parameters.AddWithValue("$pid", s.id);
            c.Parameters.AddWithValue("$ho", s.hitsOwn);
            c.Parameters.AddWithValue("$he", s.hitsEnemy);
            c.Parameters.AddWithValue("$hn", s.hitsNeutral);
            c.Parameters.AddWithValue("$m", s.miss);
            c.ExecuteNonQuery();
        }

        InsertStats(s1);
        InsertStats(s2);

        tx.Commit();
        return matchId;
    }

    public List<RatingItem> GetRating(int top)
    {
        using var con = new SqliteConnection(_cs);
        con.Open();

        //avg score + wins + aggregated click stats from match_stats
        var cmd = con.CreateCommand();
        cmd.CommandText =
        """
        WITH pm AS (
            SELECT id AS matchId, p1Name AS name, p1Score AS score, p2Score AS oppScore FROM matches
            UNION ALL
            SELECT id AS matchId, p2Name AS name, p2Score AS score, p1Score AS oppScore FROM matches
        ),
        ps AS (
            SELECT
                ms.matchId,
                CASE ms.playerId
                    WHEN 1 THEN m.p1Name
                    WHEN 2 THEN m.p2Name
                    ELSE ''
                END AS name,
                ms.hitsOwn    AS ownClicks,
                ms.hitsEnemy  AS enemyClicks,
                ms.hitsNeutral AS neutralClicks,
                (ms.hitsOwn + ms.hitsEnemy + ms.hitsNeutral) AS totalClicks
            FROM match_stats ms
            JOIN matches m ON m.id = ms.matchId
            WHERE ms.playerId IN (1,2)
        )
        SELECT
            pm.name,
            COUNT(*) AS games,
            AVG(pm.score) AS avgScore,
            SUM(pm.score) AS totalScore,
            SUM(CASE WHEN pm.score > pm.oppScore THEN 1 ELSE 0 END) AS wins,

            COALESCE(SUM(ps.totalClicks), 0) AS totalClicks,
            COALESCE(SUM(ps.ownClicks), 0) AS ownClicks,
            COALESCE(SUM(ps.enemyClicks), 0) AS enemyClicks,
            COALESCE(SUM(ps.neutralClicks), 0) AS neutralClicks,

            COALESCE(AVG(ps.totalClicks), 0) AS avgTotalClicks,
            COALESCE(AVG(ps.ownClicks), 0) AS avgOwnClicks,
            COALESCE(AVG(ps.enemyClicks), 0) AS avgEnemyClicks,
            COALESCE(AVG(ps.neutralClicks), 0) AS avgNeutralClicks
        FROM pm
        LEFT JOIN ps ON ps.matchId = pm.matchId AND ps.name = pm.name
        GROUP BY pm.name
        ORDER BY avgScore DESC
        LIMIT $top;
        """;
        cmd.Parameters.AddWithValue("$top", top <= 0 ? 20 : top);

        var res = new List<RatingItem>();
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            res.Add(new RatingItem
            {
                name = r.GetString(0),
                games = r.GetInt32(1),
                avgScore = r.GetDouble(2),
                totalScore = r.GetInt32(3),  
                wins = r.GetInt32(4),

                totalClicks = r.GetInt32(5),
                ownClicks = r.GetInt32(6),
                enemyClicks = r.GetInt32(7),
                neutralClicks = r.GetInt32(8),

                avgTotalClicks = r.GetDouble(9),
                avgOwnClicks = r.GetDouble(10),
                avgEnemyClicks = r.GetDouble(11),
                avgNeutralClicks = r.GetDouble(12),
            });
        }

        return res;
    }
    public void ClearTournament()
    {
        using var con = new SqliteConnection(_cs);
        con.Open();
        using var cmd = con.CreateCommand();
        cmd.CommandText = "DELETE FROM tournament_rounds;";
        cmd.ExecuteNonQuery();
    }

    public void InsertTournamentRound(int roundNo, int playerId, string playerName, string playerColor, int score, DateTime endedAtUtc)
    {
        using var con = new SqliteConnection(_cs);
        con.Open();
        using var cmd = con.CreateCommand();
        cmd.CommandText =
        """
    INSERT INTO tournament_rounds(roundNo,playerId,playerName,playerColor,score,endedAt)
    VALUES($r,$pid,$n,$c,$s,$t);
    """;
        cmd.Parameters.AddWithValue("$r", roundNo);
        cmd.Parameters.AddWithValue("$pid", playerId);
        cmd.Parameters.AddWithValue("$n", playerName);
        cmd.Parameters.AddWithValue("$c", playerColor);
        cmd.Parameters.AddWithValue("$s", score);
        cmd.Parameters.AddWithValue("$t", endedAtUtc.ToString("O"));
        cmd.ExecuteNonQuery();
    }
}
