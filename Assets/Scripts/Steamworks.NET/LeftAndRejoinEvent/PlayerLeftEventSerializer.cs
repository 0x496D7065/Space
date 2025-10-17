using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public static class PlayerLeftEventSerializer
{
    private static readonly List<byte> data = new(9); //only need 8 byte for steamID and 1 byte for packet type

    //clean up for destroy
    private static unsafe byte* _lastAllocatedPtr = null;
    public static unsafe IntPtr Serialize(ulong originalSenderId, out int length)
    {
        data.Clear();

        NetWriter.WriteULong(data, originalSenderId);
        NetWriter.WriteByte(data, (byte)PacketType.PlayerLeft);

        length = data.Count;
        byte* ptr = (byte*)UnsafeUtility.Malloc(length, 8, Allocator.Temp);
        _lastAllocatedPtr = ptr;

        for (int i = 0; i < length; i++)
            ptr[i] = data[i];

        return (IntPtr)ptr;
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
