using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading;

class MUDServer
{
    private const int RoomSize = 20;
    private TcpListener _listener;
    private Dictionary<int, Player> _players = new Dictionary<int, Player>();
    private List<Item> _items = new List<Item>(); // Holds items in the room
    private Random _random = new Random();

    public MUDServer(int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        Console.WriteLine("Server started...");
        PlaceItems();
        StartAcceptingClients();
    }

    private void StartAcceptingClients()
    {
        _listener.BeginAcceptTcpClient(new AsyncCallback(AcceptClientCallback), null);
    }

    private void AcceptClientCallback(IAsyncResult ar)
    {
        if (_players.Count > 2) {
            Console.WriteLine("Max players reached. Cannot accept more connections.");
            return; // Only allow two clients
        }

        TcpClient tcpClient = _listener.EndAcceptTcpClient(ar);
        int clientId = tcpClient.Client.RemoteEndPoint.GetHashCode();
        Player player = new Player(clientId, tcpClient, GetRandomPosition());
        _players[clientId] = player;

        Console.WriteLine($"Player {clientId} connected at position {player.Position}. Awaiting username...");

        StartAcceptingClients();
        StartReceivingData(tcpClient, clientId);
    }

    // private void StartReceivingData(TcpClient tcpClient, int clientId)
    // {
    //     NetworkStream stream = tcpClient.GetStream();
    //     byte[] buffer = new byte[256];
    //     stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback((ar) => {
    //         int bytesRead = stream.EndRead(ar);
    //         if (bytesRead <= 0) return;
    //         string data = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
    //         HandleCommand(data, clientId);
    //         StartReceivingData(tcpClient, clientId);
    //     }), null);
    // }
    
    private void StartReceivingData(TcpClient tcpClient, int clientId)
    {
        NetworkStream stream = tcpClient.GetStream();
        byte[] buffer = new byte[256];
        stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback((ar) => {
            try
            {
                int bytesRead = stream.EndRead(ar);
                if (bytesRead <= 0)
                {
                    Console.WriteLine($"Player {clientId} disconnected.");
                    _players.Remove(clientId);
                    tcpClient.Close();
                    return;
                }
                string data = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                HandleCommand(data, clientId);
                StartReceivingData(tcpClient, clientId);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"IOException: {ex.Message}");
                _players.Remove(clientId);
                tcpClient.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }), null);
    }

    private void HandleCommand(string data, int clientId)
    {
        Player player = _players[clientId];
        string[] parts = data.Split(' ');
        string command = parts[0].ToLower();

        switch (command) {
            case "username":
                if (parts.Length > 1) player.Username = parts[1];
                Console.WriteLine($"Player {clientId} set username to {player.Username}");
                SendMessage(player, $"Welcome, {player.Username}!");
                if (_players.Count == 2) {
                    BroadcastMessage("Both players are present. The battle begins!");
                    BroadcastMessage("Use 'help' to see available commands.");
                } else {
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
            case "say":
                SayToPlayers(player, parts);
                break;
            case "scan":
                ScanPlayers(player);
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

    private void SayToPlayers(Player player, string[] parts)
    {
        if (parts.Length > 1) {
            string message = string.Join(" ", parts[1..]);
            // SendMessage to the other player
            Player? opponent = GetOpponent(player);
            if (opponent != null) {
                SendMessage(opponent, $"{player.Username} says: {message}");
            } else {
                SendMessage(player, "No opponent to talk to.");
            }
        } else {
            SendMessage(player, "Usage: say [message]");
        }
    }
    
    private void ScanPlayers(Player player)
    {
        int distance = 0;
        foreach (Player p in _players.Values.Where(p => p.ClientId != player.ClientId)) {
            distance = Math.Abs(player.Position.x - p.Position.x) + Math.Abs(player.Position.y - p.Position.y);
        }
        SendMessage(player, $"Opponent is {distance} steps away.");
    }

    private void PlaceItems()
    {
        _items.Clear();
        for (int i = 0; i < 5; i++) {
            _items.Add(new Item("Medicine", GetRandomPosition()));
            _items.Add(new Item("Fart Bag", GetRandomPosition()));
            _items.Add(new Item("Trap", GetRandomPosition()));
            _items.Add(new Item("Health Fountain", GetRandomPosition()));
        }
        
    }

    private void MovePlayer(Player player, string direction)
    {
        (int x, int y) = player.Position;
        switch (direction) {
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
        foreach (Item item in _items.Where(item => item.Name == "Trap" && item.Position == player.Position)) {
            player.Health -= 10;
            SendMessage(player, "You stepped on a Trap! HP decreased by 10.");
        }
    }

    private void PickupItem(Player player)
    {
        Item? item = _items.Find(i => i.Position == player.Position);
        if (item != null) {
            _items.Remove(item);
            player.Inventory.Add(item.Name); // Add the item to the player's inventory
            switch (item.Name) {
                case "Medicine":
                    player.Health = Math.Min(100, player.Health + 20);
                    SendMessage(player, "You picked up a Medicine! HP restored by 20.");
                    break;
                case "Health Fountain":
                    player.Health = Math.Min(100, player.Health + 15);
                    SendMessage(player, "You picked up a Health Fountain! HP restored by 15.");
                    break;
                case "Fart Bag":
                    player.HasFartBag = true;
                    SendMessage(player, "You picked up a Fart Bag! Use 'fart' to attack.");
                    break;
                case "Trap":
                    player.Health -= 10;
                    SendMessage(player, "You stepped on a Trap! HP decreased by 10.");
                    break;
            }
        } else {
            SendMessage(player, "No item here to pick up.");
        }
    }

    private void AttackOpponent(Player player)
    {
        if (!player.HasFartBag) {
            SendMessage(player, "You don't have a Fart Bag. Find one first!");
            return;
        }

        Player? opponent = GetOpponent(player);
        if (opponent != null && GetDistance(player.Position, opponent.Position) <= 2) {
            opponent.Health -= 10;
            player.HasFartBag = false;
            SendMessage(player, "Fart attack successful! Opponent damaged.");
            SendMessage(opponent, "You were hit by a fart attack! HP decreased by 10.");
        } else {
            SendMessage(player, "Opponent not in range for fart attack.");
        }
    }

    private static void ShowStatus(Player player)
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
        for (int y = 0; y < RoomSize; y++) {
            for (int x = 0; x < RoomSize; x++) {
                roomMap[x, y] = '.'; // Empty space
            }
        }

        // Place players and items on the map
        foreach (Player p in _players.Values) {
            roomMap[p.Position.x, p.Position.y] = p.ClientId == player.ClientId ? '1' : '2';
        }

        // Place items on the map
        foreach (Item item in _items) {
            // Use the first character of the item name
            roomMap[item.Position.x, item.Position.y] = item.Name[0];
        }
        
        // Build the map display with consistent padding for each cell
        StringBuilder mapDisplay = new StringBuilder("Room Map:\n");
        for (int y = 0; y < RoomSize; y++) {
            for (int x = 0; x < RoomSize; x++) {
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
        if (opponent != null && GetDistance(player.Position, opponent.Position) <= 2) {
            SendMessage(player, "Your opponent is nearby!");
        }
    }

    private void CheckGameOver()
    {
        foreach (Player player in _players.Values.Where(player => player.Health <= 0)) {
            BroadcastMessage($"{player.Username} has been defeated. Game over.");
            ResetGame(player);
            return;
        }
    }

    private void ResetGame(Player defeatedPlayer)
    {
        // Disconnect the defeated player
        defeatedPlayer.TcpClient.Close();
        _players.Remove(defeatedPlayer.ClientId);
        _players.Values.First().ResetPlayer(GetRandomPosition());
        PlaceItems();
        BroadcastMessage("Game has been reset. Waiting for players...");
    }

    private static void SendHelp(Player player)
    {
        const string helpText = "Available commands:\n" +
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
        foreach (Player player in _players.Values) {
            SendMessage(player, message);
        }
    }

    private static void SendMessage(Player player, string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        player.TcpClient.GetStream().Write(data, 0, data.Length);
    }

    private Player? GetOpponent(Player player)
    {
        return _players.Values.FirstOrDefault(p => p.ClientId != player.ClientId);
    }

    private static int GetDistance((int x, int y) pos1, (int x, int y) pos2)
    {
        return Math.Abs(pos1.x - pos2.x) + Math.Abs(pos1.y - pos2.y);
    }

    private (int x, int y) GetRandomPosition()
    {
        return (_random.Next(RoomSize), _random.Next(RoomSize));
    }
}

// Player and Item classes
internal class Player(int clientId, TcpClient tcpClient, (int x, int y) position)
{
    public int ClientId { get; } = clientId;
    public string Username { get; set; }
    public (int x, int y) Position { get; set; } = position;
    public List<string> Inventory { get; private set; } = new(); // Initialize the inventory as an empty list
    public int Health { get; set; } = 100;
    public bool HasFartBag { get; set; } = false;
    public TcpClient TcpClient { get; } = tcpClient;

    public void ResetPlayer((int x, int y) position)
    {
        Position = position;
        Inventory.Clear();
        Health = 100;
    }
}

internal class Item(string name, (int x, int y) position)
{
    public string Name { get; } = name;
    public (int x, int y) Position { get; set; } = position;
}

class Program
{
    private static void Main(string[] args)
    {
        const int port = 12345; // Specify the port for the server
        MUDServer server = new MUDServer(port);
        Console.WriteLine($"Server is running on port {port}. Waiting for players to connect...");

        // Prevent the main thread from exiting
        while (true) {
            // Keep the server running
            Thread.Sleep(1000);
        }
    }
}