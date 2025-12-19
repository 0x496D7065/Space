using Mirror;
using UnityEngine;

public class PickableObject : NetworkBehaviour  
{
    private Rigidbody rb;
    public Quaternion pickupRotation = Quaternion.identity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnStartAuthority()
    {
        Debug.LogWarning("OnStartAuthorityCube");
        rb.isKinematic = true;
    }

    public override void OnStartClient()
    {
        if (!isOwned)
        {
            Debug.LogWarning("OnStartClientCube");
            //rb.isKinematic = false;
        }
    }
}
