using UnityEngine;
using Mirror;

public class Missile : NetworkBehaviour
{

    [SerializeField] private float speed;
    [SerializeField] private float maxDistance;
    [SerializeField] private int damage;

    [SerializeField] private LayerMask enemyLayer;

    private Vector2 direction;
    private Transform startPos;
    private bool init = false;

    public void Init(Vector2 dir, float azimuth, Transform radarCenter)
    {
        direction = dir.normalized;
        transform.rotation = Quaternion.Euler(0, -90, azimuth + 90);
        startPos = radarCenter;
        init = true;
    }

    private void Update()
    {
        if (!isServer || !init) return;
        transform.position += (Vector3)(speed * Time.deltaTime * direction);
        //Debug.Log($"Distance: {Vector2.Distance(transform.position, startPos.position)}");
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
