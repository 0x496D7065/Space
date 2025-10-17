using UnityEngine;

public class GameSystems : MonoBehaviour
{
    public static GameSystems Instance;

    public GameObject packetDispatch;

    //System Singleton to Destroy when going back to menu
    public GameObject remotePlayerManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple GameSystems detected!");
            Destroy(this);
            return;
        }
        Instance = this;
    }
    public void Init_Systems_Host()
    {
        //turretFireEventCollector.SetActive(true);
        //remotePlayerManager.SetActive(true);
        //turretManager.SetActive(true);
        packetDispatch.SetActive(true);
    }
    public void Init_Systems_Client()
    {
        //remotePlayerManager.SetActive(true);
        //remoteConstructionManager.SetActive(true);
        packetDispatch.SetActive(true);
    }

    public void CleanupSingletons()
    {
        Debug.Log("Cleaning up gameplay systems...");
        SafeDestroy(remotePlayerManager);
    }

    void SafeDestroy(UnityEngine.Object obj)
    {
        if (obj != null)
        {
            try
            {
                Destroy(obj);
            }
            catch (MissingReferenceException)
            {
                Debug.LogWarning($"[Cleanup] Tried to destroy object '{obj}', but it was already destroyed.");
            }
        }
    }

    private void OnDestroy()
    {
        CleanupSingletons();
    }
}
