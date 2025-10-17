using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

public static class DeathEventSerializer
{
    public static byte[] Serialize(ushort targetableID, ulong originalSenderId)
    {
        List<byte> data = new();

        NetWriter.WriteULong(data, originalSenderId);
        NetWriter.WriteByte(data, (byte)PacketType.DeathEvent);

        NetWriter.WriteUShort(data, targetableID);
        
        return data.ToArray();
    }

    public static ushort Deserialize(byte[] data)
    {
        var reader = new NetReader(data);
        ushort targetableID = reader.ReadUShort();
        return targetableID;
    }
}
