using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenuLoader: MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(WaitForSteamInitialization());
    }

    private IEnumerator WaitForSteamInitialization()
    {
        // Wait until SteamManager is initialized
        while (!SteamManager.Initialized)
        {
            yield return null;
        }
        while (!SteamP2PManager.Initialized)
        {
            yield return null;
        }
        while (!SteamLobbyManager.Initialized)
        {
            yield return null;
        }
        Debug.Log("Steam fully initialized. Loading MainMenuScene...");
        SceneManager.LoadScene(1);
    }
}
