using UnityEngine;
[System.Serializable]
public struct RemoteFireEvent
{
    public Vector3  hitPoint;
    public ushort   targetId;
    public byte     damage;
}

public struct CompressedShot
{
    public ushort targetId;
    public byte damage;
    public byte hitX, hitY, hitZ;
}