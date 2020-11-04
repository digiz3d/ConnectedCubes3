using System;
using System.Collections.Generic;

namespace RetardedNetworking
{
    public static class ObjectIdsManager
    {
        public const int MAX_OBJECTS = int.MaxValue;

        private static readonly List<int> _ids = new List<int>();

        public static int GetAvailableId()
        {
            if (_ids.Count >= MAX_OBJECTS)
                throw new Exception("[ObjectIdsManager] Maximum clients number reached.");

            for (int i = 1; i < MAX_OBJECTS; i++)
            {
                if (!_ids.Contains(i))
                {
                    _ids.Add(i);
                    return i;
                }
            }

            throw new Exception("[ObjectIdsManager] No free Id available. Too many objects.");
        }

        public static void FreeId(byte id)
        {
            _ids.Remove(id);
        }
    }
}