using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Steamworks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SteamP2PManager : MonoBehaviour
{
    public static SteamP2PManager Instance;

    private Callback<SteamNetConnectionStatusChangedCallback_t> _connectionChanged;
    private HSteamListenSocket _listenSocket;
    private Dictionary<HSteamNetConnection, CSteamID> _connections = new();
    public  Dictionary<CSteamID, DateTime> clientLobbyJoinTimes = new();

    private const int RELIABLE = 8;
    private const int UNRELIABLE = 0;
    public const ulong DEV_HOST_ID = 76561198020724149;
    public       ulong Own_ID;
    public       bool isHost;
    private readonly IntPtr[] _messageBuffer = new IntPtr[32];

    public Action<CSteamID, byte[]> OnDataReceived;
    public static bool Initialized { get; private set; } = false;

    public static bool  GameOn = false;
    public static DateTime GameStartTime;

    public enum LobbyPacketType : byte
    {
        StartGame = 1,
        ReadyFlag = 2,
    }


    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Initialized = true;
    }

    void Start()
    {
        Own_ID = SteamUser.GetSteamID().m_SteamID;
        _connectionChanged = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnConnectionChanged);
        //if (Own_ID == DEV_HOST_ID)
        //{
        //    Debug.Log("Running as Host.");
        //    isHost = true;
        //    StartHost();
        //    GameSystems.Instance.Init_Systems_Host();
        //}
        //else
        //{
        //    Debug.Log("Running as Client. Connecting to Host...");
        //    isHost = false;
        //    CSteamID hostId = new CSteamID(DEV_HOST_ID);
        //    ConnectToHost(hostId);
        //    GameSystems.Instance.Init_Systems_Client();
        //}
    }

    public List<CSteamID> GetConnectedClientIDs()
    {
        return new List<CSteamID>(_connections.Values);
    }

    public void StartHost()
    {
        SteamNetworkingConfigValue_t[] options = Array.Empty<SteamNetworkingConfigValue_t>();
        _listenSocket = SteamNetworkingSockets.CreateListenSocketP2P(0, 0, options);
        Debug.Log("Host listening for Steam P2P connections.");
    }

    public void ConnectToHost(CSteamID hostId)
    {
        SteamNetworkingConfigValue_t[] options = Array.Empty<SteamNetworkingConfigValue_t>();
        SteamNetworkingIdentity identity = new SteamNetworkingIdentity();
        identity.SetSteamID(hostId);
        HSteamNetConnection connection = SteamNetworkingSockets.ConnectP2P(ref identity, 0, 0, options);
        Debug.Log($"Attempting connection to host: {hostId}");
    }

    private void OnConnectionChanged(SteamNetConnectionStatusChangedCallback_t data)
    {
        var conn = data.m_hConn;

        switch (data.m_info.m_eState)
        {
            case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting:
                SteamNetworkingSockets.AcceptConnection(conn);
                CSteamID remoteId = data.m_info.m_identityRemote.GetSteamID();
                _connections[conn] = remoteId;
                Debug.Log($"Accepted new P2P connection from {remoteId}");
                break;
            case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:
                Debug.Log("Steam P2P connection fully established.");
                if (isHost)
                {
                    CSteamID clientId = data.m_info.m_identityRemote.GetSteamID();
                    clientLobbyJoinTimes.Add(clientId, DateTime.UtcNow);
                    Debug.Log($"Host: Client {clientId} fully connected.");
                    if (GameOn)
                        LaunchGameOnNewClient(clientId);
                }
                break;
            case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer:
            case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally:
                if (_connections.TryGetValue(conn, out var disconnectedId))
                {
                    Debug.LogWarning($"Connection to {disconnectedId} lost.");
                    if (GameOn && isHost) //This mean, host gets notification that a client left for any reason, so only host get that information
                    {
                        //RemotePlayerManager.Instance.remotePlayers.TryGetValue(disconnectedId, out var remote);
                        //RemotePlayerManager.Instance.DisableRemote(disconnectedId.m_SteamID, remote);
                        clientLobbyJoinTimes.Remove(disconnectedId);
                    }
                    else if (GameOn && !isHost) // this mean, host left for any reason, so only clients get that information
                    {
                        Destroy(GameSystems.Instance.gameObject);
                        SteamLobbyManager.Instance.LeaveLobby();
                        StopAllCoroutines();
                        SceneManager.LoadScene(1);
                    }
                }
                else
                {
                    Debug.LogWarning("Unknown connection lost.");
                }
                _connections.Remove(conn);
                break;
        }
    }

    void Update()
    {
        var conns = new List<HSteamNetConnection>(_connections.Keys); // avoid modifying during iteration

        foreach (var conn in conns)
        {
            int count = SteamNetworkingSockets.ReceiveMessagesOnConnection(conn, _messageBuffer, _messageBuffer.Length);

            for (int i = 0; i < count; i++)
            {
                IntPtr msgPtr = _messageBuffer[i];

                if (msgPtr == IntPtr.Zero)
                    continue;
                var msg = Marshal.PtrToStructure<SteamNetworkingMessage_t>(msgPtr);

                byte[] data = new byte[msg.m_cbSize];
                Marshal.Copy(msg.m_pData, data, 0, data.Length);

                if (_connections.TryGetValue(conn, out var senderId))
                {
                    OnDataReceived?.Invoke(senderId, data);
                }

                SteamNetworkingMessage_t.Release(msgPtr);
            }
        }
    }

    public void SendToAll(byte[] data, bool reliable = false)
    {
        int flags = reliable ? RELIABLE : UNRELIABLE;

        // Allocate unmanaged memory and copy the byte[] into it
        IntPtr unmanagedPointer = Marshal.AllocHGlobal(data.Length); //Steamworks.NET API is a wrapper over native C++ Steam SDK and does not understand C# managed memory.
        Marshal.Copy(data, 0, unmanagedPointer, data.Length);

        foreach (var conn in _connections.Keys)
        {
            SteamNetworkingSockets.SendMessageToConnection(
                conn,
                unmanagedPointer,
                (uint)data.Length,
                flags,
                out long _);
        }

        Marshal.FreeHGlobal(unmanagedPointer);
    }

    public void SendToAllUnManaged(IntPtr unmanagedPointer, uint length, bool reliable = false)
    {
        int flags = reliable ? RELIABLE : UNRELIABLE;

        foreach (var conn in _connections.Keys)
        {
            SteamNetworkingSockets.SendMessageToConnection(
                conn,
                unmanagedPointer,
                length,
                flags,
                out long _);
        }

        Marshal.FreeHGlobal(unmanagedPointer);
    }

    public unsafe void SendToAllUnManagedUnsafe(IntPtr unmanagedPointer, uint length, bool reliable = false)
    {
        int flags = reliable ? RELIABLE : UNRELIABLE;

        foreach (var conn in _connections.Keys)
        {
            SteamNetworkingSockets.SendMessageToConnection(
                conn,
                unmanagedPointer,
                length,
                flags,
                out long _);
        }

        UnsafeUtility.Free((void*)unmanagedPointer, Allocator.Temp);
    }

    public void SendToAllExcept(CSteamID excludeId, byte[] data, bool reliable = false)
    {
        int flags = reliable ? RELIABLE : UNRELIABLE;

        // Allocate unmanaged memory and copy the byte[] into it
        IntPtr unmanagedPointer = Marshal.AllocHGlobal(data.Length);
        Marshal.Copy(data, 0, unmanagedPointer, data.Length);

        foreach (var pair in _connections)
        {
            if (pair.Value == excludeId)
                continue; // Skip this one

            SteamNetworkingSockets.SendMessageToConnection(
                pair.Key,                    // HSteamNetConnection
                unmanagedPointer,
                (uint)data.Length,
                flags,
                out long _);
        }

        Marshal.FreeHGlobal(unmanagedPointer);
    }

    public unsafe void SendToOneUnManagedUnsafe(CSteamID targetId, IntPtr unmanagedPointer, uint length, bool reliable = false)
    {
        int flags = reliable ? RELIABLE : UNRELIABLE;

        foreach (var pair in _connections)
        {
            if (pair.Value != targetId)
                continue;
            SteamNetworkingSockets.SendMessageToConnection(
                    pair.Key,
                    unmanagedPointer,
                    length,
                    flags,
                    out long _);
        }
        UnsafeUtility.Free((void*)unmanagedPointer, Allocator.Temp);
    }

    //Scene loaded initialization

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
            Debug.Log("Subscribing to lobby messages");
            GameOn = false;
            SteamP2PManager.Instance.OnDataReceived += HandleLobbyMessage;
            SteamLobbyManager.Instance.InitMainMenu();
        }
        if (scene.buildIndex == 2) // 2 is gameplay scene
        {
            Debug.Log($"[{(SteamP2PManager.Instance.isHost ? "HOST" : "CLIENT")}] Still in lobby: {SteamMatchmaking.GetLobbyOwner(SteamLobbyManager.CurrentLobbyID)}");
            Debug.Log($"[{(SteamP2PManager.Instance.isHost ? "HOST" : "CLIENT")}] Own Steam ID: {SteamUser.GetSteamID()}");
            if (isHost)
            {
                Debug.Log("Initializing Host Systems...");
                GameSystems.Instance.Init_Systems_Host();
            }
            else
            {
                Debug.Log("Initializing Client Systems...");
                GameSystems.Instance.Init_Systems_Client();
            }
        }
    }

    public void HandleLobbyMessage(CSteamID sender, byte[] data)
    {
        if (data == null || data.Length == 0) return;

        switch ((LobbyPacketType)data[0])
        {
            case LobbyPacketType.StartGame:
                Debug.Log("Client received StartGame command from host");
                //MainMenuUI.Instance.LobbyMenu.SetActive(false);
                OnDataReceived -= HandleLobbyMessage;
                SceneManager.LoadScene(2);
                break;
            case LobbyPacketType.ReadyFlag:
                if (SteamP2PManager.Instance.isHost)
                {
                    if (!SteamLobbyManager.Instance.clientReadyStates.ContainsKey(sender))
                        SteamLobbyManager.Instance.clientReadyStates[sender] = false;

                    SteamLobbyManager.Instance.clientReadyStates[sender] = !SteamLobbyManager.Instance.clientReadyStates[sender]; // toggle
                    Debug.Log($"[Host] {sender} toggled ready to {SteamLobbyManager.Instance.clientReadyStates[sender]}");
                }
                break;
        }
    }

    public unsafe void LaunchGameOnNewClient(CSteamID clientId)
    {
        byte* msg = (byte*)UnsafeUtility.Malloc(1, 4, Allocator.Temp);
        msg[0] = (byte)SteamP2PManager.LobbyPacketType.StartGame;
        SteamP2PManager.Instance.SendToOneUnManagedUnsafe(clientId, (IntPtr)msg, 1, reliable: true);
    }

    public void DisconnectAllClients()
    {
        Debug.Log("Cleaning up all P2P connections...");

        foreach (var conn in _connections.Keys)
        {
            SteamNetworkingSockets.CloseConnection(conn, 0, "Host shutdown", false);
        }

        _connections.Clear();

        if (_listenSocket != HSteamListenSocket.Invalid)
        {
            SteamNetworkingSockets.CloseListenSocket(_listenSocket);
            _listenSocket = HSteamListenSocket.Invalid;
        }

        Debug.Log("P2P shutdown complete.");
    }

    public void DisconnectFromHost()
    {
        foreach (var conn in _connections)
        {
            SteamNetworkingSockets.CloseConnection(conn.Key, 0, "Client left the lobby", false);
        }

        _connections.Clear();
        Debug.Log("Disconnected from host.");
    }
}
