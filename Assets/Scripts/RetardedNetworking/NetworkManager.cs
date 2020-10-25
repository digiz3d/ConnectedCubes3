using UnityEngine;
using System.Collections.Generic;

namespace RetardedNetworking
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Singleton { get; internal set; }
        private delegate void PacketHandler(Packet msg);


        public bool IsClient { get; internal set; }
        private Client client;
        private Queue<Packet> _clientReceivedPackets = new Queue<Packet>();
        private Dictionary<PacketType, PacketHandler> _clientPacketHandlers = new Dictionary<PacketType, PacketHandler>();


        public bool IsServer { get; internal set; }
        private Server server;
        private Queue<Packet> _serverReceivedpackets = new Queue<Packet>();
        private Dictionary<PacketType, PacketHandler> _serverPacketHandlers = new Dictionary<PacketType, PacketHandler>();

        private void Update()
        {
            if (_clientPacketHandlers.Count > 0)
                while (_clientReceivedPackets.Count > 0)
                {
                    Packet msg = _clientReceivedPackets.Dequeue();
                    if (_clientPacketHandlers.ContainsKey(msg.Type))
                    {
                        _clientPacketHandlers[msg.Type](msg);
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
                        _serverPacketHandlers[msg.Type](msg);
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

            InitializePacketHandlers();
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

            InitializePacketHandlers();
            IsClient = true;
            client = new Client("127.0.0.1", 27015);
        }

        public void DisconnectFromServer()
        {
            if (!IsClient) return;
            IsClient = false;

            client.Stop();
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
                    {
                    PacketType.GIVE_CLIENT_ID,
                    (msg) =>
                    {
                    client.Id = (byte)msg.Stream.ReadByte();
                    Debug.Log($"[NetworkManager:client] The server send me my id = {client.Id}");
                    client.SendPacketToServer(PacketType.THANKS, new byte[0]);
                    }
                    }
                    };
            }

            if (_serverPacketHandlers.Count == 0)
            {
                _serverPacketHandlers = new Dictionary<PacketType, PacketHandler>(){
                    {
                    PacketType.THANKS, msg => {
                    Debug.Log($"[NetworkManager:server] The client {msg.bytes[1]} said thanks.");
                    }
                    }
                    };
            }
        }
    }
}