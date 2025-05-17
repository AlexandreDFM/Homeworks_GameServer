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
        TcpListener listener = new TcpListener(IPAddress.Any, 5000);
        listener.Start();
        Console.WriteLine("Server started!");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Client connected!");
            NetworkStream stream = client.GetStream();

            byte[] buffer = new byte[256];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            string[] parts = receivedData.Split('|');
            if (parts.Length == 2)
            {
                string message = parts[0];
                string receivedHash = parts[1];

                try
                {
                    string decryptedHash = encryptionHelper.AesEncryptionHelper.Decrypt(receivedHash);
                    Console.WriteLine($"Decrypted data: {decryptedHash} | Message: {message}");
                    if (decryptedHash == message)
                    {
                        Console.WriteLine("Data integrity accepted : Game data: " + message);
                        string response = "Data integrity verified.";
                        stream.Write(Encoding.UTF8.GetBytes(response), 0, response.Length);
                    } else {
                        Console.WriteLine("Data integrity check failed.");
                        string response = "Data integrity check failed.";
                        stream.Write(Encoding.UTF8.GetBytes(response), 0, response.Length);
                    }
                }
                catch (FormatException ex)
                {
                    Console.WriteLine($"FormatException: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Invalid data format received.");
            }

            client.Close();
        }
    }
}