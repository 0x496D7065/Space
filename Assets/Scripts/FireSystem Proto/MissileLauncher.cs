using Mirror;
using UnityEngine;

public class MissileLauncher : Weapon_Base
{
    [Header("Reference")]
    [SerializeField] private GameObject missilePrefab;

    protected override void Awake()
    {
        base.Awake();

        currentAmmo = 0;

        currentHP = maxHP;

        roomType = ShipRoomType.Weapons;
    }

    public override void Shoot(float azimuth ,Transform radarCenter)
    {
        if (!isServer || !CanShoot || !isOnline) return;



        GameObject missile = Instantiate(missilePrefab, radarCenter);
        NetworkServer.Spawn(missile);
        Missile missileScript = missile.GetComponent<Missile>();
        missileScript.Init(azimuth, radarCenter);

        Debug.Log($"Missile Rotation: {missile.transform.rotation.eulerAngles}");
        
        currentAmmo -= 1;
    }
}
