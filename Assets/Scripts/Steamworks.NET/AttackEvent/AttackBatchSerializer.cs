using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;

public class AttackEventBatch
{
    public List<AttackEvent> Events;
    private static readonly List<byte> data = new(11 + 1000 * 2); // 11 byte is header + Number of max event * size of event (ushort is 2 byte)

    //clean up for destroy
    private unsafe byte* _lastAllocatedPtr = null;

    public unsafe IntPtr Serialize(ulong originalSenderId, out int length)
    {
        data.Clear();

        NetWriter.WriteULong(data, originalSenderId);
        NetWriter.WriteByte(data, (byte)PacketType.AttackBatch);

        NetWriter.WriteUShort(data, (ushort)Events.Count);
        for (int i = 0; i < Events.Count; i++)
        {
            Events[i].Serialize(data);
        }

        length = data.Count;

        byte* ptr = (byte*)UnsafeUtility.Malloc(length, 8, Allocator.Temp);
        _lastAllocatedPtr = ptr;

        for (int i = 0; i < length; i++)
            ptr[i] = data[i];

        //IntPtr unmanagedPtr = Marshal.AllocHGlobal(length);
        //for (int i = 0; i < length; i++)
        //{
        //    Marshal.WriteByte(unmanagedPtr + i, data[i]);
        //}

        //return unmanagedPtr;
        return (IntPtr)ptr;
    }
    public static AttackEventBatch Deserialize(byte[] data)
    {
        var reader = new NetReader(data);
        ushort count = reader.ReadUShort();

        AttackEventBatch batch = new()
        {
            Events = new List<AttackEvent>(count),
        };
        for (int i = 0; i < count; i++)
        {
            batch.Events.Add(AttackEvent.Deserialize(reader));
        }

        return batch;
    }

    public unsafe void Cleanup()
    {
        if (_lastAllocatedPtr != null)
        {
            UnsafeUtility.Free(_lastAllocatedPtr, Allocator.Temp);
            _lastAllocatedPtr = null;
        }
    }
}