using System;
using System.Text;
using encryptionHelper;
using System.Net.Sockets;
using System.Security.Cryptography;

class HackerClient
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
        try
        {
            TcpClient client = new TcpClient("127.0.0.1", 5000);
            NetworkStream stream = client.GetStream();

            // Data manipulation (for example, manipulating the number of items)
            string manipulatedMessage = "Game data: items=5, location=(10,20)";
            string fakeHash = encryptionHelper.AesEncryptionHelper.Encrypt(manipulatedMessage);
            Console.WriteLine($"Generated hash: {fakeHash}");

            // Sending data with incorrect hash
            string dataToSend = $"{manipulatedMessage}|{fakeHash}";
            byte[] buffer = Encoding.UTF8.GetBytes(dataToSend);
            stream.Write(buffer, 0, buffer.Length);

            // Receive server response
            buffer = new byte[256];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Server response: {response}");

            client.Close();
        }
        catch (IOException ex)
        {
            Console.WriteLine($"IOException: {ex.Message}");
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"SocketException: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
    }
}