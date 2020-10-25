using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace RetardedNetworking
{
  public class Server
  {
    private List<ServerClient> clientsList = new List<ServerClient>();

    private Thread _serverThread;
    private bool _stopping = false;

    public Server(int port)
    {
      _serverThread = new Thread(() =>
      {
        _stopping = false;
        Debug.Log("[Server Thread] Hi.");

        TcpListener listener = new TcpListener(IPAddress.Any, port);

        listener.Start();

        while (!_stopping)
        {
          while (listener.Pending())
          {
            Debug.Log("[Server Thread] Accepting new client.");
            TcpClient tcpListener = listener.AcceptTcpClient();
            try
            {
              byte newClientId = ClientIdsManager.GetAvailableId();
              ServerClient client = new ServerClient(newClientId, tcpListener);
              client.MessagesToSend.Enqueue(new NetworkMessage(NetworkMessageType.GIVE_CLIENT_ID, ClientIdsManager.SERVER_CLIENT_ID, new byte[1] { newClientId }));
              clientsList.Add(client);
            }
            catch (Exception e)
            {
              Debug.Log(e.Message);
              Debug.Log(e.Source);
              Debug.Log(e.StackTrace);
            }
          }

          clientsList.RemoveAll(client => client == null);

          foreach (ServerClient client in clientsList)
          {
            NetworkStream stream = client.NetworkStream;

            if (client.MessagesToSend.Count > 0 && stream.CanWrite)
            {
              byte[] bytes = client.MessagesToSend.Dequeue().GetBytes();
              stream.Write(bytes, 0, bytes.Length);
            }

            if (stream.CanRead && stream.DataAvailable)
            {
              NetworkMessage message = NetworkMessage.ReadFrom(stream);
              if (message.SenderClientId != client.Id)
              {
                Debug.Log("A client sent a message as someone else.");
                stream.Close();
                stream.Dispose();
                client.Tcp.Close();
                client.Tcp.Dispose();
              }
              else
              {
                NetworkManager.Singleton.serverReceivedMessages.Enqueue(message);
              }
            }
          }

          Debug.Log("[Server Thread] I'm alive !");
          Thread.Sleep(1000);
        }

        listener.Stop();
        _stopping = false;
      })
      {
        IsBackground = true,
      };

      _serverThread.Start();
    }

    public void Stop()
    {
      _stopping = true;
      _serverThread.Join();
      _serverThread = null;
      clientsList.Clear();
    }

    public void SendMessageToClient(byte clientId, NetworkMessage msg)
    {
      ServerClient serverClient = clientsList.Find(client => client.Id == clientId);
      serverClient.MessagesToSend.Enqueue(msg);
    }
  }
}