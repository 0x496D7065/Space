using System;
using UnityEngine;

public class NetReader
{
    private readonly byte[] _data;
    private int _offset;

    public NetReader(byte[] data)
    {
        _data = data;
        _offset = 0;
    }

    public float ReadFloat()
    {
        float value = BitConverter.ToSingle(_data, _offset);
        _offset += 4;
        return value;
    }

    public byte ReadByte()
    {
        return _data[_offset++];
    }

    public bool ReadBool()
    {
        return _data[_offset++] == 1;
    }

    public ulong ReadULong()
    {
        ulong value = BitConverter.ToUInt64(_data, _offset);
        _offset += 8;
        return value;
    }

    public ushort ReadUShort()
    {
        ushort value = BitConverter.ToUInt16(_data, _offset);
        _offset += 2;
        return value;
    }

    public double ReadDouble()
    {
        double value = BitConverter.ToDouble(_data, _offset);
        _offset += 8;
        return value;
    }

    public long ReadLong()
    {
        long value = BitConverter.ToInt64(_data, _offset);
        _offset += 8;
        return value;
    }
}