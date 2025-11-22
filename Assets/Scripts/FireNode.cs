using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using static UnityEngine.UI.Image;

public class FireNode : NetworkBehaviour, IExtinguishable
{
    [Header("Reference")]
    [SerializeField] private GameObject fireVFXPrefab;
    [SerializeField] private GameObject fireNodePrefab;
    private GameObject fireVFX;

    [Header("Spread Settings")]
    [SerializeField] private float burnTime = 10f;
    [SerializeField] private float spreadDelay = 3f;
    [SerializeField] private float spreadRadius = 2f;
    [SerializeField] private int minSpreadDirections = 1;
    [SerializeField] private int maxSpreadDirections = 3;
    [SerializeField] private float chanceToSpread = 0.3f;
    [SerializeField] private LayerMask spreadLayer;
    [SerializeField] private LayerMask fireMask;


    [Header("Extinguish Settings")]
    [SerializeField] private float extinguishProgress = 0f;
    [SerializeField] private float extinguishThreshold = 3f;

    private bool isBurning = true;

    private static readonly Vector3[] spreadDirs = new Vector3[]
    {
        Quaternion.AngleAxis(0, Vector3.forward) * Vector3.right,
        Quaternion.AngleAxis(60, Vector3.forward) * Vector3.right,
        Quaternion.AngleAxis(120, Vector3.forward) * Vector3.right,
        Quaternion.AngleAxis(180, Vector3.forward) * Vector3.right,
        Quaternion.AngleAxis(240, Vector3.forward) * Vector3.right,
        Quaternion.AngleAxis(300, Vector3.forward) * Vector3.right,
    };

    public void Initialize(GameObject prefabRef)
    {
        fireNodePrefab = prefabRef;
    }

    public override void OnStartServer()
    {
        Invoke(nameof(Extinguish), burnTime);
        Invoke(nameof(TrySpread), spreadDelay);
    }
    private void Start()
    {
        if (fireVFXPrefab != null)
        {
            fireVFX = Instantiate(fireVFXPrefab, transform.position, transform.rotation);
            fireVFX.transform.SetParent(transform);
        }
    }

    [Server]
    private void TrySpread()
    {
        Collider[] nearbyFires = Physics.OverlapSphere(transform.position, spreadRadius, fireMask);
        List<int> freeIndices = GetFreeDirectionIndices(nearbyFires);
        List<int> chosenDirs = PickSpreadDirections(freeIndices);

        foreach (int i in chosenDirs)
        {
            Vector3 spreadDir = transform.TransformDirection(spreadDirs[i]);
            Vector3 origin = transform.position + transform.forward * 1f;

            if (Random.value > chanceToSpread)
            {
                Debug.DrawRay(origin, spreadDir * spreadRadius, Color.green, 2f);
                continue;
            }

            Ray outwardRay = new Ray(origin, spreadDir);
            Debug.DrawRay(origin, spreadDir * spreadRadius, Color.blue, 2f);
            if (Physics.Raycast(outwardRay, out RaycastHit hit, spreadRadius, spreadLayer))
            {
                TrySpawnFireAt(hit.point, hit.normal);
            }
            else
            {
                Ray downwardRay = new Ray(origin + spreadDir * spreadRadius, -transform.forward);
                Debug.DrawRay(origin + spreadDir * spreadRadius, -transform.forward, Color.blue, 2f);
                if (Physics.Raycast(downwardRay, out RaycastHit fallbackHit, 1.5f, spreadLayer))
                {
                    TrySpawnFireAt(fallbackHit.point, fallbackHit.normal);
                }
            }
        }

        Invoke(nameof(TrySpread), spreadDelay);
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spreadRadius);
    }
    List<int> GetFreeDirectionIndices(Collider[] nearbyFires)
    {
        List<int> blocked = new();

        foreach (Collider col in nearbyFires)
        {
            if (col.TryGetComponent<FireNode>(out var node) && node != this)
            {
                Vector3 toOther = (col.transform.position - transform.position).normalized;

                for (int i = 0; i < spreadDirs.Length; i++)
                {
                    // Convert local dir to world space
                    Vector3 dir = transform.TransformDirection(spreadDirs[i]);
                    float dot = Vector3.Dot(dir, toOther);

                    if (dot > 0.85f) // ~30° tolerance
                    {
                        if (!blocked.Contains(i))
                            blocked.Add(i);
                    }
                }
            }
        }

        // Now return only the free directions
        List<int> free = new();
        for (int i = 0; i < spreadDirs.Length; i++)
        {
            if (!blocked.Contains(i))
                free.Add(i);
        }

        return free;
    }

    List<int> PickSpreadDirections(List<int> freeIndices)
    {
        int count = Mathf.Min(Random.Range(minSpreadDirections, maxSpreadDirections + 1), freeIndices.Count);
        return freeIndices.OrderBy(_ => Random.value).Take(count).ToList();
    }

    bool TrySpawnFireAt(Vector3 hitPoint, Vector3 hitNormal)
    {
        Vector3 spawnPos = hitPoint + hitNormal * 0.01f;
        Quaternion spawnRot = Quaternion.LookRotation(hitNormal);

        Collider[] overlap = Physics.OverlapSphere(spawnPos, 0.5f);
        foreach (var col in overlap)
        {
            if (col.GetComponent<FireNode>())
                return false; // already burning here
        }

        GameObject newFire = Instantiate(fireNodePrefab, spawnPos, spawnRot);
        newFire.GetComponent<FireNode>().Initialize(fireNodePrefab);
        NetworkServer.Spawn(newFire);
        return true;
    }




















    //Extinguish logic
    [Server]
    public void ApplyExtinguish(float amount)
    {
        if (!isBurning) { return; }

        extinguishProgress += amount;
        //Debug.Log($"extinguishProgression{extinguishProgress}");
        if (extinguishProgress >= extinguishThreshold)
        {
            Extinguish();
        }
    }

    [Server]
    void Extinguish()
    {
        isBurning = false;
        //RpcStopFireVFX();
        NetworkServer.Destroy(this.gameObject);
    }

    [ClientRpc]
    void RpcStopFireVFX()
    {
        if (fireVFX != null)
            Destroy(fireVFX);
    }
}
