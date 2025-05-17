using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class RPGServer
//Responsible for the main functions of the RPG server
//   and manages client connections and message processing.
{
    private TcpListener server;
    private List<TcpClient> clients = new List<TcpClient>();
    private Dictionary<TcpClient, Player> players = new Dictionary<TcpClient, Player>();

    public void Start()
    //Start server and connect clients
    {
        server = new TcpListener(IPAddress.Any, 9000);
        server.Start();
        Console.WriteLine("========================");
        Console.WriteLine("=  B a t t l e  N e t  =");
        Console.WriteLine("=  Server Started ...  =");
        Console.WriteLine("========================");

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            clients.Add(client);

            // Start a thread to handle new client connections
            Thread clientThread = new Thread(HandleClient);
            clientThread.Start(client);
        }
    }

    private void HandleClient(object clientObj)
    //Responsible for communication with clients
    {
        TcpClient client = (TcpClient)clientObj;
        NetworkStream stream = client.GetStream();

        // Receiving player name from client
        byte[] buffer = new byte[256];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        string playerName = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

        Player player = new Player(playerName, 100, 10);
        players[client] = player;

        // Prints new client connection message and client list
        Console.WriteLine($"New Keyboard Warrior Access: {player.Name}");

        // Broadcast client connection message
        BroadcastMessage($"{player.Name} Keyboard Warrior Access!");

        while (true)
        {
            try
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"{player.Name}: {message}");
                ProcessMessage(client, message);
            }
            catch
            {
                Console.WriteLine($"{player.Name} Warrior Connection Terminate");
                clients.Remove(client);
                players.Remove(client);
                BroadcastMessage($"{player.Name} Warrior Exit");
                break;
            }
        }
    }

    private void ProcessMessage(TcpClient client, string message)
    //Analyze and execute commands sent by clients
    {
        Player player = players[client];

        if (message.StartsWith("/combat"))
        {
            var parts = message.Split(' ');
            if (parts.Length < 2)
            {
                SendMessage(client, "No combat target");
                return;
            }

            string targetName = parts[1];
            var target = FindPlayerByName(targetName);
            if (target != null)
            {
                StartBattle(player, target);
            }
            else
            {
                SendMessage(client, $"{targetName}: Absence of keyboard warrior!");
            }
        }
        else
        {
            BroadcastMessage($"{player.Name}: {message}");
        }
    }

    private void StartBattle(Player attacker, Player defender)
    //Handles combat between two players
    {
        int damage = attacker.AttackPower;
        defender.HP -= damage;
        BroadcastMessage($"{attacker.Name} damages {defender.Name} by {damage}. {defender.Name}\'s HP: {defender.HP}");

        if (defender.HP <= 0)
        {
            BroadcastMessage($"{defender.Name} Keyboard Warrior Death!");
            var defenderClient = GetClientByPlayer(defender);
            if (defenderClient != null)
            {
                RemovePlayerFromServer(defenderClient, defender);
            }
        }
    }

    private TcpClient GetClientByPlayer(Player player)
    //Returns a client (TcpClient) associated with a specific Player object.
    {
        foreach (var entry in players)
        {
            if (entry.Value == player)
            {
                return entry.Key;
            }
        }
        return null;
    }

    private void RemovePlayerFromServer(TcpClient client, Player player)
    //Removes a specific client from the clients and players list,
    //   and terminates the client connection.
    {
        if (clients.Contains(client))
        {
            clients.Remove(client);
            players.Remove(client);
            client.Close();
            BroadcastMessage($"{player.Name} Warrior Exit");
        }
    }

    private Player FindPlayerByName(string name)
    //Find Player objects based on name
    {
        foreach (var player in players.Values)
        {
            if (player.Name == name)
                return player;
        }
        return null;
    }

    private void BroadcastMessage(string message)
    // Send message to all clients
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        foreach (var client in clients)
        {
            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);
        }
    }

    private void SendMessage(TcpClient client, string message)
    //Send messages only to specific clients
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        NetworkStream stream = client.GetStream();
        stream.Write(data, 0, data.Length);
    }
}

class Player
//Save the player's basic information and status
{
    public string Name { get; set; }
    public int HP { get; set; }
    public int AttackPower { get; set; }

    public Player(string name, int hp, int attackPower)
    {
        Name = name;
        HP = hp;
        AttackPower = attackPower;
    }
}

class Program
{
    static void Main()
    {
        RPGServer server = new RPGServer();
        server.Start();
    }
}
