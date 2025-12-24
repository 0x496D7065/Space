using Mirror;
using UnityEngine;

public class MissileLauncher : Weapon_Base
{
    [Header("Reference")]
    [SerializeField] private GameObject missilePrefab;

    [SyncVar] private NetworkIdentity loadedMissile;

    protected override void Awake()
    {
        base.Awake();

        currentAmmo = 0;

        currentHP = maxHP;

        roomType = ShipRoomType.Weapons;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;

        if (loadedMissile != null) return; // Already has a missile

        if (other.TryGetComponent(out PickableObject pickable) && other.CompareTag("Missile_Ammo"))
        {
            NetworkIdentity netId = pickable.GetComponent<NetworkIdentity>();
            loadedMissile = netId;
            SetAmmo(1);

            Debug.Log("[MissileLauncher] Missile loaded via trigger.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isServer) return;

        if (loadedMissile == null) return;

        if (other.TryGetComponent(out PickableObject pickable) && other.CompareTag("Missile_Ammo"))
        {
            NetworkIdentity netId = pickable.GetComponent<NetworkIdentity>();

            if (netId == loadedMissile)
            {
                loadedMissile = null;
                SetAmmo(0);
                Debug.Log("[MissileLauncher] Missile unloaded via trigger.");
            }
        }
    }

    public override void Shoot(float azimuth ,Transform radarCenter)
    {
        if (!isServer || !CanShoot || !isOnline) return;



        GameObject missile = Instantiate(missilePrefab, radarCenter);
        NetworkServer.Spawn(missile);
        Missile missileScript = missile.GetComponent<Missile>();

        ShipRoomType[] possibleRooms = (ShipRoomType[])System.Enum.GetValues(typeof(ShipRoomType));
        ShipRoomType target = possibleRooms[1];

        missileScript.Init(azimuth, radarCenter, target, PlayerShip.Instance.playerShipCollider);

        //Debug.Log($"Missile Rotation: {missile.transform.rotation.eulerAngles}");

        if (loadedMissile != null)
        {
            NetworkServer.Destroy(loadedMissile.gameObject); // Destroy the physical 3D missile that was used as ammo
            loadedMissile = null;
        }

        SetAmmo(0);

        Debug.Log("[MissileLauncher] Missile fired and physical ammo consumed.");
    }
}
