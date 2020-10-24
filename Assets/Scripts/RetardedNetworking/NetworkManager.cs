using UnityEngine;
using System.Collections.Generic;

namespace RetardedNetworking
{
  public class NetworkManager : MonoBehaviour
  {
    public bool IsClient { get; internal set; }
    public bool IsServer { get; internal set; }

    public static NetworkManager Singleton { get; internal set; }

    private Server server;
    private Client client;

    public Queue<NetworkMessage> networkReceivedMessages = new Queue<NetworkMessage>();

    private void Update()
    {
      while (networkReceivedMessages.Count > 0)
      {
        // TODO use handler dictionary here instead of this
        // HandleNetworkMessages(networkReceivedMessages.Dequeue());
      }
    }

    private void OnEnable()
    {
      if (Singleton != null && Singleton != this)
      {
        Destroy(gameObject);
      }
      else
      {
        Singleton = this;
        DontDestroyOnLoad(gameObject);
        Application.runInBackground = true;
      }
    }

    private void OnDestroy()
    {
      if (Singleton != null && Singleton == this)
      {
        Singleton = null;
      }
    }

    public void StartServer()
    {
      if (IsClient || IsServer) return;

      IsServer = true;
      server = new Server(27015);

    }

    public void StopServer()
    {
      if (!IsServer) return;

      IsServer = false;
      server.Stop();
    }

    public void ConnectToServer()
    {
      if (IsClient) return;

      IsClient = true;
      client = new Client("127.0.0.1", 27015);
    }

    public void DisconnectFromServer()
    {
      if (!IsClient) return;
      IsClient = false;

      client.Stop();
    }

    private void SetClientId(NetworkMessage msg)
    {
      client.MyId = (byte)msg.Stream.ReadByte();

      Debug.Log($"My client id is now : {client.MyId}");
    }
  }
}