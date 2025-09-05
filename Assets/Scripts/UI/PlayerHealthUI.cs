using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Auto Setup Settings")]
    public bool setupUIOnStart = true;
    
    [Header("Health Display Settings")]
    public Color healthColor = Color.red;
    public Color backgroundColorFull = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    public Color backgroundColorLow = new Color(0.5f, 0.1f, 0.1f, 0.8f);
    
    // UI References
    private GameObject healthPanel;
    private Image healthBarFill;
    private Text healthText;
    private Canvas healthCanvas;
    
    // Health tracking
    private PlayerHealth playerHealth;
    private float lastKnownHealth = -1f;
    
    void Start()
    {
        if (setupUIOnStart)
        {
            SetupHealthUI();
        }
        
        // Find player health component
        FindPlayerHealth();
    }
    
    void Update()
    {
        UpdateHealthDisplay();
    }
    
    [ContextMenu("Setup Health UI")]
    public void SetupHealthUI()
    {
        Debug.Log("[PlayerHealthUI] Setting up health UI...");
        
        // Find or create canvas
        healthCanvas = FindHealthCanvas();
        if (healthCanvas == null)
        {
            Debug.LogError("[PlayerHealthUI] No Canvas found!");
            return;
        }
        
        CreateHealthPanel();
        
        Debug.Log("[PlayerHealthUI] ✅ Health UI setup complete!");
    }
    
    Canvas FindHealthCanvas()
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
        GameObject canvasObj = new GameObject("HealthCanvas");
        Canvas canvas2 = canvasObj.AddComponent<Canvas>();
        canvas2.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas2.sortingOrder = 50; // Lower than menu (100) but above game UI
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        Debug.Log("[PlayerHealthUI] Created new Health Canvas");
        return canvas2;
    }
    
    void CreateHealthPanel()
    {
        Debug.Log("[PlayerHealthUI] Creating health panel...");
        
        // Main health panel
        GameObject panelObj = new GameObject("HealthPanel");
        panelObj.transform.SetParent(healthCanvas.transform, false);
        
        healthPanel = panelObj;
        Image panelBg = panelObj.AddComponent<Image>();
        panelBg.color = backgroundColorFull;
        
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.02f, 0.85f);
        panelRect.anchorMax = new Vector2(0.35f, 0.95f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Health bar background
        GameObject healthBgObj = new GameObject("HealthBarBackground");
        healthBgObj.transform.SetParent(healthPanel.transform, false);
        
        Image healthBg = healthBgObj.AddComponent<Image>();
        healthBg.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        
        RectTransform healthBgRect = healthBgObj.GetComponent<RectTransform>();
        healthBgRect.anchorMin = new Vector2(0.05f, 0.3f);
        healthBgRect.anchorMax = new Vector2(0.75f, 0.7f);
        healthBgRect.offsetMin = Vector2.zero;
        healthBgRect.offsetMax = Vector2.zero;
        
        // Health bar fill
        GameObject healthFillObj = new GameObject("HealthBarFill");
        healthFillObj.transform.SetParent(healthBgObj.transform, false);
        
        healthBarFill = healthFillObj.AddComponent<Image>();
        healthBarFill.color = healthColor;
        healthBarFill.type = Image.Type.Filled;
        healthBarFill.fillMethod = Image.FillMethod.Horizontal;
        
        RectTransform healthFillRect = healthFillObj.GetComponent<RectTransform>();
        healthFillRect.anchorMin = Vector2.zero;
        healthFillRect.anchorMax = Vector2.one;
        healthFillRect.offsetMin = Vector2.zero;
        healthFillRect.offsetMax = Vector2.zero;
        
        // Health text
        GameObject textObj = new GameObject("HealthText");
        textObj.transform.SetParent(healthPanel.transform, false);
        
        // Ensure RectTransform exists
        if (textObj.GetComponent<RectTransform>() == null)
            textObj.AddComponent<RectTransform>();
        
        healthText = textObj.AddComponent<Text>();
        healthText.text = "Health: 100/100";
        healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        healthText.fontSize = 16;
        healthText.fontStyle = FontStyle.Bold;
        healthText.color = Color.white;
        healthText.alignment = TextAnchor.MiddleLeft;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.8f, 0.1f);
        textRect.anchorMax = new Vector2(1f, 0.9f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        Debug.Log("[PlayerHealthUI] Health panel created");
    }
    
    void FindPlayerHealth()
    {
        if (playerHealth == null)
        {
            playerHealth = PlayerHealth.Instance;
            
            if (playerHealth == null)
            {
                // Try to find by component
                playerHealth = FindObjectOfType<PlayerHealth>();
            }
            
            if (playerHealth != null)
            {
                Debug.Log("[PlayerHealthUI] Found PlayerHealth component");
            }
            else
            {
                Debug.LogWarning("[PlayerHealthUI] PlayerHealth component not found!");
            }
        }
    }
    
    void UpdateHealthDisplay()
    {
        if (playerHealth == null || healthBarFill == null || healthText == null)
            return;
        
        float currentHealth = playerHealth.GetCurrentHealth();
        float maxHealth = playerHealth.GetMaxHealth();
        
        // Only update if health changed
        if (Mathf.Abs(currentHealth - lastKnownHealth) > 0.1f)
        {
            lastKnownHealth = currentHealth;
            
            // Update health bar fill
            float healthPercentage = maxHealth > 0 ? currentHealth / maxHealth : 0f;
            healthBarFill.fillAmount = healthPercentage;
            
            // Update health text
            healthText.text = $"{currentHealth:F0}/{maxHealth:F0}";
            
            // Change colors based on health
            if (healthPercentage > 0.6f)
            {
                healthBarFill.color = Color.green;
                if (healthPanel != null) healthPanel.GetComponent<Image>().color = backgroundColorFull;
            }
            else if (healthPercentage > 0.3f)
            {
                healthBarFill.color = Color.yellow;
                if (healthPanel != null) healthPanel.GetComponent<Image>().color = backgroundColorFull;
            }
            else
            {
                healthBarFill.color = Color.red;
                if (healthPanel != null) healthPanel.GetComponent<Image>().color = backgroundColorLow;
            }
            
            // Flash effect when health is very low
            if (healthPercentage < 0.2f && Time.time % 1f < 0.5f)
            {
                healthBarFill.color = Color.white;
            }
        }
    }
    
    // Public method to show/hide health UI
    public void SetHealthUIVisible(bool visible)
    {
        if (healthPanel != null)
        {
            healthPanel.SetActive(visible);
        }
    }
}