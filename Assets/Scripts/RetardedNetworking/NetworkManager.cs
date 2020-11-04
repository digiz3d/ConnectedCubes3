using UnityEngine;
using System.Collections.Generic;

namespace RetardedNetworking
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Singleton { get; internal set; }
        private delegate void PacketHandler(Packet pck, Server server, Client client);

        public bool IsClient { get; internal set; }
        private Client _client;
        private Queue<Packet> _clientReceivedPackets = new Queue<Packet>();
        private Dictionary<PacketType, PacketHandler> _clientPacketHandlers = new Dictionary<PacketType, PacketHandler>();


        public bool IsServer { get; internal set; }
        private Server _server;
        private Queue<Packet> _serverReceivedpackets = new Queue<Packet>();
        private Dictionary<PacketType, PacketHandler> _serverPacketHandlers = new Dictionary<PacketType, PacketHandler>();

        public bool IsHost { get; internal set; }

        private Dictionary<int, GameObject> _spawnedGameObject;

        private void Update()
        {
            if (_clientPacketHandlers.Count > 0)
                while (_clientReceivedPackets.Count > 0)
                {
                    Packet msg = _clientReceivedPackets.Dequeue();
                    if (_clientPacketHandlers.ContainsKey(msg.Type))
                    {
                        _clientPacketHandlers[msg.Type](msg, _server, _client);
                    }
                    else
                    {
                        Debug.Log($"[NetworkManager:client] Couldn't handle msg {msg.bytes[0]}");
                    }
                }

            if (_serverPacketHandlers.Count > 0)
                while (_serverReceivedpackets.Count > 0)
                {
                    Packet msg = _serverReceivedpackets.Dequeue();

                    if (_serverPacketHandlers.ContainsKey(msg.Type))
                    {
                        _serverPacketHandlers[msg.Type](msg, _server, _client);
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
            if (IsHost || IsClient || IsServer) return;

            InitializePacketHandlers();
            IsServer = true;
            _server = new Server(27015);

        }

        public void StopServer()
        {
            if (!IsServer) return;

            IsServer = false;
            _server.Stop();
            _server = null;
        }

        public void StartClient()
        {
            if (IsHost || IsClient || IsServer) return;

            InitializePacketHandlers();
            IsClient = true;
            _client = new Client("127.0.0.1", 27015);
        }

        public void StopClient()
        {
            if (!IsClient) return;
            IsClient = false;

            _client.Stop();
            _client = null;
        }

        public void StartHost()
        {
            if (IsHost || IsClient || IsServer) return;

            InitializePacketHandlers();
            IsHost = true;
            _server = new Server(27015);
            _client = new Client("127.0.0.1", 27015);
        }

        public void StopHost()
        {
            if (!IsHost) return;
            IsHost = false;

            _client.Stop();
            _client = null;
            _server.Stop();
            _server = null;
        }


        public void ClientReceivePacket(Packet msg)
        {
            _clientReceivedPackets.Enqueue(msg);
        }

        public void ServerReceivePacket(Packet msg)
        {
            _serverReceivedpackets.Enqueue(msg);
        }

        private void InitializePacketHandlers()
        {
            if (_clientPacketHandlers.Count == 0)
            {
                _clientPacketHandlers = new Dictionary<PacketType, PacketHandler>() {
                    { PacketType.GIVE_CLIENT_ID, ClientHandler.GetIdFromServer }
                 };
            }

            if (_serverPacketHandlers.Count == 0)
            {
                _serverPacketHandlers = new Dictionary<PacketType, PacketHandler>(){
                    { PacketType.THANKS, ServerHandler.ClientSaidThanks }
                };
            }
        }

        private void SpawnGameObject(GameObject prefab, Transform spawnPoint)
        {
            GameObject go = Instantiate(prefab, spawnPoint);
            int objectId = ObjectIdsManager.GetAvailableId();
            _spawnedGameObject.Add(objectId, go);

            if (IsHost || IsServer)
            {
                //go.GetComponent<NetworkedGameObject>();
                //_server.SendPacketToAllClients();
            }
        }
    }
}