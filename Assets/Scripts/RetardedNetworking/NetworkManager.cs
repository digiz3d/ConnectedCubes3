using UnityEngine;
using System.Collections.Generic;

namespace RetardedNetworking
{
  public class NetworkManager : MonoBehaviour
  {
    public static NetworkManager Singleton { get; internal set; }
    private delegate void MessageHandler(NetworkMessage msg);


    public bool IsClient { get; internal set; }
    private Client client;
    public Queue<NetworkMessage> clientReceivedMessages = new Queue<NetworkMessage>();
    private Dictionary<NetworkMessageType, MessageHandler> _clientMessagesHandlers = new Dictionary<NetworkMessageType, MessageHandler>();


    public bool IsServer { get; internal set; }
    private Server server;
    public Queue<NetworkMessage> serverReceivedMessages = new Queue<NetworkMessage>();
    private Dictionary<NetworkMessageType, MessageHandler> _serverMessagesHandlers = new Dictionary<NetworkMessageType, MessageHandler>();

    private void Update()
    {
      if (_clientMessagesHandlers.Count > 0)
        while (clientReceivedMessages.Count > 0)
        {
          NetworkMessage msg = clientReceivedMessages.Dequeue();
          if (_clientMessagesHandlers.ContainsKey((NetworkMessageType)msg.bytes[0]))
          {
            _clientMessagesHandlers[(NetworkMessageType)msg.bytes[0]](msg);
          }
          else
          {
            Debug.Log($"[NetworkManager:client] Couldn't handle msg {msg.bytes[0]}");
          }
        }

      if (_serverMessagesHandlers.Count > 0)
        while (serverReceivedMessages.Count > 0)
        {
          NetworkMessage msg = serverReceivedMessages.Dequeue();

          if (_serverMessagesHandlers.ContainsKey((NetworkMessageType)msg.bytes[0]))
          {
            _serverMessagesHandlers[(NetworkMessageType)msg.bytes[0]](msg);
          }
          else
          {
            Debug.Log($"[NetworkManager:server] Couldn't handle msg {msg.bytes[0]}");
          }
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

      InitializeMessageHandlers();
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

      InitializeMessageHandlers();
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

    private void InitializeMessageHandlers()
    {

      if (_clientMessagesHandlers.Count == 0)
      {
        _clientMessagesHandlers = new Dictionary<NetworkMessageType, MessageHandler>() {
          {
            NetworkMessageType.GIVE_CLIENT_ID,
            (msg) =>
            {
              client.MyId = (byte)msg.Stream.ReadByte();
              Debug.Log($"[NetworkManager:client] The server send me my id = {client.MyId}");
              client.SendMessageToServer(NetworkMessageType.THANKS, new byte[0]);
            }
          }
        };
      }

      if (_serverMessagesHandlers.Count == 0)
      {
        _serverMessagesHandlers = new Dictionary<NetworkMessageType, MessageHandler>(){
          {
            NetworkMessageType.THANKS, msg => {
              Debug.Log($"[NetworkManager:server] The client {msg.bytes[1]} said thanks.");
            }
          }
        };
      }
    }
  }
}