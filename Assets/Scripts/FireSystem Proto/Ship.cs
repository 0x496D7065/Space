using UnityEngine;

public class Ship : DetectableObject
{
    [SerializeField] private float health;
    [SerializeField] private float orbitRadius;
    [SerializeField] private float orbitSpeed;

    [SerializeField] private RectTransform radarCenter;


    private float currentAngle = 0f;
    private RectTransform rt;
    private bool isDead = false;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    private void Update()
    {
        currentAngle += orbitSpeed * Time.deltaTime;
        currentAngle %= 360f;

        float rad = currentAngle * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)) * orbitRadius;

        rt.anchoredPosition = radarCenter.anchoredPosition + offset;
    }
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        health -= damage;
        if (health <= 0)
        {
            isDead = true;
        }
        Debug.Log($"Ship hit. Health: {health}, isDead:{isDead}");
        if (isDead)
            Destroy(this.gameObject);
    }
}
