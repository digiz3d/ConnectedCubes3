using System.Collections.Generic;
using System.Net.Sockets;

namespace RetardedNetworking
{
  public class ServerClient
  {
    public byte Id { get; internal set; }
    public TcpClient Tcp { get; internal set; }
    public NetworkStream NetworkStream { get; internal set; }

    public Queue<NetworkMessage> MessagesToSend = new Queue<NetworkMessage>();

    public ServerClient(byte id, TcpClient tcp)
    {
      Id = id;
      Tcp = tcp;
      NetworkStream = tcp.GetStream();
    }
  }
}