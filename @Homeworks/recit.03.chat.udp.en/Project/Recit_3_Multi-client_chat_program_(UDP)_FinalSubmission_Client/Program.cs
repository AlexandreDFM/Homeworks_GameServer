using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class UdpChatClient
{
    private static UdpClient _udpClient = null!;
    private const string ServerIp = "127.0.0.1"; // Server IP
    private const int ServerPort = 8080; // Server port#
    private const int LocalPort = 0; // Auto-allocation
    private static string _nickname = null!; // User's nickname

    public static async Task Main(string[] args)
    {
        _udpClient = new UdpClient(LocalPort);
        Console.WriteLine("Enter your nickname:");
        _nickname = Console.ReadLine();

        if (string.IsNullOrEmpty(_nickname))
            _nickname = "Anonymous";

        Console.WriteLine($"UDP Chat Client started as {_nickname}... Type messages to send to the server...");

        // Send the nickname as the first message to the server
        await SendNicknameAsync();

        // Receiving messages from the server asynchronously
        Task receiveTask = ReceiveMessagesAsync();

        // Message sending loop
        await SendMessagesAsync();
    }

    private static async Task SendNicknameAsync()
    {
        IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(ServerIp), ServerPort);
        byte[] nicknameBytes = Encoding.UTF8.GetBytes(_nickname);
        await _udpClient.SendAsync(nicknameBytes, nicknameBytes.Length, serverEndPoint);
    }

    private static async Task SendMessagesAsync()
    {
        IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(ServerIp), ServerPort);

        while (true) {
            try {
                string message = Console.ReadLine();

                if (message == "/exit") {
                    await SendExitMessageAsync(serverEndPoint);
                    _udpClient.Close();
                    Console.WriteLine("You have left the chat.");
                    break; // Exit the loop and terminate the client
                }

                if (!string.IsNullOrEmpty(message)) {
                    string fullMessage = $"{_nickname}: {message}";
                    byte[] messageBytes = Encoding.UTF8.GetBytes(fullMessage);
                    await _udpClient.SendAsync(messageBytes, messageBytes.Length, serverEndPoint);
                    Console.WriteLine($"Sent: {fullMessage}");
                }
            } catch (Exception ex) {
                Console.WriteLine("Error sending message: " + ex.Message);
            }
        }
    }

    private static async Task SendExitMessageAsync(IPEndPoint serverEndPoint)
    {
        string exitMessage = $"{_nickname} has left the chat.";
        byte[] exitMessageBytes = Encoding.UTF8.GetBytes(exitMessage);
        await _udpClient.SendAsync(exitMessageBytes, exitMessageBytes.Length, serverEndPoint);
    }

    private static async Task ReceiveMessagesAsync()
    {
        IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, 0);

        while (true) {
            try {
                UdpReceiveResult receivedResult = await _udpClient.ReceiveAsync();
                string receivedMessage = Encoding.UTF8.GetString(receivedResult.Buffer);
                Console.WriteLine($"Server: {receivedMessage}");
            } catch (Exception ex) {
                Console.WriteLine("Error receiving message: " + ex.Message);
            }
        }
    }
}
