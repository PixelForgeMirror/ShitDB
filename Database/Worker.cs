using System.Net.Sockets;
using Database.Config;
using Microsoft.Extensions.Options;

namespace Database;

public class Worker(ILogger<Worker> logger, IOptions<DatabaseConfig> dbConfig) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        TcpListener serverSocket = new(System.Net.IPAddress.Any, dbConfig.Value.Port);
        serverSocket.Start();
        logger.LogInformation($"Server running at {serverSocket.LocalEndpoint}");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var client = await serverSocket.AcceptTcpClientAsync(stoppingToken);
            logger.LogDebug($"Client connected from {client.Client.RemoteEndPoint}");
        }
    }
}