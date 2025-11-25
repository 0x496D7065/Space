using CitrioN.SettingsMenuCreator;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
//using UnityEngine.Windows;

public class UIManager : MonoBehaviour
{
    [Header("Reference")]
    public GameObject escapeMenu;
    private InputSystem_Actions input;
    public Canvas settingUI;
    public PlayerInput playerInput;
    public SettingsMenu_UGUI settingsMenu;
    public TextMeshProUGUI timerText;
    private bool isOpened = false;

    private void Awake()
    {
        input = new InputSystem_Actions();
        input.UI.Menu.performed += ctx => HandleEscape();
    }
    private void OnEnable() => input.UI.Enable();
    private void OnDisable() => input.UI.Disable();

    void Update()
    {
        if (GameManager.Instance != null)
        {
            float timeLeft = GameManager.Instance.GetTimeRemaining();
            UpdateTimerUI(timeLeft);
        }
    }

    private void UpdateTimerUI(float timeLeft)
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(timeLeft / 60f);
        int seconds = Mathf.FloorToInt(timeLeft % 60f);

        timerText.text = $"{minutes}:{seconds:00}";
    }

    private void HandleEscape()
    {
        if (settingUI.enabled == true)
        {
            // If in settings menu, back to pause menu
            CloseSettingUI();
            ToggleMenu();
        }
        else if (escapeMenu.activeSelf)
        {
            // If in pause menu, close menu entirely
            ToggleMenu();
            LockCursor();
        }
        else
        {
            // No menu open → open pause menu
            ToggleMenu();
            UnlockCursor();
        }
    }

    public void ToggleMenu()
    {
        isOpened = !isOpened;
        escapeMenu.SetActive(isOpened);
    }
    public void CloseSettingUI()
    {
        settingUI.enabled = false;
        settingsMenu.SaveSettings();
    }
    public void ShowSettingUI()
    {
        ToggleMenu();
        settingUI.enabled = true;
    }

    public void BackToMenu()
    {
        Debug.Log("Back to main menu");
        SteamLobbyManager.Instance.LeaveLobby();
        StopAllCoroutines();
        SceneManager.LoadScene(1);//MainMenu scene
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerInput.SwitchCurrentActionMap("Gameplay");
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        playerInput.SwitchCurrentActionMap("UI");
    }
}
