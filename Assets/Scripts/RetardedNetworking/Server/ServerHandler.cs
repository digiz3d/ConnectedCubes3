using UnityEngine;

namespace RetardedNetworking
{
    public static class ServerHandler
    {
        public static void ClientSaidThanks(Packet pck, Server server, Client client)
        {
            Debug.Log($"[NetworkManager:server] The client {pck.bytes[1]} said thanks.");
        }
    }
}
