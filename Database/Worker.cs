using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Options;
using ShitDB.Config;

namespace ShitDB.Database;

public class Worker(ILogger<Worker> logger, IOptions<DatabaseConfig> dbConfig, ConnectionHandler connectionHandler)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        TcpListener serverSocket = new(IPAddress.Any, dbConfig.Value.Port);
        serverSocket.Start();
        logger.LogInformation($"Server running at {serverSocket.LocalEndpoint}");

        while (!stoppingToken.IsCancellationRequested)
        {
            var client = await serverSocket.AcceptTcpClientAsync(stoppingToken);
            logger.LogDebug($"Client connected from {client.Client.RemoteEndPoint}");

            connectionHandler.HandleConnection(client, stoppingToken); // do not await on purpose
        }
    }
}