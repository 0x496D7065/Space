using UnityEngine;

public class RemoteFPSController : MonoBehaviour
{
    public ushort TargetableID;

    public void ApplyNetworkState(PlayerState state)
    {
        transform.position = state.position;
        transform.forward = state.forward;
    }
}
