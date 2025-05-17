using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class MUDServer
{
    private const int RoomSize = 20;
    private TcpListener listener;
    private Dictionary<int, Player> players = new Dictionary<int, Player>();
    private List<Item> items = new List<Item>(); // Holds items in the room
    private Random random = new Random();

    public MUDServer(int port)
    {
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine("Server started...");
        PlaceItems();
        StartAcceptingClients();
    }

    private void StartAcceptingClients()
    {
        listener.BeginAcceptTcpClient(new AsyncCallback(AcceptClientCallback), null);
    }

    private void AcceptClientCallback(IAsyncResult ar)
    {
        if (players.Count > 2)
        {
            Console.WriteLine("Max players reached. Cannot accept more connections.");
            return; // Only allow two clients
        }

        TcpClient tcpClient = listener.EndAcceptTcpClient(ar);
        int clientId = tcpClient.Client.RemoteEndPoint.GetHashCode();
        Player player = new Player(clientId, tcpClient, GetRandomPosition());
        players[clientId] = player;

        Console.WriteLine($"Player {clientId} connected at position {player.Position}. Awaiting username...");

        StartAcceptingClients();
        StartReceivingData(tcpClient, clientId);
    }

    private void StartReceivingData(TcpClient tcpClient, int clientId)
    {
        NetworkStream stream = tcpClient.GetStream();
        byte[] buffer = new byte[256];
        stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback((ar) => {
            int bytesRead = stream.EndRead(ar);
            if (bytesRead > 0)
            {
                string data = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                HandleCommand(data, clientId);
                StartReceivingData(tcpClient, clientId);
            }
        }), null);
    }

    private void HandleCommand(string data, int clientId)
    {
        Player player = players[clientId];
        string[] parts = data.Split(' ');
        string command = parts[0].ToLower();

        switch (command)
        {
            case "username":
                if (parts.Length > 1) player.Username = parts[1];
                Console.WriteLine($"Player {clientId} set username to {player.Username}");
                SendMessage(player, $"Welcome, {player.Username}!");

                if (players.Count == 2)
                {
                    BroadcastMessage("Both players are present. The battle begins!");
                    BroadcastMessage("Use 'help' to see available commands.");
                }
                else
                {
                    BroadcastMessage("Waiting for another player to join...");
                }
                                
                break;

            case "move":
                if (parts.Length > 1)
                    MovePlayer(player, parts[1]);
                break;

            case "loc":
                SendMessage(player, $"Your location: {player.Position}");
                break;

            case "pickup":
                PickupItem(player);
                break;

            case "fart":
                AttackOpponent(player);
                break;

            case "status":
                ShowStatus(player);
                break;

            case "show":
                ShowRoomMap(player);
                break;

            case "help":
                SendHelp(player);
                break;

            default:
                SendMessage(player, "Unknown command. Type 'help' for available commands.");
                break;
        }

        CheckNearby(player);
        CheckGameOver();
    }


    private void PlaceItems()
    {
        items.Clear();
        for (int i = 0; i < 5; i++)
        {
            items.Add(new Item("Medicine", GetRandomPosition()));
            items.Add(new Item("Fart Bag", GetRandomPosition()));
        }
    }

    private void MovePlayer(Player player, string direction)
    {
        (int x, int y) = player.Position;
        switch (direction)
        {
            case "up": y = Math.Max(0, y - 1); break;
            case "down": y = Math.Min(RoomSize - 1, y + 1); break;
            case "left": x = Math.Max(0, x - 1); break;
            case "right": x = Math.Min(RoomSize - 1, x + 1); break;
            default:
                SendMessage(player, "Invalid direction. Use up, down, left, or right.");
                return;
        }
        player.Position = (x, y);
        Console.WriteLine($"{player.Username} moved to {player.Position}");
    }

    private void PickupItem(Player player)
    {
        Item item = items.Find(i => i.Position == player.Position);
        if (item != null)
        {
            items.Remove(item);
            player.Inventory.Add(item.Name); // Add the item to the player's inventory
            if (item.Name == "Medicine")
            {
                player.Health = Math.Min(100, player.Health + 20);
                SendMessage(player, "You picked up a Medicine! HP restored by 20.");
            }
            else if (item.Name == "Fart Bag")
            {
                player.HasFartBag = true;
                SendMessage(player, "You picked up a Fart Bag! Use 'fart' to attack.");
            }
        }
        else
        {
            SendMessage(player, "No item here to pick up.");
        }
    }

    private void AttackOpponent(Player player)
    {
        if (!player.HasFartBag)
        {
            SendMessage(player, "You don't have a Fart Bag. Find one first!");
            return;
        }

        Player opponent = GetOpponent(player);
        if (opponent != null && GetDistance(player.Position, opponent.Position) <= 2)
        {
            opponent.Health -= 10;
            player.HasFartBag = false;
            SendMessage(player, "Fart attack successful! Opponent damaged.");
            SendMessage(opponent, "You were hit by a fart attack! HP decreased by 10.");
        }
        else
        {
            SendMessage(player, "Opponent not in range for fart attack.");
        }
    }

    private void ShowStatus(Player player)
    {
        // Count the number of fart bags in the player's inventory
        int fartBagCount = player.Inventory.FindAll(item => item == "Fart Bag").Count;

        // Build the status message
        StringBuilder statusMessage = new StringBuilder();
        statusMessage.AppendLine("Your Status:");
        statusMessage.AppendLine($"- Username: {player.Username}");
        statusMessage.AppendLine($"- HP: {player.Health}");
        statusMessage.AppendLine($"- Fart Bags: {fartBagCount}");
        statusMessage.AppendLine($"- Position: {player.Position}");

        // Send the status message to the player
        SendMessage(player, statusMessage.ToString());
    }


    private void ShowRoomMap(Player player)
    {
        char[,] roomMap = new char[RoomSize, RoomSize];

        // Initialize map with empty spaces
        for (int y = 0; y < RoomSize; y++)
        {
            for (int x = 0; x < RoomSize; x++)
            {
                roomMap[x, y] = '.'; // Empty space
            }
        }

        // Place players and items on the map
        // Todo 2

        // Build the map display with consistent padding for each cell
        StringBuilder mapDisplay = new StringBuilder("Room Map:\n");
        for (int y = 0; y < RoomSize; y++)
        {
            for (int x = 0; x < RoomSize; x++)
            {
                mapDisplay.Append($"{roomMap[x, y],-2}"); // Use -2 to make each cell two characters wide
            }
            mapDisplay.AppendLine(); // New line after each row to keep rows consistent
        }

        // Send the entire map as a single message to the client
        SendMessage(player, mapDisplay.ToString());
    }

    private void CheckNearby(Player player)
    {
        Player opponent = GetOpponent(player);
        if (opponent != null && GetDistance(player.Position, opponent.Position) <= 2)
        {
            SendMessage(player, "Your opponent is nearby!");
        }
    }

    private void CheckGameOver()
    {
        foreach (var player in players.Values)
        {
            if (player.Health <= 0)
            {
                BroadcastMessage($"{player.Username} has been defeated. Game over.");
                ResetGame();
                return;
            }
        }
    }

    private void ResetGame()
    {
        players.Clear();
        PlaceItems();
        BroadcastMessage("Game has been reset. Waiting for players...");
    }

    private void SendHelp(Player player)
    {
        string helpText = "Available commands:\n" +
                          "- move [up/down/left/right]: Move in the room\n" +
                          "- loc: Show your current coordinates\n" +
                          "- pickup: Pick up an item at your location\n" +
                          "- fart: Attack opponent if within range\n" +
                          "- show: Display map\n" +
                          "- status: Show your current status (HP, Position, etc.) \n" +
                          "- help: Show this help message";
        SendMessage(player, helpText);
    }

    private void BroadcastMessage(string message)
    {
        foreach (var player in players.Values)
        {
            SendMessage(player, message);
        }
    }

    private void SendMessage(Player player, string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        player.TcpClient.GetStream().Write(data, 0, data.Length);
    }

    private Player GetOpponent(Player player)
    {
        foreach (var p in players.Values)
        {
            if (p.ClientId != player.ClientId) return p;
        }
        return null;
    }

    private int GetDistance((int x, int y) pos1, (int x, int y) pos2)
    {
        return Math.Abs(pos1.x - pos2.x) + Math.Abs(pos1.y - pos2.y);
    }

    private (int x, int y) GetRandomPosition()
    {
        return (random.Next(RoomSize), random.Next(RoomSize));
    }
}

// Player and Item classes

class Player
{
    public int ClientId { get; }
    public string Username { get; set; }
    public (int x, int y) Position { get; set; }
    public List<string> Inventory { get; private set; }
    public int Health { get; set; } = 100;
    public bool HasFartBag { get; set; } = false;
    public TcpClient TcpClient { get; }

    public Player(int clientId, TcpClient tcpClient, (int x, int y) position)
    {
        ClientId = clientId;
        TcpClient = tcpClient;
        Position = position;
        Inventory = new List<string>(); // Initialize the inventory as an empty list
    }
}

class Item
{
    public string Name { get; }
    public (int x, int y) Position { get; }

    public Item(string name, (int x, int y) position)
    {
        Name = name;
        Position = position;
    }
}

class Program
{
    static void Main(string[] args)
    {
        int port = 12345;  // Specify the port for the server
        MUDServer server = new MUDServer(port);
        Console.WriteLine($"Server is running on port {port}. Waiting for players to connect...");

        // Prevent the main thread from exiting
        while (true)
        {
            // Keep the server running
            Thread.Sleep(1000);
        }
    }
}