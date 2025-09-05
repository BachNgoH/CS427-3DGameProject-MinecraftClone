using UnityEngine;
using UnityEngine.UI;

public class AutoMainMenuUI : MonoBehaviour
{
    [Header("Auto Setup Settings")]
    public bool setupUIOnStart = true;
    
    // UI References that will be auto-created
    private GameObject mainMenuPanel;
    private GameObject loadMenuPanel;
    private GameObject settingsPanel;
    private Canvas menuCanvas;
    private MainMenuManager menuManager;
    
    void Start()
    {
        if (setupUIOnStart)
        {
            SetupMainMenuUI();
        }
    }
    
    [ContextMenu("Setup Main Menu UI")]
    public void SetupMainMenuUI()
    {
        Debug.Log("[AutoMainMenuUI] Setting up main menu UI...");
        
        // Find or create canvas
        menuCanvas = FindMenuCanvas();
        if (menuCanvas == null)
        {
            Debug.LogError("[AutoMainMenuUI] No Canvas found!");
            return;
        }
        
        // Add MainMenuManager component if not exists
        if (GetComponent<MainMenuManager>() == null)
        {
            menuManager = gameObject.AddComponent<MainMenuManager>();
        }
        else
        {
            menuManager = GetComponent<MainMenuManager>();
        }
        
        CreateMainMenuPanel();
        CreateLoadMenuPanel();
        CreateSettingsPanel();
        
        // Assign references to MainMenuManager
        AssignMenuReferences();
        
        Debug.Log("[AutoMainMenuUI] ✅ Main menu UI setup complete!");
    }
    
    Canvas FindMenuCanvas()
    {
        // Try to find existing canvas
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return canvas;
            }
        }
        
        // Create new canvas if none found
        GameObject canvasObj = new GameObject("MainMenuCanvas");
        Canvas canvas2 = canvasObj.AddComponent<Canvas>();
        canvas2.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas2.sortingOrder = 100;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        Debug.Log("[AutoMainMenuUI] Created new Canvas");
        return canvas2;
    }
    
    void CreateMainMenuPanel()
    {
        Debug.Log("[AutoMainMenuUI] Creating main menu panel...");
        
        // Main panel
        GameObject panelObj = new GameObject("MainMenuPanel");
        panelObj.transform.SetParent(menuCanvas.transform, false);
        
        mainMenuPanel = panelObj;
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.2f, 0.8f); // Dark blue semi-transparent
        
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.3f, 0.2f);
        panelRect.anchorMax = new Vector2(0.7f, 0.8f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Title
        CreateTitle(mainMenuPanel, "MINECRAFT 4 UNITY");
        
        // Buttons container
        GameObject buttonsContainer = new GameObject("ButtonsContainer");
        buttonsContainer.transform.SetParent(mainMenuPanel.transform, false);
        
        RectTransform buttonsRect = buttonsContainer.AddComponent<RectTransform>();
        buttonsRect.anchorMin = new Vector2(0.1f, 0.3f);
        buttonsRect.anchorMax = new Vector2(0.9f, 0.8f);
        buttonsRect.offsetMin = Vector2.zero;
        buttonsRect.offsetMax = Vector2.zero;
        
        // Add vertical layout
        VerticalLayoutGroup layoutGroup = buttonsContainer.AddComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 20;
        layoutGroup.padding = new RectOffset(20, 20, 20, 20);
        layoutGroup.childControlHeight = false;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = true;
        
        // Create menu buttons
        CreateMenuButton(buttonsContainer, "NEW GAME", Color.green);
        CreateMenuButton(buttonsContainer, "LOAD GAME", Color.blue);
        CreateMenuButton(buttonsContainer, "SETTINGS", Color.yellow);
        CreateMenuButton(buttonsContainer, "QUIT", Color.red);
        
        Debug.Log("[AutoMainMenuUI] Main menu panel created");
    }
    
    void CreateLoadMenuPanel()
    {
        Debug.Log("[AutoMainMenuUI] Creating load menu panel...");
        
        GameObject panelObj = new GameObject("LoadMenuPanel");
        panelObj.transform.SetParent(menuCanvas.transform, false);
        
        loadMenuPanel = panelObj;
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.2f, 0.1f, 0.8f); // Dark green semi-transparent
        
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.2f, 0.1f);
        panelRect.anchorMax = new Vector2(0.8f, 0.9f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        CreateTitle(loadMenuPanel, "LOAD GAME");
        CreateBackButton(loadMenuPanel);
        
        // Add LoadMenu component if it doesn't exist
        if (loadMenuPanel.GetComponent<LoadMenu>() == null)
        {
            loadMenuPanel.AddComponent<LoadMenu>();
        }
        
        // Start hidden
        loadMenuPanel.SetActive(false);
        
        Debug.Log("[AutoMainMenuUI] Load menu panel created");
    }
    
    void CreateSettingsPanel()
    {
        Debug.Log("[AutoMainMenuUI] Creating settings panel...");
        
        GameObject panelObj = new GameObject("SettingsPanel");
        panelObj.transform.SetParent(menuCanvas.transform, false);
        
        settingsPanel = panelObj;
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.2f, 0.1f, 0.1f, 0.8f); // Dark red semi-transparent
        
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.25f, 0.2f);
        panelRect.anchorMax = new Vector2(0.75f, 0.8f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        CreateTitle(settingsPanel, "SETTINGS");
        CreateBackButton(settingsPanel);
        
        // Add placeholder settings text
        CreateSettingsContent(settingsPanel);
        
        // Start hidden
        settingsPanel.SetActive(false);
        
        Debug.Log("[AutoMainMenuUI] Settings panel created");
    }
    
    void CreateTitle(GameObject parent, string titleText)
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(parent.transform, false);
        
        // Ensure RectTransform exists
        if (titleObj.GetComponent<RectTransform>() == null)
            titleObj.AddComponent<RectTransform>();
        
        Text title = titleObj.AddComponent<Text>();
        title.text = titleText;
        title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        title.fontSize = 32;
        title.fontStyle = FontStyle.Bold;
        title.color = Color.white;
        title.alignment = TextAnchor.MiddleCenter;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.85f);
        titleRect.anchorMax = new Vector2(1, 1f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
    }
    
    Button CreateMenuButton(GameObject parent, string buttonText, Color buttonColor)
    {
        GameObject buttonObj = new GameObject($"Button_{buttonText.Replace(" ", "")}");
        buttonObj.transform.SetParent(parent.transform, false);
        
        Image buttonBg = buttonObj.AddComponent<Image>();
        buttonBg.color = buttonColor;
        
        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = buttonBg;
        
        // Set size
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(0, 60); // Auto width, 60 height
        
        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        // Ensure RectTransform exists
        if (textObj.GetComponent<RectTransform>() == null)
            textObj.AddComponent<RectTransform>();
        
        Text text = textObj.AddComponent<Text>();
        text.text = buttonText;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 20;
        text.fontStyle = FontStyle.Bold;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        return button;
    }
    
    void CreateBackButton(GameObject parent)
    {
        GameObject buttonObj = new GameObject("BackButton");
        buttonObj.transform.SetParent(parent.transform, false);
        
        Image buttonBg = buttonObj.AddComponent<Image>();
        buttonBg.color = Color.gray;
        
        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = buttonBg;
        
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.05f, 0.05f);
        buttonRect.anchorMax = new Vector2(0.25f, 0.15f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        
        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        // Ensure RectTransform exists
        if (textObj.GetComponent<RectTransform>() == null)
            textObj.AddComponent<RectTransform>();
        
        Text text = textObj.AddComponent<Text>();
        text.text = "BACK";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 16;
        text.fontStyle = FontStyle.Bold;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Add back functionality
        button.onClick.AddListener(() => {
            if (menuManager != null)
                menuManager.GoBackToMainMenu();
        });
    }
    
    void CreateSettingsContent(GameObject parent)
    {
        GameObject contentObj = new GameObject("SettingsContent");
        contentObj.transform.SetParent(parent.transform, false);
        
        // Ensure RectTransform exists
        if (contentObj.GetComponent<RectTransform>() == null)
            contentObj.AddComponent<RectTransform>();
        
        Text content = contentObj.AddComponent<Text>();
        content.text = "Settings panel - coming soon!\n\nYou can add graphics, audio,\nand control settings here.";
        content.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        content.fontSize = 18;
        content.color = Color.white;
        content.alignment = TextAnchor.MiddleCenter;
        
        RectTransform contentRect = contentObj.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.1f, 0.3f);
        contentRect.anchorMax = new Vector2(0.9f, 0.7f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;
    }
    
    void AssignMenuReferences()
    {
        if (menuManager == null) return;
        
        // Find and assign button references
        Button newGameBtn = mainMenuPanel.transform.Find("ButtonsContainer/Button_NEWGAME")?.GetComponent<Button>();
        Button loadGameBtn = mainMenuPanel.transform.Find("ButtonsContainer/Button_LOADGAME")?.GetComponent<Button>();
        Button settingsBtn = mainMenuPanel.transform.Find("ButtonsContainer/Button_SETTINGS")?.GetComponent<Button>();
        Button quitBtn = mainMenuPanel.transform.Find("ButtonsContainer/Button_QUIT")?.GetComponent<Button>();
        
        // Use reflection to set private fields (since they're public in MainMenuManager)
        var managerType = typeof(MainMenuManager);
        
        managerType.GetField("newGameButton")?.SetValue(menuManager, newGameBtn);
        managerType.GetField("loadGameButton")?.SetValue(menuManager, loadGameBtn);
        managerType.GetField("settingsButton")?.SetValue(menuManager, settingsBtn);
        managerType.GetField("quitButton")?.SetValue(menuManager, quitBtn);
        
        managerType.GetField("mainMenuPanel")?.SetValue(menuManager, mainMenuPanel);
        managerType.GetField("loadMenuPanel")?.SetValue(menuManager, loadMenuPanel);
        managerType.GetField("settingsPanel")?.SetValue(menuManager, settingsPanel);
        
        Debug.Log("[AutoMainMenuUI] Menu references assigned to MainMenuManager");
    }
}