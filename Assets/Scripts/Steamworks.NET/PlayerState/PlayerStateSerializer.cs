using System.Collections.Generic;
using System;
using Unity.Mathematics;
using System.Net.Sockets;
using UnityEngine;
using Steamworks;
using System.IO;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;

public static class PlayerStateSerializer
{
    private static readonly int BytesPerPlayerState = UnsafeUtility.SizeOf<PlayerState>();
    private static readonly List<byte> data = new(BytesPerPlayerState + 9);// 9 byte is the size of the header(ulong + 1byte)

    //clean up for destroy
    private static unsafe byte* _lastAllocatedPtr = null;
    public static unsafe IntPtr Serialize(PlayerState state, ulong originalSenderId, out int length)
    {
        data.Clear();

        NetWriter.WriteULong(data, originalSenderId);
        NetWriter.WriteByte(data, (byte)PacketType.PlayerState);

        NetWriter.WriteFloat(data, state.position.x);
        NetWriter.WriteFloat(data, state.position.y);
        NetWriter.WriteFloat(data, state.position.z);

        NetWriter.WriteFloat(data, state.forward.x);
        NetWriter.WriteFloat(data, state.forward.y);
        NetWriter.WriteFloat(data, state.forward.z);

        NetWriter.WriteFloat(data, state.pitch);
        NetWriter.WriteFloat(data, state.yaw);

        NetWriter.WriteFloat(data, state.moveInput.x);
        NetWriter.WriteFloat(data, state.moveInput.y);

        NetWriter.WriteUShort(data, (ushort)state.Health);

        NetWriter.WriteByte(data, (byte)state.movementState);
        NetWriter.WriteByte(data, (byte)state.poseState);
        //NetWriter.WriteByte(data, (byte)state.aimState);
        //NetWriter.WriteByte(data, (byte)state.actionState);

        NetWriter.WriteByte(data, state.activeWeaponIndex);

        length = data.Count;
        byte* ptr = (byte*)UnsafeUtility.Malloc(length, 8, Allocator.Temp);
        _lastAllocatedPtr = ptr;

        for (int i = 0; i < length; i++)
            ptr[i] = data[i];

        return (IntPtr)ptr;
    }

    public static PlayerState Deserialize(byte[] data)
    {
        PlayerState state = new();
        var reader = new NetReader(data);

        state.position = new float3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
        state.forward = new float3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());

        state.pitch = reader.ReadFloat();
        state.yaw = reader.ReadFloat();

        state.moveInput = new Vector2(reader.ReadFloat(), reader.ReadFloat());
        state.Health = (short)reader.ReadUShort();
        state.movementState = (FPSMovementState)reader.ReadByte();
        state.poseState = (FPSPoseState)reader.ReadByte();
        //state.aimState = (FPSAimState)reader.ReadByte();
        //state.actionState = (FPSActionState)reader.ReadByte();

        state.activeWeaponIndex = reader.ReadByte();

        return state;
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
