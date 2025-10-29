using Mirror;
using UnityEngine;

public class PickableObject : NetworkBehaviour  
{
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnStartAuthority()
    {
        rb.isKinematic = true;
    }

    public override void OnStopAuthority()
    {
        rb.isKinematic = false;
    }

    public override void OnStartClient()
    {
        if (!isOwned)
        {
            rb.isKinematic=true;
        }
    }
}
