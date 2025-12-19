using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class FireExtinguisher : Interactable
{
    [Header("Reference")]
    [SerializeField] private ParticleSystem sprayEffect;

    [Header("Extinguisher Settings")]
    [SerializeField] private float sprayRate = 1f;
    [SerializeField] private float sprayRange = 4f;
    [SerializeField] private float sprayRadius = 1.5f;

    private bool isSpraying = false;

    public override void Interact(InputAction.CallbackContext ctx)
    {
        base.Interact(ctx);

        //Debug.Log("Extinguisher interact");
        //if (sprayEffect != null && !sprayEffect.isPlaying)
        if (ctx.started)
        {
            //Debug.Log("Extinguisher starting");
            sprayEffect.Play();
            isSpraying = true;
            CmdToggleSpray(isSpraying);
        }
        //else if (sprayEffect.isPlaying)
        else if (ctx.canceled)
        {
            //Debug.Log("Extinguisher stopping");
            sprayEffect.Stop();
            isSpraying = false;
            CmdToggleSpray(isSpraying);
        }
    }

    private void FixedUpdate()
    {
        if (!isSpraying) { return; }
        Vector3 origin = transform.position + transform.forward + Vector3.up * 0.5f;
        CmdSpray(origin, transform.forward);
    }

    //[Command]
    void CmdSpray(Vector3 origin, Vector3 direction)
    {
        RaycastHit[] hits = Physics.SphereCastAll(
            origin,
            sprayRadius,
            direction,
            sprayRange
        );

        foreach (var hit in hits)
        {
            if (hit.collider.TryGetComponent<IExtinguishable>(out var extinguishable))
            {
                extinguishable.ApplyExtinguish(sprayRate * Time.fixedDeltaTime);
            }
        }
    }

    [Command]
    private void CmdToggleSpray(bool spraying)
    {
        // Local player already played effect.
        // Now tell all other players.
        foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
        {
            if (conn != connectionToClient) // exclude the sender
            {
                ShowEffectToOthers(conn, spraying);
            }
        }
    }
    [TargetRpc]
    private void ShowEffectToOthers(NetworkConnection target, bool spraying)
    {
        if (spraying && !sprayEffect.isPlaying)
            sprayEffect.Play();
        else if (!spraying && sprayEffect.isPlaying)
            sprayEffect.Stop();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Gizmo settings
        Gizmos.color = Color.cyan;

        // Start of the spray
        Vector3 origin = transform.position + transform.forward + Vector3.up * 0.5f;
        Vector3 direction = transform.forward;

        // Visualize the center line of the spray
        Gizmos.DrawLine(origin, origin + direction * sprayRange);

        // Draw a wire sphere at the origin
        Gizmos.DrawWireSphere(origin, sprayRadius);

        // Draw a wire sphere at the end of the spherecast
        Vector3 end = origin + direction * sprayRange;
        Gizmos.DrawWireSphere(end, sprayRadius);
    }
#endif

}
