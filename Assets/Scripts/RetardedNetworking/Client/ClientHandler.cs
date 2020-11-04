using UnityEngine;

namespace RetardedNetworking
{
    public static class ClientHandler
    {
        public static void ServerGaveMyId(Packet pck, Server server, Client client)
        {
            client.Id = (byte)pck.Stream.ReadByte();
            Debug.Log($"[NetworkManager:client] The server send me my id = {client.Id}");
            client.SendPacketToServer(PacketType.THANKS, new byte[0]);
        }
    }
}