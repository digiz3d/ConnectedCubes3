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
        private List<ServerClient> _clientsList = new List<ServerClient>();
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
                            _clientsList.Add(client);
                            SendPacketToClient(PacketType.GIVE_CLIENT_ID, client, new byte[1] { newClientId });
                        }
                        catch (Exception e)
                        {
                            Debug.Log(e.Message);
                            Debug.Log(e.Source);
                            Debug.Log(e.StackTrace);
                        }
                    }

                    _clientsList.RemoveAll(client => client == null);

                    foreach (ServerClient client in _clientsList)
                    {
                        NetworkStream stream = client.NetworkStream;

                        while (stream.CanWrite && client.packetsToSend.Count > 0)
                        {
                            client.packetsToSend.Dequeue().SendToStream(stream);
                        }

                        while (stream.CanRead && stream.DataAvailable)
                        {
                            Packet packet = Packet.ReadFrom(stream);
                            if (packet.SenderClientId != client.Id)
                            {
                                Debug.Log("A client sent a packet as someone else.");
                                stream.Close();
                                stream.Dispose();
                                client.Tcp.Close();
                                client.Tcp.Dispose();
                            }
                            else
                            {
                                NetworkManager.Singleton.ServerReceivePacket(packet);
                            }
                        }
                    }

                    Thread.Sleep(7);
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
            _clientsList.Clear();
        }



        public void SendPacketToClient(PacketType type, byte clientId, byte[] data)
        {
            ServerClient client = _clientsList.Find(c => c.Id == clientId);
            SendPacketToClient(type, client, data);
        }
        public void SendPacketToClient(PacketType type, ServerClient client, byte[] data)
        {
            client.packetsToSend.Enqueue(new Packet(type, client.Id, data));
        }

        public void SendPacketToAllClients(PacketType type, byte[] data)
        {
            foreach (ServerClient client in _clientsList)
            {
                client.packetsToSend.Enqueue(new Packet(type, client.Id, new byte[0]));
            }
        }

    }
}