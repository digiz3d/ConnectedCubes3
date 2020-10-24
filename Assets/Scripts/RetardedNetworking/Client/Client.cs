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
    public int MyId { get; set; }

    private Queue<NetworkMessage> _messagesToSend = new Queue<NetworkMessage>();

    private delegate void MessageHandler(NetworkMessage msg);

    private Dictionary<NetworkMessageType, MessageHandler> _messagesHandlers;

    public Client(string serverIp, int serverPort)
    {
      InitializeMessageHandlers();

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
            Debug.Log("client can write");
            byte[] bytes = _messagesToSend.Dequeue().GetBytes();
            stream.Write(bytes, 0, bytes.Length);
          }

          if (stream.CanRead && stream.DataAvailable)
          {
            Debug.Log("client can read");
            NetworkMessage message = NetworkMessage.ReadFrom(stream);
            NetworkManager.Singleton.networkReceivedMessages.Enqueue(message);
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

    public void SendMessageToServer(NetworkMessage msg)
    {
      _messagesToSend.Enqueue(msg);
    }

    private void InitializeMessageHandlers()
    {
      _messagesHandlers = new Dictionary<NetworkMessageType, MessageHandler>() {
        {
          NetworkMessageType.GIVE_CLIENT_ID,
          (msg) =>
          {
            MyId=msg.Stream.ReadByte();
          }
        }
      };
    }
  }
}