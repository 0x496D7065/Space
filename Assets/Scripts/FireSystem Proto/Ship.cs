using UnityEngine;
using Mirror;

public class Ship : DetectableObject
{
    [Header("Reference")]
    [SerializeField] private Transform radarCenter;

    [Header("Settings")]
    [SerializeField] private int health;
    [SerializeField] private float orbitRadius;
    [SerializeField] private float orbitSpeed;
    [SerializeField] private float rateOfFire = 5f;
    [SerializeField] private int   damage = 1;


    private float currentAngle = 0f;
    private bool isDead = false;
    private bool init = false;

    public void Init(Transform center, float radius, float speed)
    {
        radarCenter = center;
        orbitRadius = radius;
        orbitSpeed = speed;
        init = true;

        Invoke(nameof(TryShoot), rateOfFire);
    }

    private void Update()
    {
        if (!init || !isServer) { return; }
        currentAngle += orbitSpeed * Time.deltaTime;
        currentAngle %= 360f;

        float rad = currentAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Sin(rad), Mathf.Cos(rad), 0f) * orbitRadius;
        //transform.position = radarCenter.position + offset;
        transform.position = radarCenter.TransformPoint(offset);
    }

    [Server]
    private void TryShoot()
    {
        if (isDead) return;

        if (Random.value > PlayerShip.Instance.evasion)
        {
            ShipRoomType[] possibleRooms = (ShipRoomType[])System.Enum.GetValues(typeof(ShipRoomType));
            ShipRoomType randomRoom = possibleRooms[Random.Range(0, possibleRooms.Length)];

            PlayerShip.Instance.TakeDamage(randomRoom,damage);
            Debug.Log($"{this} fired on player ship and hit for {damage}");
        }
        Invoke(nameof(TryShoot), rateOfFire);
    }
    [Server]
    public void TakeDamage(int damage)
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
            GameManager.Instance.RemoveEnemy(this);
        }
    }
}
