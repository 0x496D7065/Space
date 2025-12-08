using System.Collections.Generic;
using Mirror;
using UnityEngine;

public enum ShipRoomType
{
    Engine,
    Bridge,
    Weapons,
    Medbay,
}

public class ShipRoom : NetworkBehaviour
{
    [Header("Reference")]
    [SerializeField] public ShipRoomType roomType;
    [SerializeField] public GameObject firePrefab;
    [SerializeField] public Collider roomBox;

    [Header("Settings")]
    [SerializeField] private float roomHealth;

    private readonly List<ShipComponent> componentsInRoom = new();

    private void Start()
    {
        if (!isServer) return;
        PlayerShip.Instance.RegisterRoom(this);
    }

    public void RegisterComponent(ShipComponent comp)
    {
        if (!componentsInRoom.Contains(comp))
        {
            componentsInRoom.Add(comp);
            Debug.Log($"{comp.name} registered to room {roomType}");
        }
    }

    public void HandleHit(int amount)
    {
        if (roomHealth > 0)
        {
            roomHealth -= amount;
            if (roomHealth < 0)
                roomHealth = 0;
        }
        if (Random.value > 0.2f)
        {
            Vector3 pos = new Vector3(Random.Range(roomBox.bounds.min.x, roomBox.bounds.max.x), roomBox.bounds.max.y, Random.Range(roomBox.bounds.min.z, roomBox.bounds.max.z)) ;
            CmdStartFire(roomBox.ClosestPointOnBounds(pos), -transform.up); // attention au Y peut etre ichala

        }
        Debug.Log($"{roomType} took damage, roomHealth remaining: {roomHealth}");

        foreach (var comp in componentsInRoom)
        {
            comp.TakeDamage(amount);
        }
    }

    public void CmdStartFire(Vector3 origin, Vector3 direction)//Temporary fire spawn
    {
        Debug.Log("Hit started a fire");


        if (Physics.Raycast(origin, direction, out RaycastHit hit, 20f))
        {
            Vector3 firePos = hit.point + hit.normal * 0.01f;
            Quaternion fireRot = Quaternion.LookRotation(hit.normal);
            Debug.DrawRay(hit.point, hit.normal, Color.green, 30);

            GameObject fireNode = Instantiate(firePrefab, firePos, fireRot);
            fireNode.GetComponent<FireNode>().Initialize(firePrefab);
            NetworkServer.Spawn(fireNode);
        }
    }
}
