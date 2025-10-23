using UnityEngine;

public class RadarShoot : MonoBehaviour
{
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private Transform radarCenter;
    [SerializeField] LayerMask enemyLayer;

    [Range(0f, 360f)]
    [SerializeField] public float azimuth = 0f;

    [ContextMenu("Fire Missile")]
    public void FireFromInspector()
    {
        FireMissile(azimuth);
    }

    public void FireMissile(float azimuth)
    {
        Vector2 direction = AzimuthToDirection(azimuth);
        GameObject missile = Instantiate(missilePrefab, radarCenter);
        Missile missileScript = missile.GetComponent<Missile>();
        missileScript.Init(direction, azimuth);

        RectTransform missileRT = missile.GetComponent<RectTransform>();
        RectTransform centerRT = radarCenter.GetComponent<RectTransform>();

        // Spawn at center of radar
        missileRT.anchoredPosition = centerRT.anchoredPosition;
        Debug.Log($"Azimuth: {azimuth} Direction: {direction}");
    }

    private Vector2 AzimuthToDirection(float azimuthDegrees)
    {
        float radians = -azimuthDegrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Sin(radians), Mathf.Cos(radians));
    }
}
