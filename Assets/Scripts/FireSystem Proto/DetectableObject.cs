using UnityEngine;
using Mirror;

public class DetectableObject : NetworkBehaviour
{
    [SerializeField] private Animator anim;

    public virtual void Detected()
    {
        anim.SetTrigger("detected");
    }
}
