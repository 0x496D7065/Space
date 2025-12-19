using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponInteractable: Interactable
{
    [Header("Reference")]
    public Weapon_Base weapon;

    public override void Interact(InputAction.CallbackContext ctx)
    {
        base.Interact(ctx);

        if (weapon.isOnline && ctx.started)
            CmdRequestReload();
        else if (ctx.started)
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
