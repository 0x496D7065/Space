using System.Collections.Generic;
using Mirror;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private NetworkManager networkManager;
    private const string HostAddressKey = "HostAddress";
    public GameObject playerPrefab;

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
        networkManager = FindFirstObjectByType<NetworkManager>();
        InitMainMenu();
        if (!SteamManager.Initialized)
        {
            Debug.LogError("SteamManager not initialized");
            return; 
        }
    }

    public void HostLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);
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
        networkManager.StartHost();
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
        SteamFriends.SetRichPresence("connect", CurrentLobbyID.ToString());
        SteamFriends.SetRichPresence("status", "In Lobby");

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
        if (NetworkServer.active && NetworkClient.isConnected) 
        {
            mainMenu.startButton.SetActive(true);
            return; 
        }
        Debug.Log("Lobby entered as client");

        CurrentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);

        if (!NetworkServer.active)
        {
            if (steamLobbyUIManager == null)
                steamLobbyUIManager = FindFirstObjectByType<SteamLobbyUIManager>();

            mainMenu.ShowLobbyMenu();
            if (mainMenu.MainMenu.activeSelf)
                mainMenu.ToggleMenu();
            steamLobbyUIManager.RefreshPlayerList();

            string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);
            networkManager.networkAddress = hostAddress;
            networkManager.StartClient();
        }
    }
    public void LeaveLobby()
    {
        if (SteamLobbyManager.CurrentLobbyID.IsValid())
        {
            Debug.Log("Leaving lobby...");
            if (NetworkServer.active)
                networkManager.StopHost();
            else
                networkManager.StopClient();
            SteamMatchmaking.LeaveLobby(SteamLobbyManager.CurrentLobbyID);
            SteamLobbyManager.CurrentLobbyID = CSteamID.Nil;
        }
        if (steamLobbyUIManager != null)
            steamLobbyUIManager.ClearPlayerList();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene Loaded: {scene.buildIndex}");
        if (scene.buildIndex == 1)
        {
            SteamLobbyManager.Instance.InitMainMenu();
        }
    }

}
