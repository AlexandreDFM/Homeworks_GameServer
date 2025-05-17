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
            TcpClient client = new TcpClient("13.209.161.15", 57254);
            Console.WriteLine("Connected!");

            NetworkStream stream = client.GetStream();

            // Start a thread that receives messages from the server in real time.
            Thread receiveThread = new Thread(() => ReceiveMessages(client));
            receiveThread.Start();

            while (true)
            {
                Console.WriteLine("Enter Command (W/A/S/D/Q): ");
                string command = Console.ReadLine();

                if (string.IsNullOrEmpty(command))
                    continue;

                byte[] buffer = Encoding.UTF8.GetBytes(command);
                stream.Write(buffer, 0, buffer.Length);

                if (command.ToUpper() == "Q")
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


/*
using System;
using System.Net.Sockets;
using System.Text;

class TreasureHuntClient
{
    static void Main()
    {
        Console.WriteLine("Connecting...");
        TcpClient client = new TcpClient("127.0.0.1", 5000);
        NetworkStream stream = client.GetStream();
        Console.WriteLine("Connected!");

        byte[] buffer = new byte[1024];

        while (true)
        {
            Console.WriteLine("Enter Command (W, A, D, S): ");
            string command = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(command)) continue;

            // Sending commands to the server
            byte[] commandBytes = Encoding.UTF8.GetBytes(command);
            stream.Write(commandBytes, 0, commandBytes.Length);

            // Receive server response
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"서버 응답: {response}");
        }
    }
}
*/
