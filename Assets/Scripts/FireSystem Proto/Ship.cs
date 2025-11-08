using UnityEngine;
using Mirror;

public class Ship : DetectableObject
{
    [SerializeField] private float health;
    [SerializeField] private float orbitRadius;
    [SerializeField] private float orbitSpeed;

    [SerializeField] private Transform radarCenter;


    private float currentAngle = 0f;
    private bool isDead = false;
    private bool init = false;

    public void Init(Transform center, float radius, float speed)
    {
        radarCenter = center;
        orbitRadius = radius;
        orbitSpeed = speed;
        init = true;
    }

    private void Update()
    {
        if (!init || !isServer) { return; }
        currentAngle += orbitSpeed * Time.deltaTime;
        currentAngle %= 360f;

        float rad = currentAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Sin(rad), Mathf.Cos(rad), 0f) * orbitRadius;
        transform.position = radarCenter.position + offset;
    }
    public void TakeDamage(float damage)
    {
        if (!isServer || isDead) return;

        health -= damage;
        if (health <= 0)
        {
            isDead = true;
        }
        Debug.Log($"Ship hit. Health: {health}, isDead:{isDead}");
        if (isDead)
        {
            //NetworkServer.Destroy(this.gameObject);
            //this is temporary fix until enemy ships have logic to spawn on server instead of already being in the scene
            //if (isServer)
            //{
            //    Destroy(this.gameObject);
            //    RpcDestroyOnClients();
            //}
            EnemyManager.Instance.RemoveEnemy(this);
        }
    }
    //[ClientRpc]
    //void RpcDestroyOnClients()
    //{
    //    if (!isServer) // server already did it
    //        Destroy(this.gameObject);
    //}
}
