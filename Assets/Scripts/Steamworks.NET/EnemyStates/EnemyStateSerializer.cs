using Unity.Mathematics;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using K4os.Compression.LZ4;
using Unity.Collections;
public static class EnemyStateSerializer
{
    //private static readonly List<byte> data = new(11 + 1000 * 40);// again 1000 used for max amount of enemies for now / 11 is size of the header, count is the number of states, 40 is the size in byte of each enemy state
    private static readonly int MaxEnemyCount = 1000;
    private static readonly int BytesPerEnemy = UnsafeUtility.SizeOf<EnemyUnitState>();
    private static readonly int MaxRawSize = MaxEnemyCount * BytesPerEnemy;
    private static readonly int MaxCompressedSize = MaxRawSize + 128;

    //clean up for destroy
    private static unsafe byte* _lastAllocatedPtr = null;

    //Preallocated buffers
    public  static readonly byte[] rawBuffer = new byte[MaxRawSize];
    private static readonly byte[] compressedBuffer = new byte[MaxCompressedSize];

    private static int writeIndex;

    public static unsafe IntPtr Serialize(EnemyUnitState[] states, int count, ulong originalSenderId, out int length)
    {
        writeIndex = 0;

        WriteUShort((ushort)count);

        for (int i = 0; i < count; i++)
        {
            WriteLong(states[i].timeStamp);

            WriteFloat(states[i].position.x);
            WriteFloat(states[i].position.y);
            WriteFloat(states[i].position.z);

            WriteFloat(states[i].forward.x);
            WriteFloat(states[i].forward.y);
            WriteFloat(states[i].forward.z);

            WriteFloat(states[i].moveSpeed);

            WriteUShort(states[i].ID);

            WriteBool(states[i].IsDead);
        }

        int uncompressedSize = writeIndex;

        int compressedSize = LZ4Codec.Encode(rawBuffer, 0, uncompressedSize, compressedBuffer, 0, compressedBuffer.Length);
        length = 9 + compressedSize;
        byte* ptr = (byte*)UnsafeUtility.Malloc(length, 8, Allocator.Temp);
        _lastAllocatedPtr = ptr;

        //Debug.Log($"UncompressedSize = {uncompressedSize} vs CompressedSize{compressedSize}");

        UnsafeUtility.CopyStructureToPtr(ref originalSenderId, ptr);
        ptr[8] = (byte)PacketType.EnemyState;

        for (int i = 0; i < compressedSize; i++)
            ptr[9 + i] = compressedBuffer[i];

        return (IntPtr)ptr;
    }

    public static EnemyUnitState[] Deserialize(byte[] data)
    {
        var reader = new NetReader(data);
        ushort count = reader.ReadUShort();
        EnemyUnitState[] states = new EnemyUnitState[ count ];
        for (int i = 0; i < states.Length; i++)
        {
            states[i].timeStamp = reader.ReadLong();
            states[i].position = new float3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
            states[i].forward = new float3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
            states[i].moveSpeed = reader.ReadFloat();
            states[i].ID = reader.ReadUShort();
            states[i].IsDead = reader.ReadBool();
        }
        //Debug.Log($"payload size {data.Length} byte");
        return states;
    }

    private static void WriteUShort(ushort value)
    {
        rawBuffer[writeIndex++] = (byte)(value & 0xFF);
        rawBuffer[writeIndex++] = (byte)((value >> 8) & 0xFF);
    }

    private static void WriteFloat(float value)
    {
        unsafe
        {
            byte* p = (byte*)&value;
            for (int i = 0; i < 4; i++)
                rawBuffer[writeIndex++] = p[i];
        }
    }

    private static void WriteBool(bool value)
    {
        rawBuffer[writeIndex++] = (byte)(value ? 1 : 0);
    }

    private static void WriteLong(long value)
    {
        for (int i = 0; i < 8; i++)
            rawBuffer[writeIndex++] = (byte)((value >> (i * 8)) & 0xFF);
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
