using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public Button newGameButton;
    public Button loadGameButton;
    public Button settingsButton;
    public Button quitButton;
    
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject loadMenuPanel;
    public GameObject settingsPanel;
    
    void Start()
    {
        // Set up button listeners
        if (newGameButton != null)
            newGameButton.onClick.AddListener(StartNewGame);
            
        if (loadGameButton != null)
            loadGameButton.onClick.AddListener(ShowLoadMenu);
            
        if (settingsButton != null)
            settingsButton.onClick.AddListener(ShowSettings);
            
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
            
        // Show main menu by default
        ShowMainMenu();
    }
    
    public void StartNewGame()
    {
        Debug.Log("Starting new game...");
        
        if (SaveManager.Main != null)
        {
            SaveManager.Main.NewGame();
        }
        else
        {
            // Fallback if SaveManager not available
            SceneManager.LoadScene("VoxelWorld");
        }
    }
    
    public void ShowLoadMenu()
    {
        Debug.Log("Showing load menu...");
        HideAllPanels();
        if (loadMenuPanel != null)
            loadMenuPanel.SetActive(true);
    }
    
    public void ShowSettings()
    {
        Debug.Log("Showing settings...");
        HideAllPanels();
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }
    
    public void ShowMainMenu()
    {
        Debug.Log("Showing main menu...");
        HideAllPanels();
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }
    
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        
        if (SaveManager.Main != null)
        {
            SaveManager.Main.QuitGame();
        }
        else
        {
            // Fallback
            Application.Quit();
        }
    }
    
    void HideAllPanels()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
        if (loadMenuPanel != null)
            loadMenuPanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }
    
    // Method to be called from Back buttons in sub-menus
    public void GoBackToMainMenu()
    {
        ShowMainMenu();
    }
}