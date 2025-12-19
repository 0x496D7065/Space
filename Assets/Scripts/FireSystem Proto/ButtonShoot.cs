using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class ButtonShoot : Interactable
{
    [Header("Reference")]
    [SerializeField] private RadarShoot radarShoot;

    public override void Interact(InputAction.CallbackContext ctx)
    {
        base.Interact(ctx);

        if (ctx.started)
            CmdShoot();
    }

    [Command(requiresAuthority = false)]
    private void CmdShoot()
    {
        radarShoot.FireWeapon(radarShoot.azimuth);
    }
}
