using UnityEngine;
using Mirror;

public class Missile : NetworkBehaviour
{

    [SerializeField] private float speed;
    [SerializeField] private float maxDistance;
    [SerializeField] private int damage;

    [SerializeField] private ShipRoomType targetedShipRoom;
    [SerializeField] private LayerMask enemyLayer;

    private Vector3 startPos;
    private Collider owner;
    private bool init = false;
    public bool isArmed = false;

    public void Init(float azimuth, Transform StartPos, ShipRoomType target, Collider Owner)
    {
        transform.localRotation = Quaternion.Euler(0, 0, azimuth);
        startPos = StartPos.position;
        targetedShipRoom = target;
        owner = Owner;
        Debug.Log($"azimuth init {azimuth}");
        init = true;
    }

    private void Update()
    {
        if (!isServer || !init) 
            return;

        transform.position += transform.up * speed * Time.deltaTime;

        float traveled = Vector2.Distance(transform.position, startPos);
        if (traveled > maxDistance)
            NetworkServer.Destroy(this.gameObject);
    }

    public void OnTriggerExit(Collider other)
    {
        if (isServer && !isArmed)
        {
            isArmed = true;
            //Debug.Log("Missile armed after exiting owner collider.");
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        //Debug.Log("OnTriggerEnter");
        if (!isServer || !isArmed) return;
        //Debug.Log($"Armed State = {isArmed}");
        other.TryGetComponent<EnemyShip>(out var ship);
        if (ship != null)
        {
            ship.TakeDamage(damage);
            NetworkServer.Destroy(this.gameObject);
        }
        if (other.name == "PlayerShipCollider")
        {
            PlayerShip.Instance.TakeDamage(targetedShipRoom, damage);
            NetworkServer.Destroy(this.gameObject);
        }
    }
}
