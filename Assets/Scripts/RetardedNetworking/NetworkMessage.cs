using System;
using System.IO;
using System.Net.Sockets;

namespace RetardedNetworking
{
  public class NetworkMessage
  {
    public const byte headerSize = 2 * sizeof(byte) + sizeof(int);

    public byte NetworkMessageType { internal set; get; }
    public byte SenderClientId { internal set; get; }
    public MemoryStream Stream { internal set; get; }
    internal byte[] bytes;

    public static NetworkMessage ReadFrom(NetworkStream stream)
    {
      byte[] headerBuffer = new byte[headerSize];
      stream.Read(headerBuffer, 0, headerSize);

      byte networkMessageType = headerBuffer[0];
      byte clientId = headerBuffer[1];

      int dataLength = BitConverter.ToInt32(headerBuffer, 2 * sizeof(byte));
      byte[] data = new byte[dataLength];

      stream.Read(data, 0, dataLength);

      return new NetworkMessage(networkMessageType, clientId, data);
    }

    public NetworkMessage(NetworkMessageType networkMessageType, byte senderClientId, byte[] data) : this((byte)networkMessageType, senderClientId, data) { }
    public NetworkMessage(byte networkMessageType, byte senderClientId, byte[] data)
    {
      NetworkMessageType = networkMessageType;
      SenderClientId = senderClientId;
      Stream = new MemoryStream(data);
      bytes = new byte[headerSize + data.Length];
      bytes[0] = networkMessageType;
      bytes[1] = senderClientId;
      bytes[2] = (byte)data.Length;
      bytes[3] = (byte)(data.Length >> 8);
      bytes[4] = (byte)(data.Length >> 16);
      bytes[5] = (byte)(data.Length >> 24);
      data.CopyTo(bytes, headerSize);
    }

    public byte[] GetBytes()
    {
      return bytes;
    }
  }

  public enum NetworkMessageType : byte
  {
    GIVE_CLIENT_ID,
    RPC,
    Ping,
  }
}