using System.Net.Sockets;
using System.Text;
using DataSystem;

namespace Database;

public class ConnectionHandler(ILogger<ConnectionHandler> logger, QueryDecoder decoder)
{
    public async void HandleConnection(TcpClient client, CancellationToken stoppingToken)
    {
        var stream = client.GetStream();
        byte[] buffer = new byte[1024];

        StringBuilder query = new StringBuilder(1024);
        
        while (!stoppingToken.IsCancellationRequested && client.Connected)
        {
            int read = await stream.ReadAsync(buffer, 0, buffer.Length, stoppingToken);
            query.Append(Encoding.UTF8.GetString(buffer, 0, read));
            logger.LogDebug(query.ToString());
            if (buffer[read - 1] == ';') // todo: rework so that any semicolon counts
            {
                logger.LogDebug(query.ToString());
                await decoder.DecodeQuery(query.ToString());
                query.Clear();
            }
        }
    }
}