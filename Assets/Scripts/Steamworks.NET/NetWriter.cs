using System.Collections.Generic;
using System;
using UnityEngine;

public static class NetWriter
{
    public static void WriteFloat(List<byte> data, float value) => data.AddRange(BitConverter.GetBytes(value));

    public static void WriteByte(List<byte> data, byte value) => data.Add(value);

    public static void WriteBool(List<byte> data, bool value) => data.Add((byte)(value ? 1 : 0));

    public static void WriteULong(List<byte> data, ulong value) => data.AddRange(BitConverter.GetBytes(value));

    public static void WriteUShort(List<byte> data, ushort value) => data.AddRange(BitConverter.GetBytes(value));

    public static void WriteDouble(List<byte> data, double value) => data.AddRange(BitConverter.GetBytes(value));

    public static void WriteLong(List<byte> data, long value) => data.AddRange(BitConverter.GetBytes(value));
}