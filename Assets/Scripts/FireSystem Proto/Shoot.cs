using UnityEngine;

public class Shoot : Interactable
{
    [Header("Reference")]
    [SerializeField] private RadarShoot radarShoot;
    public override void Interact()
    {
        radarShoot.FireMissile(radarShoot.azimuth);
    }
}
