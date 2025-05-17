using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class TreasureHuntServer
{
    private static TcpListener server;
    private static Dictionary<string, TcpClient> clients = new Dictionary<string, TcpClient>();
    private static Dictionary<string, (int X, int Y)> playerPositions = new Dictionary<string, (int, int)>();
    private static Dictionary<string, int> playerNumbers = new Dictionary<string, int>();
    //private static Dictionary<string, int> playerScores = new Dictionary<string, int>();
    private static (int X, int Y) treasure = (5, 5); // Treasure Location
    private static char[,] map = new char[10, 10]; // Map
    private static object lockObj = new object();
    private static int playerCount = 0; // Player number

    static void Main()
    {
        InitializeMap();
        server = new TcpListener(IPAddress.Any, 12345);
        server.Start();
        Console.WriteLine("Server started. Waiting for client connection...");
        DisplayMap(); // Initial map print when starting the server

        Thread acceptThread = new Thread(AcceptClients);
        acceptThread.Start();

        while (true)
        {
            Thread.Sleep(1000);
        }
    }

    private static void InitializeMap()
    {
        for (int y = 0; y < 10; y++)
            for (int x = 0; x < 10; x++)
                map[y, x] = '.'; // Initialize empty space

        map[treasure.Y, treasure.X] = 'T'; // Set initial treasure location
    }

    private static void AcceptClients()
    {
        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            Thread clientThread = new Thread(() => HandleClient(client));
            clientThread.Start();
        }
    }


    private static string ProcessCommand(string clientId, string command)
    {
        lock (lockObj)
        {
            var oldPosition = playerPositions[clientId];
            var newPosition = oldPosition;

            // Handling player movement
            switch (command)
            {
                case "W": newPosition.Y = Math.Max(0, oldPosition.Y - 1); break;
                case "S": newPosition.Y = Math.Min(9, oldPosition.Y + 1); break;
                case "A": newPosition.X = Math.Max(0, oldPosition.X - 1); break;
                case "D": newPosition.X = Math.Min(9, oldPosition.X + 1); break;
                default: return "Invalid command!";
            }

            // Distance calculation
            playerPositions[clientId] = newPosition;
            UpdateMap();

            return $"Current location: ({newPosition.X}, {newPosition.Y})";
        }
    }


    private static void UpdateMap()
    {
        // Reset map
        for (int y = 0; y < 10; y++)
            for (int x = 0; x < 10; x++)
                if (map[y, x] != 'T') map[y, x] = '.';

        // Player Position Update
        foreach (var player in playerPositions)
        {
            var pos = player.Value;
            int playerNum = playerNumbers[player.Key];
            map[pos.Y, pos.X] = playerNum.ToString()[0]; // Display player number
        }
    }

    private static void DisplayMap()
    {
        Console.Clear();
        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                Console.Write(map[y, x]);
            }
            Console.WriteLine();
        }
    }

    private static void HandleClient(TcpClient client)
    {
        string clientId = Guid.NewGuid().ToString();
        lock (lockObj)
        {
            playerCount++;
            clients[clientId] = client;
            playerPositions[clientId] = (0, 0); // Initial location
            playerNumbers[clientId] = playerCount; // Set player number
            
            UpdateMap();
        }

        Console.WriteLine($"Player {playerNumbers[clientId]} connected!");

        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];

        try
        {
            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).ToUpper().Trim();
                if (message == "Q")
                {
                    RemovePlayer(clientId, "Player quits the game.");
                    break;
                }

                string response;
                lock (lockObj)
                {
                    if (!playerPositions.ContainsKey(clientId))
                    {
                        Console.WriteLine($"Player {clientId} data has been deleted already.");
                        break;
                    }

                    response = ProcessCommand(clientId, message);
                }

                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                stream.Write(responseBytes, 0, responseBytes.Length);

                lock (lockObj)
                {
                    DisplayMap(); // Map output after player action
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Player {clientId} connection error: {ex.Message}");
        }
        finally
        {
            RemovePlayer(clientId, "Player has disconnected.");
        }
    }

    private static void RemovePlayer(string clientId, string reason)
    {
        lock (lockObj)
        {
            if (playerNumbers.ContainsKey(clientId))
            {
                Console.WriteLine($"Player {playerNumbers[clientId]} left: {reason}");

                // Close client connection
                if (clients.ContainsKey(clientId))
                {
                    clients[clientId].Close();
                    clients.Remove(clientId);
                }

                // Delete player data
                playerPositions.Remove(clientId);
                playerNumbers.Remove(clientId);
                UpdateMap();

                // Notification to all players
                string message = $"Player {playerNumbers[clientId]} has disconnected.";
                foreach (var client in clients.Values)
                {
                    NetworkStream stream = client.GetStream();
                    byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                    stream.Write(messageBytes, 0, messageBytes.Length);
                }
            }
            else
            {
                Console.WriteLine($"Player {clientId} data has been deleted already.");
            }
        }
    }


}