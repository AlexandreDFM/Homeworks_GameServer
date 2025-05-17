//server (given)
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class UdpChatServer
{
    private static int serverPort = 8080;
    private static UdpClient udpServer;
    private static IPEndPoint connectedClient = null; // Single client

    public static async Task Main(string[] args)
    {
        udpServer = new UdpClient(serverPort);
        Console.WriteLine($"Server is listening on port {serverPort}...");

        while (true)
        {
            try
            {
                // Receiving messages from clients
                UdpReceiveResult receivedResult = await udpServer.ReceiveAsync();
                IPEndPoint clientEndPoint = receivedResult.RemoteEndPoint;
                string receivedMessage = Encoding.UTF8.GetString(receivedResult.Buffer);

                // Allow only first client
                if (connectedClient == null)
                {
                    connectedClient = clientEndPoint;
                    Console.WriteLine($"Client connected: {connectedClient.Address}:{connectedClient.Port}");
                }

                // Ignore if a new client attempts to connect
                if (!clientEndPoint.Equals(connectedClient))
                {
                    Console.WriteLine($"Rejected new connection attempt from {clientEndPoint.Address}:{clientEndPoint.Port}");
                    continue;
                }

                // Output messages received from clients
                Console.WriteLine($"Client {connectedClient.Address}:{connectedClient.Port} says: {receivedMessage}");

                // Send a response message to the client
                string responseMessage = "Server received: " + receivedMessage;
                byte[] responseBytes = Encoding.UTF8.GetBytes(responseMessage);
                await udpServer.SendAsync(responseBytes, responseBytes.Length, connectedClient);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error receiving message: " + ex.Message);
            }
        }
    }
}
