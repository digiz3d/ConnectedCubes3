using UnityEngine;

namespace RetardedNetworking
{
    public static class ClientHandler
    {
        public static void GetMyClientId(Packet pck, Server server, Client client, NetworkManager manager)
        {
            client.Id = pck.ReadByte();
            Debug.Log($"[NetworkManager:client] The server send me my id = {client.Id}");
            client.SendPacketToServer(PacketType.THANKS, new byte[0]);
        }
    }
}