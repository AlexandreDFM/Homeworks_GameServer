//client (given)
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class UdpChatClient
{
    private static UdpClient udpClient;
    private static string serverIp = "127.0.0.1";  // Server IP
    private static int serverPort = 8080;          // Server port#
    private static int localPort = 0;           // Auto-allocation

    public static async Task Main(string[] args)
    {
        // Binding a client to a local port
        udpClient = new UdpClient(localPort);
        Console.WriteLine($"UDP Chat Client started... Type messages to send to the server...");

        // Receiving messages from the server asynchronously
        Task receiveTask = ReceiveMessagesAsync();

        // Message sending loop
        await SendMessagesAsync();
    }

    // Method to send a message to the server asynchronously
    private static async Task SendMessagesAsync()
    {
        IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);

        while (true)
        {
            try
            {
                string message = Console.ReadLine();
                if (!string.IsNullOrEmpty(message))
                {
                    byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                    await udpClient.SendAsync(messageBytes, messageBytes.Length, serverEndPoint);
                    Console.WriteLine($"Sent to server: {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending message: " + ex.Message);
            }
        }
    }

    // Method to receive messages asynchronously from the server
    private static async Task ReceiveMessagesAsync()
    {
        IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, 0);

        while (true)
        {
            try
            {
                UdpReceiveResult receivedResult = await udpClient.ReceiveAsync();
                string receivedMessage = Encoding.UTF8.GetString(receivedResult.Buffer);
                Console.WriteLine($"Server: {receivedMessage}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error receiving message: " + ex.Message);
            }
        }
    }
}
