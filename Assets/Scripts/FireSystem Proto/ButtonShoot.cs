using Mirror;
using UnityEngine;

public class ButtonShoot : Interactable
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
        radarShoot.FireWeapon(radarShoot.azimuth);
    }
}
