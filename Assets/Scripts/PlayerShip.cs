using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerShip : NetworkBehaviour
{
    //[Header("Reference")]
    [Header("Settings")]
    [SerializeField]private int hullIntegrity;
    [SerializeField] public float evasion = 0.5f;// will later be linked to engine room status

    public static PlayerShip Instance { get; private set; }
    public readonly Dictionary<ShipRoomType, ShipRoom> roomMap = new();
    private bool isDead;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple PlayerShip instances!");
            Destroy(this);
            return;
        }

        Instance = this;
    }

    [Server]
    public void TakeDamage(ShipRoomType targetRoom, int amount)
    {
        if (isDead) return;

        if (roomMap.TryGetValue(targetRoom, out var room))
        {
            room.HandleHit(amount);
        }

        hullIntegrity -= amount;
        Debug.Log($"PlayerShip hit, hullIntegrity at {hullIntegrity}");
        if (hullIntegrity <= 0)
        {
            isDead = true;
            Debug.Log($"PlayerShip dead");
            //Player lose
        }
    }

    [Server]
    public void RegisterRoom(ShipRoom room)
    {
        if (!roomMap.ContainsKey(room.roomType))
        {
            roomMap.Add(room.roomType, room);
            Debug.Log($"Room {room.roomType} registered.");
        }
        else
        {
            Debug.LogWarning($"Room {room.roomType} already registered.");
        }
    }
}
