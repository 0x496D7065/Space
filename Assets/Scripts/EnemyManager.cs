using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class EnemyManager : NetworkBehaviour
{
    public static EnemyManager Instance { get; private set; }
    private readonly List<Ship> enemyList = new();

    [Header("Reference")]
    [SerializeField] private Transform radarCenter;
    [SerializeField] private GameObject enemyShipPrefab;

    [Header("Settings")]
    [SerializeField] private int maxEnemy;
    [SerializeField] private float spawnRate;

    [SerializeField] private float minSpawnRadius = 0.15f;
    [SerializeField] private float maxSpawnRadius = 0.4f;

    [SerializeField] private float minSpeed = 5f;
    [SerializeField] private float maxSpeed = 20f;
    private float timeElapsed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (!isServer) { return; }
        timeElapsed += Time.deltaTime;
        if (enemyList.Count < maxEnemy && timeElapsed >= spawnRate)
        {
            SpawnEnemyOnServer();
            timeElapsed = 0f;
        }
    }

    private void SpawnEnemyOnServer()
    {
        float angle = Random.Range(0f, 360f);
        float radius = Random.Range(minSpawnRadius, maxSpawnRadius);

        Vector3 spawnOffset = new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad),
            0f) * radius;

        Vector3 spawnPosition = radarCenter.position + spawnOffset;

        GameObject enemyShip = Instantiate(enemyShipPrefab, spawnPosition, Quaternion.identity);

        enemyShip.TryGetComponent<Ship>( out var shipScript);
        if (shipScript != null)
        {
            float speed = Random.Range(minSpeed, maxSpeed);
            shipScript.Init(radarCenter, radius, speed);
            enemyList.Add(shipScript);
        }

        NetworkServer.Spawn(enemyShip);
    }

    public void RemoveEnemy(Ship ship)
    {
        if (!isServer) return;

        if (enemyList.Contains(ship))
        {
            enemyList.Remove(ship);
            NetworkServer.Destroy(ship.gameObject);
            Debug.Log($"NetworkServer.Destroy() called");
        }
    }
}
