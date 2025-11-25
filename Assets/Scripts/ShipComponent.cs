using UnityEngine;
using Mirror;
using System.Collections;

public abstract class ShipComponent: NetworkBehaviour
{
    [SyncVar] public int currentHP;

    [SyncVar] public bool isOnline = true;

    public int maxHP;
    public ShipRoomType roomType;

    protected virtual void Awake()
    {
        currentHP = maxHP;
        isOnline = true;
    }

    protected virtual void Start()
    {
        StartCoroutine(RegisterToRoom());
    }

    [Server]
    public virtual void TakeDamage(int amount)
    {
        if (!isOnline) return;

        currentHP -= amount;
        Debug.Log($"{name} took {amount} damage");
        if (currentHP <= 0)
        {
            currentHP = 0;
            isOnline = false;
            Debug.Log($"{name} is offline");
        }
    }

    [Server]
    public virtual void Repair(int amount)
    {
        if (currentHP <= 0)
        {
            currentHP = amount;
            isOnline = true;
            Debug.Log($"{name} is back Online");
        }
        else
        {
            currentHP += amount;
            if (currentHP > maxHP)
                currentHP = maxHP;
        }
    }

    private IEnumerator RegisterToRoom()
    {
        while (PlayerShip.Instance == null)
        {
            yield return null;
        }

        ShipRoom room;
        while (!PlayerShip.Instance.roomMap.TryGetValue(roomType, out room))
        {
            yield return null;
        }

        room.RegisterComponent(this);
        Debug.Log($"{name} registered to {roomType} room");
    }
}
