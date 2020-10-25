using System;
using System.IO;
using System.Net.Sockets;

namespace RetardedNetworking
{
    public class Packet
    {
        // Network packet id, client id, packet's data portion length
        public const byte headerSize = sizeof(byte) + sizeof(byte) + sizeof(int);
        public PacketType Type { internal set; get; }
        public byte SenderClientId { internal set; get; }
        public MemoryStream Stream { internal set; get; }
        internal byte[] bytes;

        public static Packet ReadFrom(NetworkStream stream)
        {
            byte[] headerBuffer = new byte[headerSize];
            stream.Read(headerBuffer, 0, headerSize);

            PacketType type = (PacketType)headerBuffer[0];
            byte clientId = headerBuffer[1];

            int dataLength = BitConverter.ToInt32(headerBuffer, 2 * sizeof(byte));
            byte[] data = new byte[dataLength];

            stream.Read(data, 0, dataLength);

            return new Packet(type, clientId, data);
        }

        public Packet(PacketType type, byte senderId, byte[] data)
        {
            Type = type;
            SenderClientId = senderId;
            Stream = new MemoryStream(data);
            bytes = new byte[headerSize + data.Length];
            bytes[0] = (byte)type;
            bytes[1] = senderId;
            bytes[2] = (byte)data.Length;
            bytes[3] = (byte)(data.Length >> 8);
            bytes[4] = (byte)(data.Length >> 16);
            bytes[5] = (byte)(data.Length >> 24);
            data.CopyTo(bytes, headerSize);
        }

        public void SendToStream(NetworkStream stream)
        {
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}