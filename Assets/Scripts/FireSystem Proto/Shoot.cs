using Mirror;
using UnityEngine;

public class Shoot : Interactable
{
    [Header("Reference")]
    [SerializeField] private RadarShoot radarShoot;

    public override void Interact()
    {
        CmdShoot();
    }

    [Command(requiresAuthority = false)]
    private void CmdShoot()
    {
        radarShoot.FireMissile(radarShoot.azimuth);
    }
}
