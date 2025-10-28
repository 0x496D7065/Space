using UnityEngine;

public class DetectableObject : MonoBehaviour
{
    [SerializeField] private Animator anim;

    public virtual void Detected()
    {
        anim.SetTrigger("detected");
    }
}
