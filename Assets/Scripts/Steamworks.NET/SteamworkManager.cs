using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SteamworkManager : NetworkManager
{
    [Header("Scene Names")]
    public string gameplayScene = "DevTestScene";

    public Transform[] spawnPossibility;
    private int playerOrder = 0;

    public override void OnServerSceneChanged(string scene)
    {
        base.OnServerSceneChanged(scene);
        Debug.Log("[Server] Scene changed to: " + scene);
    }

    public override void OnClientSceneChanged()
    {
        base.OnClientSceneChanged();
        if (!NetworkClient.ready)
        {
            NetworkClient.Ready();
            Debug.Log("[Client] Called NetworkClient.Ready()");
        }
        Debug.Log("[Client] Scene changed and ready");
    }

    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);
        Debug.Log($"[Server] Client {conn.connectionId} is ready");


        if (SceneManager.GetActiveScene().name == gameplayScene && conn.identity == null)
        {
            playerOrder++;

            GameObject player = Instantiate(playerPrefab);

            try
            {
                player.transform.position = spawnPossibility[playerOrder - 1].position;
            }
            catch (System.Exception)
            {
                Debug.LogError("No Spawn Possibility");
            }

            player.name = "Player " + playerOrder;

            NetworkServer.AddPlayerForConnection(conn, player);
            Debug.Log($"[Server] Player spawned for connection {conn.connectionId} / {player.name}");
        }
    }
}
