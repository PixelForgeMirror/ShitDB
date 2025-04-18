// See https://aka.ms/new-console-template for more information

using System.Net.Sockets;
using System.Text;

TcpClient client = new();

client.Connect("127.0.0.1", 6543);

var stream = client.GetStream();


Console.WriteLine("Connected to database on localhost:6543");

while (true)
{
    var line = Console.ReadLine();
    if (line == "exit")
        break;
    if (line is null)
        continue;
    stream.Write(Encoding.UTF8.GetBytes(line));
    Console.WriteLine("Sent query to server");
}