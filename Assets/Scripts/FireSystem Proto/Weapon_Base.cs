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

    [Server]
    protected void SetAmmo(int amount)
    {
        currentAmmo = Mathf.Clamp(amount, 0, maxAmmo);
    }

    public bool CanShoot => currentAmmo > 0;
}
