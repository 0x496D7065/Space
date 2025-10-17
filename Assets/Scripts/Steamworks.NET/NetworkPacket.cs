using System.Net.Sockets;
using System;
using UnityEngine;
using Steamworks;

public enum PacketType : byte
{
    PlayerState = 0,
    RemoteFire = 1,
    EnemyState = 2,
    TargetableRegistryFullSync = 3,
    TargetableRegistryFullSyncRequest = 4,
    AttackBatch = 5,
    DeathEvent = 6,
    TurretRequest = 7,
    TurretSpawn = 8,
    TurretState = 9,
    TurretFireEvent = 10,
    TargetableDestroyedEvent = 11,
    PlayerRejoin = 12,
    PlayerLeft = 13,
}

public struct ParsedPacket
{
    public ulong originalSender;
    public PacketType type;
    public byte[] payload;
}

public static class NetworkPacket
{
    public static ParsedPacket Extract(byte[] rawData)
    {
        ParsedPacket parsedPacket = new()
        {
            originalSender = BitConverter.ToUInt64(rawData, 0),
            type = (PacketType)rawData[8],
            payload = new byte[rawData.Length - 9],
        };
        Array.Copy(rawData, 9, parsedPacket.payload, 0, parsedPacket.payload.Length);
        return parsedPacket;
    }
}