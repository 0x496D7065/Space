using System.Collections.Generic;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SteamLobbyUIManager : MonoBehaviour
{
    public GameObject lobbyMenu;
    public Transform playerListContainer;
    public GameObject playerEntryPrefab;
    public MainMenuUI mainMenu;

    private Callback<LobbyChatUpdate_t> _onLobbyChatUpdate;

    private void Start()
    {
        InitMainMenu();
    }

    public void InitMainMenu()
    {
        mainMenu = FindAnyObjectByType<MainMenuUI>();
    }

    void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
    {
        CSteamID lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        CSteamID userChanged = new CSteamID(callback.m_ulSteamIDUserChanged);
        EChatMemberStateChange change = (EChatMemberStateChange)callback.m_rgfChatMemberStateChange;

        if (lobbyId != SteamLobbyManager.CurrentLobbyID)
            return;

        if (change.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeEntered))
        {
            Debug.Log($"User {userChanged} has joined the lobby.");
            if (!SteamLobbyManager.Instance.clientReadyStates.ContainsKey(userChanged))
                SteamLobbyManager.Instance.clientReadyStates.Add(userChanged, false);
            SteamLobbyManager.Instance.clientReadyStates[userChanged] = false;
            RefreshPlayerList();
        }

        if (change.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeLeft) ||
            change.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeDisconnected) ||
            change.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeKicked) ||
            change.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeBanned))
        {
            Debug.Log($"Player {userChanged} left the lobby");

            // If host left, everyone should leave
            if (SteamMatchmaking.GetNumLobbyMembers(lobbyId) == 0 || userChanged == new CSteamID(ulong.Parse(SteamMatchmaking.GetLobbyData(lobbyId, "hostid"))))
            {
                Debug.LogWarning("Host has left. Closing lobby.");
                if (mainMenu != null)
                    mainMenu.CloseLobbyMenu();
            }
            else
            {
                SteamP2PManager.Instance.clientLobbyJoinTimes.Remove(userChanged);
                RefreshPlayerList();
            }
        }
    }

    public void RefreshPlayerList()
    {
        Debug.Log("Refreshing PlayerList");
        foreach (Transform child in playerListContainer)
            Destroy(child.gameObject);

        int count = SteamMatchmaking.GetNumLobbyMembers(SteamLobbyManager.CurrentLobbyID);
        Debug.Log($"Number of player found in lobby {count}");
        for (int i = 0; i < count; i++)
        {
            CSteamID memberId = SteamMatchmaking.GetLobbyMemberByIndex(SteamLobbyManager.CurrentLobbyID, i);
            CreatePlayerEntry(memberId);
        }
    }

    void CreatePlayerEntry(CSteamID steamID)
    {
        Debug.Log($"Creating entry for {steamID}");
        GameObject entry = Instantiate(playerEntryPrefab, playerListContainer);
        TextMeshProUGUI nameText = entry.transform.Find("Name_Display").GetComponent<TextMeshProUGUI>();
        RawImage avatarImage = entry.transform.Find("Avatar").GetComponent<RawImage>();

        string playerName = SteamFriends.GetFriendPersonaName(steamID);
        string hostIdStr = SteamMatchmaking.GetLobbyData(SteamLobbyManager.CurrentLobbyID, "hostid");
        if (ulong.TryParse(hostIdStr, out ulong hostId) && steamID.m_SteamID == hostId)
        {
            playerName = "(Host) " + playerName;
        }
        nameText.text = playerName;
        StartCoroutine(LoadAvatar(steamID, avatarImage));
    }

    System.Collections.IEnumerator LoadAvatar(CSteamID steamID, RawImage targetImage)
    {
        Debug.Log($"Loading Avatar");
        int avatarInt = SteamFriends.GetLargeFriendAvatar(steamID);
        if (avatarInt == -1) yield break;

        uint width = 0, height = 0;
        while (!SteamUtils.GetImageSize(avatarInt, out width, out height) || width == 0 || height == 0)
            yield return null;
        Debug.Log($"GetImageSize done");
        byte[] image = new byte[width * height * 4];
        SteamUtils.GetImageRGBA(avatarInt, image, (int)(width * height * 4));

        Texture2D avatarTexture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
        avatarTexture.LoadRawTextureData(image);
        avatarTexture.Apply();

        // Flip vertically
        Texture2D flipped = new Texture2D(avatarTexture.width, avatarTexture.height, avatarTexture.format, false);
        for (int y = 0; y < avatarTexture.height; y++)
        {
            flipped.SetPixels(0, y, avatarTexture.width, 1, avatarTexture.GetPixels(0, avatarTexture.height - y - 1, avatarTexture.width, 1));
        }
        flipped.Apply();

        targetImage.texture = flipped;
        Debug.Log($"Avatar applied");
    }
    public void ClearPlayerList()
    {
        foreach (Transform child in playerListContainer)
        {
            Destroy(child.gameObject);
        }
    }
    void OnEnable()
    {
        _onLobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
    }

    void OnDisable()
    {
        _onLobbyChatUpdate?.Unregister();
    }
}
