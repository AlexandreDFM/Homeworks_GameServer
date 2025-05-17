using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Server
{

    static void Main()
    {
        // ① Initialize server socket
        TcpListener server = new TcpListener(IPAddress.Any, 5000);
        server.Start();
        Console.WriteLine("Waiting for commander’s instructions...");

        while (true)
        {
            // ② Accept client connection
            TcpClient client = server.AcceptTcpClient();
            Console.WriteLine("Commander connected!");

            // ③ Receive data
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[256];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string receivedData = 
                Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Data received from commander: {receivedData}");
            
            receivedData = SimpleDecrypt(receivedData);
            
            Console.WriteLine($"Decrypted data: {receivedData}");

            // ④ Send response
            string response = 
                "I need to sleep at dawn. I will attack after lunch.";
            response = SimpleEncrypt(response);
            buffer = Encoding.UTF8.GetBytes(response);
            stream.Write(buffer, 0, buffer.Length);

            client.Close();
        }
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