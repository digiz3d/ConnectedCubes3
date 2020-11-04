using System;
using UnityEngine;

namespace RetardedNetworking
{
    public static class ServerHandler
    {
        public static void ClientSaidThanks(Packet pck, Server server, Client client)
        {
            Debug.Log($"[NetworkManager:server] The client {pck.SenderClientId} said thanks.");
            //byte[] playerId = BitConverter.GetBytes();
            //server.SendPacketToAllClients(PacketType.SPAWN_PLAYER);
        }
    }
}
