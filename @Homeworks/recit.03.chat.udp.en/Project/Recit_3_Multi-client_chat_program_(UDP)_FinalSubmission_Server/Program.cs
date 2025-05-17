using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

class UdpChatServer
{
    private const int ServerPort = 8080;
    private static UdpClient _udpServer = null!;
    private static readonly Dictionary<IPEndPoint, string> _connectedClients = new Dictionary<IPEndPoint, string>();

    public static async Task Main(string[] args)
    {
        _udpServer = new UdpClient(ServerPort);
        Console.WriteLine($"Server is listening on port {ServerPort}...");

        while (true) {
            try {
                // Receiving messages from clients
                UdpReceiveResult receivedResult = await _udpServer.ReceiveAsync();
                IPEndPoint clientEndPoint = receivedResult.RemoteEndPoint;
                string receivedMessage = Encoding.UTF8.GetString(receivedResult.Buffer);

                // Handle new client connection
                if (!_connectedClients.ContainsKey(clientEndPoint)) {
                    _connectedClients.Add(clientEndPoint, receivedMessage); // Store nickname
                    Console.WriteLine($"{receivedMessage} has joined the chat from {clientEndPoint.Address}:{clientEndPoint.Port}");
                    BroadcastMessage($"{receivedMessage} has joined the chat", clientEndPoint);
                    continue; // Skip message processing, as this is the initial nickname message
                }

                // Handle client exit
                if (receivedMessage.EndsWith("/exit")) {
                    string nickname = _connectedClients[clientEndPoint];
                    _connectedClients.Remove(clientEndPoint);
                    Console.WriteLine($"{nickname} has left the chat.");
                    BroadcastMessage($"{nickname} has left the chat", clientEndPoint);
                    continue;
                }

                // Output the received message
                string nicknameMessage = $"{_connectedClients[clientEndPoint]}: {receivedMessage}";
                Console.WriteLine(nicknameMessage);

                // Broadcast the message to all other clients
                BroadcastMessage(nicknameMessage, clientEndPoint);
            } catch (Exception ex) {
                Console.WriteLine("Error receiving message: " + ex.Message);
            }
        }
    }

    // Broadcast a message to all connected clients except the sender
    private static async void BroadcastMessage(string message, IPEndPoint sender)
    {
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        List<IPEndPoint> clientsToRemove = new List<IPEndPoint>();

        foreach (var client in _connectedClients.Keys) {
            if (!client.Equals(sender)) {
                try {
                    await _udpServer.SendAsync(messageBytes, messageBytes.Length, client);
                } catch (Exception) {
                    clientsToRemove.Add(client); // If sending fails, mark the client for removal
                }
            }
        }

        // Remove clients that failed to receive the message
        foreach (var client in clientsToRemove) {
            _connectedClients.Remove(client);
            Console.WriteLine($"Client {client.Address}:{client.Port} removed from the list.");
        }
    }
}
