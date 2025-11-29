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

        Vector2 direction = AzimuthToDirection(azimuth);
        GameObject missile = Instantiate(missilePrefab, radarCenter);
        NetworkServer.Spawn(missile);
        Missile missileScript = missile.GetComponent<Missile>();
        missileScript.Init(direction, azimuth, radarCenter);
        Debug.Log($"Missile Rotation: {missile.transform.rotation.eulerAngles}");
        Debug.Log($"Azimuth: {azimuth} Direction: {direction}");

        currentAmmo -= 1;
    }

    private Vector2 AzimuthToDirection(float azimuthDegrees)
    {
        float radians = -azimuthDegrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Sin(radians), Mathf.Cos(radians));
    }
}
