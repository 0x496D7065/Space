using System.Collections.Generic;
using Newtonsoft.Json.Bson;
using Steamworks;
using UnityEngine;
using UnityEngine.Rendering;

public class SteamLobbyManager : MonoBehaviour
{
    public  static  SteamLobbyManager Instance;
    public  SteamLobbyUIManager       steamLobbyUIManager;
    private Callback<LobbyCreated_t> _lobbyCreated;
    private Callback<GameLobbyJoinRequested_t> _joinRequested;
    private Callback<LobbyEnter_t> _lobbyEntered;
    private Callback<GameRichPresenceJoinRequested_t> _onJoinViaRichPresence;
    public  MainMenuUI mainMenu;

    public static CSteamID CurrentLobbyID;
    public static bool Initialized { get; private set; } = false;
    public Dictionary<CSteamID, bool> clientReadyStates = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Initialized = true;
    }

    public void InitMainMenu()
    {
        mainMenu = FindAnyObjectByType<MainMenuUI>();
    }

    private void Start()
    {
        _lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        _joinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequested);
        _lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        _onJoinViaRichPresence = Callback<GameRichPresenceJoinRequested_t>.Create(OnJoinViaRichPresence);
        InitMainMenu();
    }

    public void HostLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 4);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError("Lobby creation failed");
            return;
        }
        if (steamLobbyUIManager == null)
            steamLobbyUIManager = FindFirstObjectByType<SteamLobbyUIManager>();
        Debug.Log("Lobby created successfully");

        CurrentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);

        // Set host Steam ID in lobby metadata
        SteamMatchmaking.SetLobbyData(CurrentLobbyID, "hostid", SteamUser.GetSteamID().ToString());

        SteamP2PManager.Instance.isHost = true;
        SteamP2PManager.Instance.StartHost();
        SteamFriends.SetRichPresence("connect", CurrentLobbyID.ToString());
        SteamFriends.SetRichPresence("status", "In Lobby");

        Debug.Log($"host flag = {SteamP2PManager.Instance.isHost}");
        mainMenu.readyButton.SetActive(!SteamP2PManager.Instance.isHost);
        mainMenu.startButton.SetActive(SteamP2PManager.Instance.isHost);

        steamLobbyUIManager.RefreshPlayerList();
    }

    private void OnJoinRequested(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("Join requested via Steam overlay/invite");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnJoinViaRichPresence(GameRichPresenceJoinRequested_t callback)
    {
        Debug.Log($"[Steam] Join via rich presence: {callback.m_rgchConnect}");
        if (ulong.TryParse(callback.m_rgchConnect, out ulong lobbyId))
        {
            SteamMatchmaking.JoinLobby(new CSteamID(lobbyId));
        }
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        Debug.Log("Lobby entered");

        CurrentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);

        if (!SteamP2PManager.Instance.isHost)
        {
            if (steamLobbyUIManager == null)
                steamLobbyUIManager = FindFirstObjectByType<SteamLobbyUIManager>();
            mainMenu.readyButton.SetActive(!SteamP2PManager.Instance.isHost);
            mainMenu.startButton.SetActive(SteamP2PManager.Instance.isHost);
            mainMenu.ShowLobbyMenu();
            if (mainMenu.MainMenu.activeSelf)
                mainMenu.ToggleMenu();
            steamLobbyUIManager.RefreshPlayerList();
            // Get the host Steam ID from lobby metadata
            string hostIdStr = SteamMatchmaking.GetLobbyData(CurrentLobbyID, "hostid");
            if (ulong.TryParse(hostIdStr, out ulong hostId))
            {
                CSteamID hostSteamID = new(hostId);
                SteamP2PManager.Instance.ConnectToHost(hostSteamID);
                SteamP2PManager.Instance.isHost = false;
            }
            else
            {
                Debug.LogError("Failed to parse host Steam ID from lobby");
            }
        }
    }
    public void LeaveLobby()
    {
        if (SteamLobbyManager.CurrentLobbyID.IsValid())
        {
            Debug.Log("Leaving lobby...");
            SteamMatchmaking.LeaveLobby(SteamLobbyManager.CurrentLobbyID);
            SteamLobbyManager.CurrentLobbyID = CSteamID.Nil;
        }

        // If host, also shut down the listen socket
        if (SteamP2PManager.Instance.isHost)
        {
            SteamP2PManager.Instance.DisconnectAllClients();
            SteamP2PManager.Instance.isHost = false;
            SteamP2PManager.Instance.clientLobbyJoinTimes.Clear();
            clientReadyStates.Clear();
        }
        else
            SteamP2PManager.Instance.DisconnectFromHost();

        if (steamLobbyUIManager != null)
            steamLobbyUIManager.ClearPlayerList();
    }
}
