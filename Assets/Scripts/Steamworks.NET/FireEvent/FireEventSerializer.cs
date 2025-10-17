using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

public static class FireEventSerializer
{
    public static byte[] Serialize(RemoteFireEvent fireEvent, ulong originalSenderId)
    {
        List<byte> data = new();

        NetWriter.WriteULong(data, originalSenderId);
        NetWriter.WriteByte(data, (byte)PacketType.RemoteFire);

        NetWriter.WriteFloat(data, fireEvent.hitPoint.x);
        NetWriter.WriteFloat(data, fireEvent.hitPoint.y);
        NetWriter.WriteFloat(data, fireEvent.hitPoint.z);

        NetWriter.WriteUShort(data, fireEvent.targetId);

        NetWriter.WriteByte(data, fireEvent.damage);

        return data.ToArray();
    }

    public static RemoteFireEvent Deserialize(byte[] data)
    {
        var reader = new NetReader(data);
        RemoteFireEvent fireEvent;
        fireEvent.hitPoint = new Vector3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
        fireEvent.targetId = reader.ReadUShort();
        fireEvent.damage = reader.ReadByte();
        return fireEvent;
    }
}
