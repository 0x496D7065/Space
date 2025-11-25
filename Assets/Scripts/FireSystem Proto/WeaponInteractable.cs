using Mirror;
using UnityEngine;

public class WeaponInteractable: Interactable
{
    [Header("Reference")]
    public Weapon_Base weapon;

    public override void Interact()
    {
        if (weapon.isOnline)
            CmdRequestReload();
        else
            CmdRequestRepair();
    }

    [Command(requiresAuthority = false)]
    private void CmdRequestReload()
    {
        weapon.Reload();
        Debug.Log("Weapon reloaded");
    }

    [Command(requiresAuthority = false)]
    private void CmdRequestRepair()
    {
        weapon.Repair(3);
        Debug.Log("Weapon got repaired");
    }
}
