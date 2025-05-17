using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class TreasureHuntClient
{
    static void Main()
    {
        try
        {
            Console.WriteLine("Connecting to server...");
            // TcpClient client = new TcpClient("88.99.66.11", 50141);
            TcpClient client = new TcpClient("127.0.0.1", 12345);
            Console.WriteLine("Connected!");

            NetworkStream stream = client.GetStream();

            // Start a thread that receives messages from the server in real time.
            Thread receiveThread = new Thread(() => ReceiveMessages(client));
            receiveThread.Start();

            while (true)
            {
                Console.WriteLine("Enter Command (W/A/S/D/Q): ");
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                string command = keyInfo.KeyChar.ToString().ToUpper();

                if (string.IsNullOrEmpty(command))
                    continue;

                byte[] buffer = Encoding.UTF8.GetBytes(command);
                stream.Write(buffer, 0, buffer.Length);

                if (command == "Q")
                {
                    Console.WriteLine("Terminate the Game!");
                    break;
                }
            }

            client.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static void ReceiveMessages(TcpClient client)
    {
        try
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break; // Disconnected

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"\n[Server message]: {message}");
            }
        }
        catch (Exception ex)
        {
            if (client.Connected)
            {
                Console.WriteLine($"An error occurred while receiving a message from the server!: {ex.Message}");
            }
        }
    }
}