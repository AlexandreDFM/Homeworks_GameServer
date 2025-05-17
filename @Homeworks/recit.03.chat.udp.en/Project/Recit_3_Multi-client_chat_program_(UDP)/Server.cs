using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

class UdpChatServer
{
    private static int _serverPort = 8080;
    private static UdpClient _udpServer;
    private static readonly Dictionary<IPEndPoint, string> ConnectedClients = new();

    private static async void AddNickname(IPEndPoint clientEndPoint, string firstMessage)
    {
        if (string.IsNullOrEmpty(firstMessage)) firstMessage = "Anonymous";
        ConnectedClients.Add(clientEndPoint, firstMessage);
        Console.WriteLine($"Client connected: {clientEndPoint.Address}:{clientEndPoint.Port}, nickname set to: {firstMessage}");
        // Broadcast the nickname to all connected clients
        string nicknameMessage = $"{firstMessage} has joined the chat";
        byte[] nicknameBytes = Encoding.UTF8.GetBytes(nicknameMessage);
        foreach (var client in ConnectedClients)
        {
            await _udpServer.SendAsync(nicknameBytes, nicknameBytes.Length, client.Key);
        }
    }
    
    private static async void ExitChat(IPEndPoint clientEndPoint)
    {
        string nickname = ConnectedClients[clientEndPoint];
        ConnectedClients.Remove(clientEndPoint);
        Console.WriteLine($"Client disconnected: {clientEndPoint.Address}:{clientEndPoint.Port}, nickname: {nickname}");
        // Broadcast the exit message to all connected clients
        string exitMessage = $"{nickname} has left the chat";
        byte[] exitBytes = Encoding.UTF8.GetBytes(exitMessage);
        foreach (var client in ConnectedClients)
        {
            await _udpServer.SendAsync(exitBytes, exitBytes.Length, client.Key);
        }
    }
    
    public static async Task Main(string[] args)
    {
        _udpServer = new UdpClient(_serverPort);
        Console.WriteLine($"Server is listening on port {_serverPort}...");

        while (true) {
            try {
                // Receiving messages from clients
                UdpReceiveResult receivedResult = await _udpServer.ReceiveAsync();
                IPEndPoint clientEndPoint = receivedResult.RemoteEndPoint;
                string receivedMessage = Encoding.UTF8.GetString(receivedResult.Buffer);

                // Add new client to the list if not already connected, set nickname to Anonymous by default
                if (!ConnectedClients.TryGetValue(clientEndPoint, out string connectedClient))
                {
                    AddNickname(clientEndPoint, receivedMessage);
                    continue;
                }
                
                // Handle client exit
                if (receivedMessage == "/exit")
                {
                    ExitChat(clientEndPoint);
                    continue;
                }

                // Output messages received from clients
                Console.WriteLine($"Client {clientEndPoint.Address}:{clientEndPoint.Port}:{connectedClient} says: {receivedMessage}");

                // Broadcast the message to all connected clients
                string responseMessage = "Server received from " + ConnectedClients[clientEndPoint] + ": " + receivedMessage;
                byte[] responseBytes = Encoding.UTF8.GetBytes(responseMessage);
                
                foreach (var client in ConnectedClients)
                {
                    await _udpServer.SendAsync(responseBytes, responseBytes.Length, client.Key);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error receiving message: " + ex.Message);
            }
        }
    }
}
