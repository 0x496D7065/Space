using UnityEngine;
using Mirror;

public class Missile : NetworkBehaviour
{

    [SerializeField] private float speed;
    [SerializeField] private float maxDistance;
    [SerializeField] private int damage;

    [SerializeField] private LayerMask enemyLayer;

    private Transform startPos;
    private bool init = false;

    public void Init(float azimuth, Transform radarCenter)
    {
        transform.rotation = Quaternion.Euler(-azimuth, 0, 0);
        init = true;
    }

    private void Update()
    {
        if (!isServer || !init) 
            return;

        transform.position += transform.up * speed * Time.deltaTime;

        if (Vector2.Distance(transform.position, startPos.position) > maxDistance)
            NetworkServer.Destroy(this.gameObject);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;

        other.TryGetComponent<Ship>(out var ship);
        if (ship != null)
        {
            ship.TakeDamage(damage);
            NetworkServer.Destroy(this.gameObject);
        }
    }
}
