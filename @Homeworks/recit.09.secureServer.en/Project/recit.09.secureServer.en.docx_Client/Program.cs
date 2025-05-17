using System;
using System.Net.Sockets;
using System.Text;

class Client {
    static void Main()
    {
        // ① Server connection
        TcpClient client = new TcpClient("127.0.0.1", 5000);
        Console.WriteLine("Soldier connected!");

        // ② Transfer data
        NetworkStream stream = client.GetStream();
        string message = "Attack at dawn!";
        message = SimpleEncrypt(message);
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        stream.Write(buffer, 0, buffer.Length);
        Console.WriteLine("Completed sending message to soldier!");

        // ③ Receive response
        buffer = new byte[256];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Console.WriteLine($"Soldier response: {response}");
    
        response = SimpleDecrypt(response);
        
        Console.WriteLine($"Decrypted response: {response}");
        
        client.Close();
    }
    
    public static string SimpleEncrypt(string input)
    {
        char[] charArray = input.ToCharArray();
        for (int i = 0; i < charArray.Length; i++)
        {
            charArray[i] = (char)(charArray[i] + 10);
        }
        return new string(charArray);
    }

    public static string SimpleDecrypt(string input)
    {
        char[] charArray = input.ToCharArray();
        for (int i = 0; i < charArray.Length; i++)
        {
            charArray[i] = (char)(charArray[i] - 10);
        }
        return new string(charArray);
    }
}