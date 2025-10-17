using Unity.Mathematics;

[System.Serializable]
public struct EnemyUnitState
{
    public long     timeStamp;//8
    public float3   position;//12
    public float3   forward;//12
    public float    moveSpeed;//4
    public ushort   ID;//2
    public bool     IsDead;//1
}