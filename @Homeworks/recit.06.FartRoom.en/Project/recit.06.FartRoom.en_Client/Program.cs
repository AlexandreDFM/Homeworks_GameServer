using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class MUDClient
{
    private TcpClient client;
    private NetworkStream stream;
    private string username;
    private bool connected = false;

    public MUDClient(string serverAddress, int port)
    {
        client = new TcpClient(serverAddress, port);
        stream = client.GetStream();
        connected = true;
        Console.WriteLine("Connected to server.");

        StartReceivingData();
    }

    public void StartGame()
    {
        Console.Write("Enter your username: ");
        username = Console.ReadLine();
        SendMessage($"username {username}");
        Console.WriteLine("Type 'help' to see available commands.");

        while (connected)
        {
            string input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
            {
                SendMessage(input);
            }
        }
    }

    private void StartReceivingData()
    {
        Thread thread = new Thread(() =>
        {
            while (connected)
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine(message);
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Disconnected from server.");
                    connected = false;
                }
            }
        });
        thread.IsBackground = true;
        thread.Start();
    }

    private void SendMessage(string message)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }
        catch (Exception)
        {
            Console.WriteLine("Error sending message. Disconnected from server.");
            connected = false;
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Welcome to the Fart Room!");
        Console.WriteLine("Connecting to server...");
        MUDClient client = new MUDClient("127.0.0.1", 12345);
        client.StartGame();
    }
}
