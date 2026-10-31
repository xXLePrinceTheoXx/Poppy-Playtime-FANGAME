using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject pausePanel;
    public GameObject optionsPanel;
    public GameObject inventoryPanel;

    [Header("Sous-scriptes")]
    public OptionsMenu optionsMenu;
    public InventoryManager inventoryManager;

    private bool isPaused = false;
    private bool wasPausedBeforeInventory = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        CloseAll();
        // Ouvre le menu principal au lancement (optionnel, commente si tu veux démarrer directement en jeu)
        // OpenMainMenu();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (inventoryPanel != null && inventoryPanel.activeSelf)
                CloseInventory();
            else if (optionsPanel != null && optionsPanel.activeSelf)
                CloseOptions();
            else if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void OpenMainMenu()
    {
        CloseAll();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void PauseGame()
    {
        CloseAll();
        if (pausePanel != null) pausePanel.SetActive(true);
        isPaused = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        CloseAll();
        isPaused = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OpenOptions()
    {
        CloseAll();
        if (optionsPanel != null) optionsPanel.SetActive(true);
        if (optionsMenu != null) optionsMenu.RefreshUI();
    }

    public void CloseOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (isPaused && pausePanel != null) pausePanel.SetActive(true);
        else if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void OpenInventory()
    {
        if (isPaused && pausePanel != null)
        {
            wasPausedBeforeInventory = true;
            pausePanel.SetActive(false);
        }
        CloseAllExceptInventory();
        if (inventoryPanel != null) inventoryPanel.SetActive(true);
        if (inventoryManager != null) inventoryManager.RefreshUI();
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseInventory()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (wasPausedBeforeInventory)
        {
            if (pausePanel != null) pausePanel.SetActive(true);
            wasPausedBeforeInventory = false;
        }
        else
        {
            ResumeGame();
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void CloseAll()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
    }

    void CloseAllExceptInventory()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    public bool IsPaused => isPaused;
    public bool IsInventoryOpen => inventoryPanel != null && inventoryPanel.activeSelf;
}