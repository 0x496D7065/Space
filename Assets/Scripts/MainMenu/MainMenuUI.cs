using System;
using CitrioN.SettingsMenuCreator;
using Mirror;
using Steamworks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static SteamP2PManager;

public class MainMenuUI : MonoBehaviour
{
    public static MainMenuUI Instance;

    public GameObject PlayMenu;
    public GameObject MainMenu;
    public GameObject LobbyMenu;

    public GameObject readyButton;
    public GameObject startButton;

    public SteamLobbyUIManager steamLobbyUIManager;

    private PlayerControls input;
    public Canvas settingUI;
    public PlayerInput playerInput;
    public SettingsMenu_UGUI settingsMenu;
    private bool isOpened = true;

    private void Awake()
    {
        input = new PlayerControls();
        input.UI.ToggleMenu.performed += ctx => HandleEscape();
    }

    private void OnEnable()
    {
        input?.UI.Enable();

    }
    private void OnDisable()
    {
        input?.UI.Disable();
    }



    private void HandleEscape()
    {
        if (settingUI.enabled == true)
        {
            // If in settings menu, back to pause menu
            CloseSettingUI();
            if (!LobbyMenu.activeSelf)
                ToggleMenu();
        }
        else if (PlayMenu.activeSelf)
        {
            ClosePlayMenu();
            ToggleMenu();
        }
        else if (LobbyMenu.activeSelf)
        {
            CloseLobbyMenu();
        }
    }

    public void ToggleMenu()
    {
        isOpened = !isOpened;
        MainMenu.SetActive(isOpened);
    }
    public void ShowSettingUI()
    {
        if (!LobbyMenu.activeSelf)
            ToggleMenu();
        settingUI.enabled = true;
    }

    public void CloseSettingUI()
    {
        settingUI.enabled = false;
        settingsMenu.SaveSettings();
    }
    public void ShowPlayMenu()
    {
        ToggleMenu();
        PlayMenu.SetActive(true);
    }
    public void ClosePlayMenu()
    {
        PlayMenu.SetActive(false);
    }
    public void ShowLobbyMenu()
    {
        ClosePlayMenu();
        LobbyMenu.SetActive(true);
    }
    public void CloseLobbyMenu()
    {
        SteamLobbyManager.Instance.LeaveLobby();
        LobbyMenu.SetActive(false);
        PlayMenu.SetActive(true);
    }
    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
    
    //Host/Join Buttons
    public void OnHostGameClicked()
    {
        SteamLobbyManager.Instance.HostLobby();
        ShowLobbyMenu();
    }

    public void OnJoinGameClicked()
    {
        SteamFriends.ActivateGameOverlay("friends");
    }

    public void OnStartClicked()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            LobbyMenu.SetActive(false);
            SteamworkManager networkManager = (SteamworkManager)NetworkManager.singleton;
            Debug.Log("Changing to scene: " + networkManager.gameplayScene);
            networkManager.ServerChangeScene(networkManager.gameplayScene);
        }
    }
}
