using Mirror;
using UnityEngine;

public abstract class Weapon_Base : ShipComponent
{
    [SyncVar] protected int currentAmmo;
    [SyncVar] public int maxAmmo;
    public abstract void Shoot(float azimuth, Transform radarCenter);

    [Server]
    public virtual void Reload()
    {
        if (currentAmmo < maxAmmo)
        {
            currentAmmo++;
        }
    }

    public bool CanShoot => currentAmmo > 0;
}
