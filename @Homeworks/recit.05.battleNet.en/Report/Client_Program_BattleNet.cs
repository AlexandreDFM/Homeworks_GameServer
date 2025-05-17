using System.Text;
using System.Net.Sockets;

//Connects to a server, exchanges messages between
//   the user and the server, and manages the state of the client.
class RPGClient {
    TcpClient client;
    NetworkStream stream;

    public void Connect(string host, int port)
    //Initiates a connection to the server,
    //   takes input from the user, and sends a message.
    {
        client = new TcpClient(host, port);
        stream = client.GetStream();

        Console.WriteLine("Connected to Battle Net...");
        Console.Write("Enter the name you want to use: ");
        string playerName = Console.ReadLine();

        // Send player name to server
        SendMessage(playerName);

        // Start a thread to receive messages from the server.
        Thread receiveThread = new Thread(ReceiveMessages);
        receiveThread.Start();

        while (true) {
            string message = Console.ReadLine();
            if (message.ToLower() == "/exit") {
                Console.WriteLine("Exit the game!");
                break;
            }
            SendMessage(message);
        }

        Disconnect();
    }

    //Continuously receives messages from the server
    //   and outputs them to the client console.
    private void ReceiveMessages()
    {
        try {
            while (true) {
                byte[] buffer = new byte[256];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                // If the server closes the connection
                if (bytesRead == 0) {
                    Console.WriteLine("The connection to the server was lost. You have been removed.");
                    break;
                }
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine(message);
            }
        } catch {
            Console.WriteLine("The connection to the server was lost. You have been removed.");
        } finally {
            Disconnect();
        }
    }

    //Send a string message to the server
    private void SendMessage(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        stream.Write(data, 0, data.Length);
    }

    //Closes the connection to the server and
    //   clears the client's connection status.
    private void Disconnect()
    {
        if (client == null) return;
        client.Close();
        client = null;
        Console.WriteLine("The connection has been terminated.");
    }
}

class Program {
    private static void Main()
    {
        RPGClient client = new RPGClient();
        client.Connect("127.0.0.1", 9000);
    }
}
