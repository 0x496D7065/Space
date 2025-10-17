using System;
using UnityEngine;
using System.Collections.Generic;
using Steamworks;
using System.Collections;

public class RemotePlayerManager : MonoBehaviour
{
    public static RemotePlayerManager Instance;
    public GameObject remotePlayerPrefab;

    //public Dictionary<CSteamID, RemoteFPSController> remotePlayers = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple TargetableManager detected!");
            Destroy(this);
            return;
        }
        Instance = this;
    }

    //public void InstantiateRemotePlayer(CSteamID cSteamID, ushort TargetableID)
    //{
    //    GameObject go = Instantiate(remotePlayerPrefab);
    //    var remote = go.GetComponent<RemoteFPSController>();
    //    remotePlayers[cSteamID] = remote;
    //    remote.TargetableID = TargetableID;
    //}

    //public void HandlePlayerPacket(ParsedPacket packet)
    //{
    //    if (!remotePlayers.TryGetValue((CSteamID)(packet.originalSender), out var remote))
    //    {
    //        if (packet.originalSender == SteamP2PManager.Instance.Own_ID && packet.type == PacketType.PlayerRejoin)//this is used if the rejoining player is the local player
    //        {
    //            PlayerRejoinEvent rejoinEvent = PlayerRejoinEventSerializer.Deserialize(packet.payload);
    //            //ActivateRejoinRemote(packet.originalSender, rejoinEvent);
    //            return;
    //        }
    //        else
    //            return;
    //    }

    //    switch (packet.type)
    //    {
    //        case PacketType.RemoteFire:
    //            RemoteFireEvent fireEvent = FireEventSerializer.Deserialize(packet.payload);
    //            remote.ApplyFireEvent(fireEvent);
    //            if (SteamP2PManager.Instance.isHost)
    //            {
    //                //EnemyAIManager.Instance.ApplyDamage(fireEvent);
    //            }
    //            break;
    //        case PacketType.PlayerState:
    //            PlayerState state = PlayerStateSerializer.Deserialize(packet.payload);
    //            remote.ApplyNetworkState(state);
    //            break;
    //        case PacketType.DeathEvent:
    //            remote.IsDead = true;
    //            remote.OnDeath();
    //            break;
    //        case PacketType.PlayerRejoin:
    //            //deserialize packet payload
    //            PlayerRejoinEvent rejoinEvent = PlayerRejoinEventSerializer.Deserialize(packet.payload);
    //            //ActivateRejoinRemote(packet.originalSender, rejoinEvent);
    //            break;
    //        case PacketType.PlayerLeft:
    //            //DisableRemote(packet.originalSender, remote);
    //            break;
    //    }
    //}

    //public void DisableRemote(ulong cSteamID, RemoteFPSController remote)
    //{
    //    if (SteamP2PManager.Instance.isHost)
    //    {
    //        //serialize and tell all client who to disable
    //        IntPtr data = PlayerLeftEventSerializer.Serialize(cSteamID, out int length);
    //        SteamP2PManager.Instance.SendToAllUnManagedUnsafe(data, (uint)length, reliable: true);
    //    }
    //    //var remoteCharacterController = remote.GetComponent<CharacterController>();
    //    //var remoteMesh = remote.GetComponentInChildren<SkinnedMeshRenderer>();
    //    //var remoteWeapon = remote.GetComponentInChildren<FPSItem>();
    //    //var remoteWeaponMesh = remoteWeapon.GetComponentInChildren<SkinnedMeshRenderer>();
    //    //remoteCharacterController.enabled = false;
    //    //remoteMesh.enabled = false;
    //    //remoteWeaponMesh.enabled = false;
    //    remotePlayers.Remove(new CSteamID(cSteamID));
    //    TargetableRegistry.Instance.Unregister(remote.TargetableID);
    //    Destroy(remote.gameObject);
    //}

    //public void ActivateRejoinRemote(ulong cSteamID, PlayerRejoinEvent rejoinEvent)
    //{
    //    if (SteamP2PManager.Instance.isHost)
    //    {
    //        //serialize and tell all clients who to re-activate
    //        IntPtr data = PlayerRejoinEventSerializer.Serialize(rejoinEvent, cSteamID, out int length);
    //        SteamP2PManager.Instance.SendToAllUnManagedUnsafe(data, (uint)length, reliable: true);
    //    }
    //    else if (SteamP2PManager.Instance.Own_ID == cSteamID)//client is the one rejoining, setting up health and position
    //    {
    //        Debug.LogError("Applying health and position to local player");
    //        var localCharacterController = FPSController.Instance.GetComponent<CharacterController>();
    //        localCharacterController.enabled = false;
    //        FPSController.Instance.health = rejoinEvent.Health;
    //        FPSController.Instance.healthUI.UpdateHealthDisplay(rejoinEvent.Health, "Health");
    //        FPSController.Instance.gameObject.transform.position = rejoinEvent.position;
    //        FPSController.Instance.gameObject.transform.forward = rejoinEvent.forward;
    //        Debug.LogError($"Aimed postion: {rejoinEvent.position}, {rejoinEvent.forward}, {rejoinEvent.Health}");
    //        localCharacterController.enabled = true;
    //        return;
    //    }
    //    // Host and local client (that is not the one rejoining) re-activate the remote
    //    if (!remotePlayers.TryGetValue((CSteamID)cSteamID, out var remote) || remote == null) return; //In case the local player joined after the rejoining player quit and then rejoin,
    //    var remoteCharacterController = remote.GetComponent<CharacterController>();                                             //the remote is not present for the local player, therefore does not need to be activated
    //    var remoteMesh = remote.GetComponentInChildren<SkinnedMeshRenderer>();                                                  //And will be instantiated by the full sync
    //    var remoteWeapon = remote.GetComponentInChildren<FPSItem>();
    //    var remoteWeaponMesh = remoteWeapon.GetComponentInChildren<SkinnedMeshRenderer>();
    //    remoteCharacterController.enabled = true;
    //    remoteMesh.enabled = true;
    //    remoteWeaponMesh.enabled = true;
    //}
    private void OnDestroy()
    {
        PlayerLeftEventSerializer.Cleanup();
        PlayerRejoinEventSerializer.Cleanup();
    }
}
