using UnityEngine;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace RetardedNetworking
{
  public class Client
  {
    private Thread _clientThread;
    private bool _stopping = false;
    public byte MyId { get; set; }

    private Queue<NetworkMessage> _messagesToSend = new Queue<NetworkMessage>();

    public Client(string serverIp, int serverPort)
    {
      _clientThread = new Thread(() =>
      {
        _stopping = false;

        Debug.Log($"[Client Thread] Connecting to {serverIp}:{serverPort}");

        TcpClient tcpClient = new TcpClient();

        if (!tcpClient.ConnectAsync(serverIp, serverPort).Wait(1000))
        {
          Debug.Log("[Client Thread] Could not connect");
          tcpClient.Close();
          return;
        }

        NetworkStream stream = tcpClient.GetStream();

        while (!_stopping)
        {
          if (_messagesToSend.Count > 0 && stream.CanWrite)
          {
            byte[] bytes = _messagesToSend.Dequeue().GetBytes();
            stream.Write(bytes, 0, bytes.Length);
          }

          if (stream.CanRead && stream.DataAvailable)
          {
            NetworkMessage message = NetworkMessage.ReadFrom(stream);
            NetworkManager.Singleton.clientReceivedMessages.Enqueue(message);
          }

          Debug.Log("[Client Thread] I'm alive !");
          Thread.Sleep(1000);
        }

        tcpClient.Close();

        _stopping = false;
      })
      {
        IsBackground = true,
      };

      _clientThread.Start();
    }

    public void Stop()
    {
      _stopping = true;
      _clientThread.Join();
      _clientThread = null;
      _stopping = false;
    }

    public void SendMessageToServer(NetworkMessageType type, byte[] data)
    {
      _messagesToSend.Enqueue(new NetworkMessage(type, MyId, data));
    }


  }
}