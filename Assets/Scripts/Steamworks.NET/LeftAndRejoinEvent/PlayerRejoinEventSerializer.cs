using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

public static class PlayerRejoinEventSerializer
{
    private static readonly int BytesOfRejoinEvent = UnsafeUtility.SizeOf<PlayerRejoinEvent>();
    private static readonly List<byte> data = new(BytesOfRejoinEvent + 9); //need 9 addition bytes for originalsenderId and packetType

    //clean up for destroy
    private static unsafe byte* _lastAllocatedPtr = null;
    public static unsafe IntPtr Serialize(PlayerRejoinEvent rejoinEvent, ulong originalSenderId, out int length)
    {
        data.Clear();

        NetWriter.WriteULong(data, originalSenderId);
        NetWriter.WriteByte(data, (byte)PacketType.PlayerRejoin);

        NetWriter.WriteFloat(data, rejoinEvent.position.x);
        NetWriter.WriteFloat(data, rejoinEvent.position.y);
        NetWriter.WriteFloat(data, rejoinEvent.position.z);

        NetWriter.WriteFloat(data, rejoinEvent.forward.x);
        NetWriter.WriteFloat(data, rejoinEvent.forward.y);
        NetWriter.WriteFloat(data, rejoinEvent.forward.z);

        NetWriter.WriteUShort(data, (ushort)rejoinEvent.Health);

        length = data.Count;
        byte* ptr = (byte*)UnsafeUtility.Malloc(length, 8, Allocator.Temp);
        _lastAllocatedPtr = ptr;

        for (int i = 0; i < length; i++)
            ptr[i] = data[i];

        return (IntPtr)ptr;
    }

    public static PlayerRejoinEvent Deserialize(byte[] data)
    {
        PlayerRejoinEvent rejoinEvent = new();
        var reader = new NetReader(data);

        rejoinEvent.position = new float3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
        rejoinEvent.forward = new float3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());

        rejoinEvent.Health = (short)reader.ReadUShort();

        return rejoinEvent;
    }

    public static unsafe void Cleanup()
    {
        if (_lastAllocatedPtr != null)
        {
            UnsafeUtility.Free(_lastAllocatedPtr, Allocator.Temp);
            _lastAllocatedPtr = null;
        }
    }
}
