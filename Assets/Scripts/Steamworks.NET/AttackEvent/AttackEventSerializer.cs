using System.Collections.Generic;
using UnityEngine;

public struct AttackEvent
{
    public ushort AttackerID;

    public readonly void Serialize(List<byte> data)
    {
        NetWriter.WriteUShort(data, AttackerID);
    }

    public static AttackEvent Deserialize(NetReader reader)
    {
        return new AttackEvent
        {
            AttackerID = reader.ReadUShort(),
        };
    }
}
