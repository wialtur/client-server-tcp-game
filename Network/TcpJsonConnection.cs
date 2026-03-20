using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ClientServerTcpGame.Network;

public sealed class TcpJsonConnection : IDisposable
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;

    public TcpJsonConnection(TcpClient client)
    {
        _client = client;
        _stream = client.GetStream();
        _reader = new StreamReader(_stream, Encoding.UTF8, leaveOpen: true);
        _writer = new StreamWriter(_stream, new UTF8Encoding(false)) { AutoFlush = true, NewLine = "\n" };
    }

    public async Task SendAsync(object msg, CancellationToken ct = default)
    {
        // single line JSON + '\n'
        var json = JsonSerializer.Serialize(msg);
        await _writer.WriteLineAsync(json.AsMemory(), ct);
    }

    public async Task ReadLoopAsync(Func<string, Task> onLine, CancellationToken ct = default)
    {
        while (!ct.IsCancellationRequested)
        {
            var line = await _reader.ReadLineAsync(ct);
            if (line == null) break; // disconnected
            if (line.Length == 0) continue;
            await onLine(line);
        }
    }

    public void Dispose()
    {
        try { _writer.Dispose(); } catch { }
        try { _reader.Dispose(); } catch { }
        try { _stream.Dispose(); } catch { }
        try { _client.Close(); } catch { }
    }
}
