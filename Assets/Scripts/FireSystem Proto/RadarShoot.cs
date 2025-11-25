using Mirror;
using UnityEngine;

public class RadarShoot : MonoBehaviour
{
    [SerializeField] private Weapon_Base weapon;
    [SerializeField] private Transform radarCenter;

    [Range(0f, 360f)]
    [SerializeField] public float azimuth = 0f;

    [ContextMenu("Fire Missile")]
    public void FireFromInspector()
    {
        weapon.Shoot(azimuth, radarCenter);
    }

    public void FireWeapon(float azimuth)
    {
        weapon.Shoot(azimuth, radarCenter);
    }
}
