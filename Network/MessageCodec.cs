using System;
using System.Text.Json;
using ClientServerTcpGame.Shared;

namespace ClientServerTcpGame.Network;

public static class MessageCodec
{
    //detect t, then deserialize to concrete type
    public static IMsg? Decode(string json)
    {
        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("t", out var tProp)) return null;

        var t = tProp.GetString() ?? "";
        return t switch
        {
            "HELLO" => JsonSerializer.Deserialize<HelloMsg>(json),
            "WELCOME" => JsonSerializer.Deserialize<WelcomeMsg>(json),
            "COLOR_PICK" => JsonSerializer.Deserialize<ColorPickMsg>(json),
            "READY" => JsonSerializer.Deserialize<ReadyMsg>(json),
            "LOBBY" => JsonSerializer.Deserialize<LobbyMsg>(json),
            "START" => JsonSerializer.Deserialize<StartMsg>(json),
            "SPAWN" => JsonSerializer.Deserialize<SpawnMsg>(json),
            "POP" => JsonSerializer.Deserialize<PopMsg>(json),
            "POP_RESULT" => JsonSerializer.Deserialize<PopResultMsg>(json),
            "END" => JsonSerializer.Deserialize<EndMsg>(json),
            "PING" => JsonSerializer.Deserialize<PingMsg>(json),
            "PONG" => JsonSerializer.Deserialize<PongMsg>(json),
            "GET_RATING" => JsonSerializer.Deserialize<GetRatingMsg>(json),
            "RATING" => JsonSerializer.Deserialize<RatingMsg>(json),
            "ERROR" => JsonSerializer.Deserialize<ErrorMsg>(json),
            "TOURNAMENT_RESULT" => JsonSerializer.Deserialize<TournamentResultMsg>(json),
            _ => null
        };
    }
}
