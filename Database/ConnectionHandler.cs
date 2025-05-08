using System.Net.Sockets;
using System.Text;
using ShitDB.DataSystem;

namespace ShitDB.Database;

public class ConnectionHandler(ILogger<ConnectionHandler> logger, QueryDecoder decoder)
{
    public async void HandleConnection(TcpClient client, CancellationToken stoppingToken)
    {
        try
        {
            var stream = client.GetStream();
            var buffer = new byte[1024];

            var query = new StringBuilder(1024);

            while (!stoppingToken.IsCancellationRequested && client.Connected)
            {
                var read = await stream.ReadAsync(buffer, 0, buffer.Length, stoppingToken);
                if (read <= 0)
                {
                    logger.LogInformation("Client disconnected");
                    return;
                }

                query.Append(Encoding.UTF8.GetString(buffer, 0, read));
                logger.LogDebug(query.ToString());
                if (buffer[read - 1] == ';') // todo: rework so that any semicolon counts
                {
                    logger.LogDebug(query.ToString());
                    var result = await decoder.DecodeQuery(query.ToString());
                    string response;

                    if (result.IsErr())
                    {
                        logger.LogError(result.UnwrapErr(), $"Error occured during executing query {query}");
                        response = result.UnwrapErr().Message;
                    }
                    else
                    {
                        var values = result.Unwrap();
                        var lines = values.Select(val => string.Join(", ", val.Entries)).ToList();
                        response = string.Join('\n', lines);
                    }

                    var responseBuffer = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseBuffer, 0, responseBuffer.Length, stoppingToken);
                    query.Clear();
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unexpected error occured while handling connection.");
        }
    }
}