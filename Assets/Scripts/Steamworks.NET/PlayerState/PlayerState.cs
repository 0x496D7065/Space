using Unity.Mathematics;
using UnityEngine;

//public enum FPSMovementState : byte
//{
//    Idle,
//    Walking,
//    Sprinting,
//    InAir,
//    Sliding
//}

//public enum FPSPoseState : byte
//{
//    Standing,
//    Crouching,
//    Prone
//}
public struct PlayerState
{
    public float3 position;
    public float3 forward;

    //public float pitch;
    //public float yaw;

    //public Vector2 moveInput;

    public short Health;

    //public FPSMovementState movementState;
    //public FPSPoseState poseState;
    //public FPSAimState aimState;
    //public FPSActionState actionState;

    //public byte activeWeaponIndex;
}