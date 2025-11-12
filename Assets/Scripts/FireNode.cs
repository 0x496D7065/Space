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
    [SerializeField] private LayerMask spreadLayer;

    [Header("Extinguish Settings")]
    [SerializeField] private float extinguishProgress = 0f;
    [SerializeField] private float extinguishThreshold = 3f;

    private bool isBurning = true;

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

    //[Server]
    //void TrySpread()
    //{
    //    if (!isBurning) return;

    //    int spreadAttempts = 6;
    //    Vector3 normal = transform.forward;
    //    Vector3 center = transform.position + normal * 0.5f;



    //    Vector3 tangent = Vector3.Cross(normal, Vector3.up);
    //    if (tangent.sqrMagnitude < 0.01f)
    //        tangent = Vector3.Cross(normal, Vector3.right);

    //    tangent.Normalize();
    //    Vector3 bitangent = Vector3.Cross(normal, tangent).normalized;




    //    for (int i = 0; i < spreadAttempts; i++)
    //    {
    //        float angle = i * (360f / spreadAttempts) * Mathf.Deg2Rad;
    //        Vector3 radialOffset = Mathf.Cos(angle) * tangent + Mathf.Sin(angle) * bitangent;
    //        Vector3 radialPoint = center + radialOffset * spreadRadius;

    //        Ray rayCast = new Ray(radialPoint, -normal);
    //        Debug.DrawRay(radialPoint, -normal, Color.blue, 2);

    //        if (Physics.Raycast(rayCast, out RaycastHit hit, spreadRadius))
    //        {
    //            Vector3 spawnPos = hit.point + hit.normal * 0.01f;
    //            Quaternion spawnRot = Quaternion.LookRotation(hit.normal);

    //            Collider[] overlap = Physics.OverlapSphere(spawnPos, 0.5f);
    //            bool fireAlreadyThere = false;
    //            foreach (var col in overlap)
    //            {
    //                if (col.GetComponent<FireNode>())
    //                {
    //                    fireAlreadyThere = true;
    //                    break;
    //                }
    //            }
    //            if (fireAlreadyThere) continue;

    //            GameObject newFire = Instantiate(fireNodePrefab, spawnPos, spawnRot);
    //            newFire.GetComponent<FireNode>().Initialize(fireNodePrefab);
    //            NetworkServer.Spawn(newFire);
    //        }
    //    }

    //    Invoke(nameof(TrySpread), spreadDelay);
    //}

    [Server]
    void TrySpread()
    {
        if (!isBurning) return;

        Vector3 normal = transform.forward; // this is our stored surface normal
        Vector3 center = transform.position + normal * 0.5f;
        int pointCount = 6;

        // Tangent basis to generate 2D directions
        Vector3 tangent = Vector3.Cross(normal, Vector3.up);
        if (tangent.sqrMagnitude < 0.01f)
            tangent = Vector3.Cross(normal, Vector3.forward);
        tangent.Normalize();
        Vector3 bitangent = Vector3.Cross(normal, tangent).normalized;

        for (int i = 0; i < pointCount; i++)
        {
            float angle = i * (360f / pointCount) * Mathf.Deg2Rad;
            Vector3 radialDir = Mathf.Cos(angle) * tangent + Mathf.Sin(angle) * bitangent;
            Vector3 spreadOrigin = center;

            bool spreadSuccess = false;

            // First, try direct raycast outward from fire center
            Ray outwardRay = new Ray(spreadOrigin, radialDir);
            Debug.DrawRay(spreadOrigin, radialDir * spreadRadius, Color.blue, 2f);

            if (Physics.Raycast(outwardRay, out RaycastHit outwardHit, spreadRadius, spreadLayer))
            {
                spreadSuccess = TrySpawnFireAt(outwardHit.point, outwardHit.normal);
                //Debug.Log("Hit: " + outwardHit.collider.name);
            }
            //else
                //Debug.Log("Missed everything");

            // If no surface hit, try projecting downward onto the same surface
            if (!spreadSuccess)
            {
                Vector3 fallbackPoint = center + radialDir * spreadRadius;
                Ray fallbackRay = new Ray(fallbackPoint, -normal); // down towards surface
                Debug.DrawRay(fallbackPoint, -normal * 1.5f, Color.cyan, 2f);

                if (Physics.Raycast(fallbackRay, out RaycastHit downHit, 1.5f, spreadLayer))
                {
                    TrySpawnFireAt(downHit.point, downHit.normal);
                }
            }
        }

        //Invoke(nameof(TrySpread), spreadDelay);
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
