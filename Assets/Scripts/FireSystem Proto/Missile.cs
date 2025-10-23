using CitrioN.Common;
using Newtonsoft.Json.Bson;
using UnityEngine;

public class Missile : MonoBehaviour
{

    [SerializeField] private float speed;
    [SerializeField] private float maxDistance;
    [SerializeField] private float damage;

    [SerializeField] private LayerMask enemyLayer;

    private Vector2 direction;
    private RectTransform rt;
    private Vector2 startPos;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    public void Init(Vector2 dir, float azimuth)
    {
        direction = dir.normalized;
        transform.rotation = Quaternion.Euler(0, 0, azimuth + 90);
        startPos = transform.position;
    }

    private void Update()
    {
        transform.position += (Vector3)(speed * Time.deltaTime * direction);
        Debug.Log($"Distance: {Vector2.Distance(transform.position, startPos)}");
        if (Vector2.Distance(transform.position, startPos) > maxDistance)
            Destroy(this.gameObject);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Ship>())
        {
            other.GetComponent<Ship>().TakeDamage(damage);
            Destroy(this.gameObject);
        }
    }
}
