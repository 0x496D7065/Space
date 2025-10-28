using Steamworks;
using UnityEngine;

public class PacketDispatch : MonoBehaviour
{
    private void Awake()
    {
        SteamP2PManager.Instance.OnDataReceived += HandleIncomingPacket;
    }
    public void HandleIncomingPacket(CSteamID senderId, byte[] rawData)
    {
        if ((ulong)senderId == SteamP2PManager.Instance.Own_ID) return;
        var parsedPacket = NetworkPacket.Extract(rawData);

        switch (parsedPacket.type)
        {
            case PacketType.PlayerState:
                RemotePlayerManager.Instance.HandlePlayerPacket(parsedPacket);
                break;
            case PacketType.RemoteFire:
            case PacketType.DeathEvent:
            case PacketType.PlayerRejoin://this can only be sent by the host
            case PacketType.PlayerLeft://this can only be sent by the host
                //RemotePlayerManager.Instance.HandlePlayerPacket(parsedPacket);
                break;
            case PacketType.EnemyState:
               // RemoteAIManager.Instance.HandleEnemyState(parsedPacket);
                break;
            case PacketType.TargetableRegistryFullSyncRequest:
                //TargetableRegistry.Instance.OnClientConnected(senderId);
                return;//returns instead of break because only the clients sends this to the host, the host must not relay this to other clients
            case PacketType.TargetableRegistryFullSync:
                //TargetableRegistry.Instance.HandleRegistryFullSync(parsedPacket);
                break;
            case PacketType.AttackBatch:
                //RemoteAIManager.Instance.HandleAttackBatches(parsedPacket);
                break;
        }

        if (SteamP2PManager.Instance.isHost)
        {
            SteamP2PManager.Instance.SendToAllExcept(senderId, rawData);
        }
    }

    private void OnDestroy()
    {
        if (SteamP2PManager.Instance != null)
            SteamP2PManager.Instance.OnDataReceived -= HandleIncomingPacket;
    }
}
