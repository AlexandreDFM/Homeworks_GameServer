using System;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Security.Cryptography;

class Server
{
    public static string ComputeHash(string data)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(bytes);
        }
    }

    static void Main()
    {
        TcpListener server = new TcpListener(IPAddress.Any, 5000);
        server.Start();
        Console.WriteLine("Server started!");

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            Console.WriteLine("Client connected!");

            NetworkStream stream = client.GetStream();

            // Deceiving data
            byte[] buffer = new byte[256];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string receivedData = 
                Encoding.UTF8.GetString(buffer, 0, bytesRead);

            // Separation of data and hash
            string[] parts = receivedData.Split('|');
            string data = parts[0];
            string receivedHash = parts[1];

            // Integrity check
            string computedHash = ComputeHash(data);
            Console.WriteLine($"Generated hash: {computedHash}");
            if (computedHash == receivedHash) {
                Console.WriteLine($"Data integrity verified: {data}");
                string response = " Data received normally";
                buffer = Encoding.UTF8.GetBytes(response);
                stream.Write(buffer, 0, buffer.Length);
            } else {
                Console.WriteLine("Data is corrupted or falsified!");
                string response = " Data corrupted!";
                buffer = Encoding.UTF8.GetBytes(response);
                stream.Write(buffer, 0, buffer.Length);
            }

            client.Close();
        }
    }
}