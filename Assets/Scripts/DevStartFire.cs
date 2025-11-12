using Mirror;
using UnityEngine;

public class DevStartFire : NetworkBehaviour
{
    [Header("Reference")]
    [SerializeField] private GameObject firePrefab;
    [SerializeField] private Camera playerCamera;

    public void TryStartFire()
    {
        Debug.Log("TryStartFire");
        if (playerCamera == null)
        {
            Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            Debug.Log("Camera not found, searching");
            foreach (Camera cam in cameras)
            {
                NetworkBehaviour netParent = cam.GetComponentInParent<NetworkBehaviour>();
                if (netParent != null && netParent.isLocalPlayer)
                {
                    playerCamera = cam;
                    Debug.Log("Camera assigned");
                }
            }
        }
        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.forward;

        CmdStartFire(origin, direction);
    }

    //[Command(requiresAuthority = false)]
    public void CmdStartFire(Vector3 origin, Vector3 direction)
    {
        Debug.Log("CmdStartFire");
        if (Physics.Raycast(origin, direction, out RaycastHit hit, 10f))
        {
            Vector3 firePos = hit.point + hit.normal * 0.01f;
            Quaternion fireRot = Quaternion.LookRotation(hit.normal);
            Debug.DrawRay(hit.point, hit.normal, Color.green, 30);

            GameObject fireNode = Instantiate(firePrefab, firePos, fireRot);
            fireNode.GetComponent<FireNode>().Initialize(firePrefab);
            NetworkServer.Spawn(fireNode);
        }
    }
}
