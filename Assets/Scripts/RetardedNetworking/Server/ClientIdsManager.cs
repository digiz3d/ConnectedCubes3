using System;
using System.Collections.Generic;

namespace RetardedNetworking
{
  public static class ClientIdsManager
  {
    public const byte MAX_PLAYERS = 253;
    public const byte SERVER_CLIENT_ID = 0;

    private static readonly List<byte> _ids = new List<byte>();

    public static byte GetAvailableId()
    {
      if (_ids.Count >= MAX_PLAYERS)
        throw new Exception("[ClientIdsManager] Maximum clients number reached.");

      for (byte i = 1; i < MAX_PLAYERS; i++)
      {
        if (!_ids.Contains(i))
        {
          _ids.Add(i);
          return i;
        }
      }

      throw new Exception("[ClientIdsManager] No free Id available. Too many players.");
    }

    public static void FreeId(byte id)
    {
      _ids.Remove(id);
    }
  }
}