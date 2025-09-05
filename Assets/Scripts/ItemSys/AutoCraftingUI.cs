using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AutoCraftingUI : MonoBehaviour
{
    [Header("Settings")]
    public KeyCode craftingKey = KeyCode.C;
    public bool setupUIOnStart = true;
    
    // UI References (will be auto-created)
    private GameObject craftingPanel;
    private Transform recipeContainer;
    private Text titleText;
    private Text inventoryText;
    private Button closeButton;
    private ScrollRect scrollRect;
    
    private bool isOpen = false;
    private Canvas gameCanvas;
    private float lastRefreshTime = 0f;
    
    void Start()
    {
        // Ensure CraftingSystem exists
        if (CraftingSystem.Main == null)
        {
            GameObject craftingSystemObj = new GameObject("CraftingSystem");
            craftingSystemObj.AddComponent<CraftingSystem>();
            Debug.Log("[AutoCraftingUI] Created CraftingSystem singleton");
        }
        
        if (setupUIOnStart)
        {
            SetupCraftingUI();
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(craftingKey))
        {
            ToggleCrafting();
        }
        
        // Auto-retry initialization if inventory system becomes available
        if (craftingPanel != null && isOpen && ContainerManager.Main?.playerInventory?.SlotIDs != null)
        {
            if (Time.time - lastRefreshTime > 1f) // Refresh every second when open
            {
                RefreshCraftingMenu();
                lastRefreshTime = Time.time;
            }
        }
    }
    
    [ContextMenu("Setup Crafting UI")]
    public void SetupCraftingUI()
    {
        Debug.Log("[AutoCraftingUI] Setting up crafting UI...");
        
        // Find or create canvas
        gameCanvas = FindCanvas();
        if (gameCanvas == null)
        {
            Debug.LogError("[AutoCraftingUI] No Canvas found!");
            return;
        }
        
        CreateCraftingPanel();
        Debug.Log("[AutoCraftingUI] ✅ Crafting UI setup complete!");
    }
    
    Canvas FindCanvas()
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
        GameObject canvasObj = new GameObject("CraftingCanvas");
        Canvas canvas2 = canvasObj.AddComponent<Canvas>();
        canvas2.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas2.sortingOrder = 100; // Make sure it's on top
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        Debug.Log("[AutoCraftingUI] Created new Canvas");
        return canvas2;
    }
    
    void CreateCraftingPanel()
    {
        Debug.Log("[AutoCraftingUI] Creating main crafting panel...");
        
        // Main panel
        GameObject panelObj = new GameObject("CraftingPanel");
        panelObj.transform.SetParent(gameCanvas.transform, false);
        
        craftingPanel = panelObj;
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f); // Dark semi-transparent
        
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.2f, 0.1f);
        panelRect.anchorMax = new Vector2(0.8f, 0.9f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        Debug.Log("[AutoCraftingUI] Panel created, adding child elements...");
        
        // Title
        CreateTitle();
        Debug.Log("[AutoCraftingUI] Title created");
        
        // Close Button
        CreateCloseButton();
        Debug.Log("[AutoCraftingUI] Close button created");
        
        // Inventory Display
        CreateInventoryDisplay();
        Debug.Log("[AutoCraftingUI] Inventory display created");
        
        // Recipe Container with Scroll
        CreateRecipeContainer();
        Debug.Log("[AutoCraftingUI] Recipe container created");
        
        // Start hidden
        craftingPanel.SetActive(false);
        Debug.Log("[AutoCraftingUI] Panel creation complete, starting hidden");
    }
    
    void CreateTitle()
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(craftingPanel.transform, false);
        
        // Ensure RectTransform exists for UI element
        if (titleObj.GetComponent<RectTransform>() == null)
            titleObj.AddComponent<RectTransform>();
        
        titleText = titleObj.AddComponent<Text>();
        titleText.text = "CRAFTING MENU";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 24;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.9f);
        titleRect.anchorMax = new Vector2(1, 1f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
    }
    
    void CreateCloseButton()
    {
        GameObject buttonObj = new GameObject("CloseButton");
        buttonObj.transform.SetParent(craftingPanel.transform, false);
        
        closeButton = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = Color.red;
        
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.9f, 0.9f);
        buttonRect.anchorMax = new Vector2(1f, 1f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        
        // X text on button
        GameObject xTextObj = new GameObject("XText");
        xTextObj.transform.SetParent(buttonObj.transform, false);
        
        // Ensure RectTransform exists for UI element
        if (xTextObj.GetComponent<RectTransform>() == null)
            xTextObj.AddComponent<RectTransform>();
        
        Text xText = xTextObj.AddComponent<Text>();
        xText.text = "X";
        xText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        xText.fontSize = 18;
        xText.fontStyle = FontStyle.Bold;
        xText.color = Color.white;
        xText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform xRect = xTextObj.GetComponent<RectTransform>();
        xRect.anchorMin = Vector2.zero;
        xRect.anchorMax = Vector2.one;
        xRect.offsetMin = Vector2.zero;
        xRect.offsetMax = Vector2.zero;
        
        closeButton.onClick.AddListener(() => ToggleCrafting());
    }
    
    void CreateInventoryDisplay()
    {
        GameObject invObj = new GameObject("InventoryDisplay");
        invObj.transform.SetParent(craftingPanel.transform, false);
        
        // Background
        Image invBg = invObj.AddComponent<Image>();
        invBg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        RectTransform invRect = invObj.GetComponent<RectTransform>();
        invRect.anchorMin = new Vector2(0.05f, 0.7f);
        invRect.anchorMax = new Vector2(0.95f, 0.85f);
        invRect.offsetMin = Vector2.zero;
        invRect.offsetMax = Vector2.zero;
        
        // Text
        GameObject invTextObj = new GameObject("InventoryText");
        invTextObj.transform.SetParent(invObj.transform, false);
        
        // Ensure RectTransform exists for UI element
        if (invTextObj.GetComponent<RectTransform>() == null)
            invTextObj.AddComponent<RectTransform>();
        
        inventoryText = invTextObj.AddComponent<Text>();
        inventoryText.text = "Your Items: Loading...";
        inventoryText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        inventoryText.fontSize = 14;
        inventoryText.color = Color.white;
        inventoryText.alignment = TextAnchor.UpperLeft;
        
        RectTransform invTextRect = invTextObj.GetComponent<RectTransform>();
        invTextRect.anchorMin = Vector2.zero;
        invTextRect.anchorMax = Vector2.one;
        invTextRect.offsetMin = new Vector2(10, 5);
        invTextRect.offsetMax = new Vector2(-10, -5);
    }
    
    void CreateRecipeContainer()
    {
        // Scroll View
        GameObject scrollObj = new GameObject("RecipeScrollView");
        scrollObj.transform.SetParent(craftingPanel.transform, false);
        
        Image scrollBg = scrollObj.AddComponent<Image>();
        scrollBg.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
        
        scrollRect = scrollObj.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        
        RectTransform scrollRectTrans = scrollObj.GetComponent<RectTransform>();
        scrollRectTrans.anchorMin = new Vector2(0.05f, 0.1f);
        scrollRectTrans.anchorMax = new Vector2(0.95f, 0.65f);
        scrollRectTrans.offsetMin = Vector2.zero;
        scrollRectTrans.offsetMax = Vector2.zero;
        
        // Content container
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(scrollObj.transform, false);
        
        // Add RectTransform component for UI
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        recipeContainer = contentObj.transform;
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;
        
        // Add VerticalLayoutGroup for auto-sizing
        VerticalLayoutGroup layoutGroup = contentObj.AddComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 10;
        layoutGroup.padding = new RectOffset(10, 10, 10, 10);
        layoutGroup.childControlHeight = false;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = true;
        
        // Add ContentSizeFitter
        ContentSizeFitter sizeFitter = contentObj.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        scrollRect.content = contentRect;
    }
    
    void ToggleCrafting()
    {
        if (craftingPanel == null)
        {
            Debug.Log("[AutoCraftingUI] Setting up UI...");
            SetupCraftingUI();
        }
        
        if (craftingPanel == null)
        {
            Debug.LogError("[AutoCraftingUI] Failed to setup crafting panel!");
            return;
        }
        
        isOpen = !isOpen;
        craftingPanel.SetActive(isOpen);
        
        if (isOpen)
        {
            RefreshCraftingMenu();
            // Show cursor and unlock it
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // Hide cursor and lock it
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    void RefreshCraftingMenu()
    {
        Debug.Log("[AutoCraftingUI] Refreshing crafting menu...");
        
        // Check if inventory system is ready
        if (ContainerManager.Main?.playerInventory?.SlotIDs == null)
        {
            Debug.LogWarning("[AutoCraftingUI] Inventory system not ready - skipping refresh");
            return;
        }
        
        UpdateInventoryDisplay();
        UpdateRecipeButtons();
        Debug.Log("[AutoCraftingUI] Menu refresh complete");
    }
    
    void UpdateInventoryDisplay()
    {
        Debug.Log("[AutoCraftingUI] Updating inventory display...");
        Dictionary<string, int> playerItems = GetPlayerItems();
        
        Debug.Log($"[AutoCraftingUI] Found {playerItems.Count} item types in inventory");
        
        string inventoryDisplay = "Your Items:\n";
        foreach (var item in playerItems)
        {
            inventoryDisplay += $"{item.Key}: {item.Value}  ";
        }
        
        if (inventoryText != null)
        {
            inventoryText.text = inventoryDisplay;
            Debug.Log($"[AutoCraftingUI] Updated inventory text: {inventoryDisplay}");
        }
        else
        {
            Debug.LogError("[AutoCraftingUI] inventoryText is null!");
        }
    }
    
    void UpdateRecipeButtons()
    {
        Debug.Log("[AutoCraftingUI] Updating recipe buttons...");
        
        // Clear existing buttons
        if (recipeContainer != null)
        {
            int childCount = recipeContainer.childCount;
            Debug.Log($"[AutoCraftingUI] Clearing {childCount} existing children from recipe container");
            foreach (Transform child in recipeContainer)
            {
                Destroy(child.gameObject);
            }
        }
        else
        {
            Debug.LogError("[AutoCraftingUI] recipeContainer is null!");
            return;
        }
        
        // Check if CraftingSystem exists
        if (CraftingSystem.Main == null)
        {
            Debug.LogWarning("[AutoCraftingUI] CraftingSystem.Main is null! Creating it...");
            GameObject craftingSystemObj = new GameObject("CraftingSystem");
            craftingSystemObj.AddComponent<CraftingSystem>();
        }
        
        // Double check after creation
        if (CraftingSystem.Main == null || CraftingSystem.Main.recipes == null)
        {
            Debug.LogError("[AutoCraftingUI] CraftingSystem or recipes is still null!");
            CreateErrorMessage("CraftingSystem not found!");
            return;
        }
        
        Debug.Log($"[AutoCraftingUI] Found CraftingSystem with {CraftingSystem.Main.recipes.Count} recipes");
        
        Dictionary<string, int> playerItems = GetPlayerItems();
        var availableRecipes = CraftingSystem.Main.GetAvailableRecipes(playerItems);
        
        Debug.Log($"[AutoCraftingUI] Player can craft {availableRecipes.Count} recipes");
        
        // Create buttons for each recipe
        int buttonCount = 0;
        foreach (var recipe in CraftingSystem.Main.recipes)
        {
            bool canCraft = availableRecipes.Contains(recipe);
            Debug.Log($"[AutoCraftingUI] Creating button for {recipe.recipeName} (canCraft: {canCraft})");
            CreateRecipeButton(recipe, canCraft, playerItems);
            buttonCount++;
        }
        
        Debug.Log($"[AutoCraftingUI] Created {buttonCount} recipe buttons");
        
        // Add a test button to ensure something is visible
        CreateTestButton();
    }
    
    void CreateTestButton()
    {
        Debug.Log("[AutoCraftingUI] Creating test button to ensure visibility...");
        
        GameObject testButtonObj = new GameObject("TestButton");
        testButtonObj.transform.SetParent(recipeContainer, false);
        
        Image testBg = testButtonObj.AddComponent<Image>();
        testBg.color = new Color(1f, 0.5f, 0f, 0.8f); // Orange
        
        Button testButton = testButtonObj.AddComponent<Button>();
        testButton.targetGraphic = testBg;
        
        RectTransform testRect = testButtonObj.GetComponent<RectTransform>();
        testRect.sizeDelta = new Vector2(0, 60);
        
        GameObject testTextObj = new GameObject("TestText");
        testTextObj.transform.SetParent(testButtonObj.transform, false);
        
        // Ensure RectTransform exists for UI element
        if (testTextObj.GetComponent<RectTransform>() == null)
            testTextObj.AddComponent<RectTransform>();
        
        Text testText = testTextObj.AddComponent<Text>();
        testText.text = "TEST BUTTON - UI IS WORKING";
        testText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        testText.fontSize = 18;
        testText.color = Color.white;
        testText.alignment = TextAnchor.MiddleCenter;
        testText.fontStyle = FontStyle.Bold;
        
        RectTransform testTextRect = testTextObj.GetComponent<RectTransform>();
        testTextRect.anchorMin = Vector2.zero;
        testTextRect.anchorMax = Vector2.one;
        testTextRect.offsetMin = Vector2.zero;
        testTextRect.offsetMax = Vector2.zero;
        
        testButton.onClick.AddListener(() => Debug.Log("TEST BUTTON CLICKED!"));
        
        Debug.Log("[AutoCraftingUI] Test button created");
    }
    
    void CreateErrorMessage(string message)
    {
        if (recipeContainer == null) return;
        
        GameObject errorObj = new GameObject("ErrorMessage");
        errorObj.transform.SetParent(recipeContainer, false);
        
        Image errorBg = errorObj.AddComponent<Image>();
        errorBg.color = new Color(0.8f, 0.2f, 0.2f, 0.8f);
        
        RectTransform errorRect = errorObj.GetComponent<RectTransform>();
        errorRect.sizeDelta = new Vector2(0, 60);
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(errorObj.transform, false);
        
        // Ensure RectTransform exists for UI element
        if (textObj.GetComponent<RectTransform>() == null)
            textObj.AddComponent<RectTransform>();
        
        Text errorText = textObj.AddComponent<Text>();
        errorText.text = message;
        errorText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        errorText.fontSize = 16;
        errorText.color = Color.white;
        errorText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
    
    void CreateRecipeButton(CraftingRecipe recipe, bool canCraft, Dictionary<string, int> playerItems)
    {
        Debug.Log($"[AutoCraftingUI] Creating recipe button for {recipe.recipeName}");
        
        GameObject buttonObj = new GameObject($"Recipe_{recipe.recipeName}");
        buttonObj.transform.SetParent(recipeContainer, false);
        
        // Button background
        Image buttonBg = buttonObj.AddComponent<Image>();
        buttonBg.color = canCraft ? new Color(0.3f, 0.6f, 0.3f, 0.8f) : new Color(0.6f, 0.3f, 0.3f, 0.8f);
        
        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = buttonBg;
        button.interactable = canCraft;
        
        // Set size
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(0, 80); // Auto width, 80 height
        
        Debug.Log($"[AutoCraftingUI] Button background and size set");
        
        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        // Ensure RectTransform exists for UI element
        if (textObj.GetComponent<RectTransform>() == null)
            textObj.AddComponent<RectTransform>();
        
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 16;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        
        // Build recipe text
        string recipeText = $"<b>{recipe.recipeName}</b>\n";
        recipeText += "Needs: ";
        foreach (var ingredient in recipe.ingredients)
        {
            int playerAmount = playerItems.ContainsKey(ingredient.itemName) ? playerItems[ingredient.itemName] : 0;
            recipeText += $"{ingredient.itemName}({playerAmount}/{ingredient.amount}) ";
        }
        recipeText += $"\n→ {recipe.resultAmount}x {recipe.resultItem}";
        
        buttonText.text = recipeText;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(5, 5);
        textRect.offsetMax = new Vector2(-5, -5);
        
        // Add click handler
        button.onClick.AddListener(() => TryCraftRecipe(recipe));
        
        Debug.Log($"[AutoCraftingUI] Recipe button created for {recipe.recipeName} with text: {recipeText}");
    }
    
    Dictionary<string, int> GetPlayerItems()
    {
        Dictionary<string, int> items = new Dictionary<string, int>();
        
        if (ContainerManager.Main?.playerInventory != null)
        {
            var inventory = ContainerManager.Main.playerInventory;
            
            // Check if SlotIDs is available
            if (inventory.SlotIDs != null)
            {
                foreach (string slotID in inventory.SlotIDs)
                {
                    if (slotID != null)
                    {
                        ItemStack itemStack = inventory.Peek(slotID);
                        if (itemStack?.item != null)
                        {
                            string itemName = itemStack.item.name;
                            if (items.ContainsKey(itemName))
                                items[itemName] += itemStack.amount;
                            else
                                items[itemName] = itemStack.amount;
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("[AutoCraftingUI] Inventory SlotIDs is null - inventory not initialized properly");
            }
        }
        else
        {
            Debug.LogWarning("[AutoCraftingUI] ContainerManager or playerInventory is null");
        }
        
        return items;
    }
    
    void TryCraftRecipe(CraftingRecipe recipe)
    {
        if (CraftingSystem.Main == null)
        {
            Debug.LogError("[AutoCraftingUI] CraftingSystem.Main is null!");
            return;
        }
        
        Dictionary<string, int> playerItems = GetPlayerItems();
        
        if (CraftingSystem.Main.TryCraft(recipe, playerItems))
        {
            // Remove ingredients from inventory
            RemoveItemsFromInventory(recipe);
            
            // Add result to inventory
            AddItemToInventory(recipe.resultItem, recipe.resultAmount);
            
            // Refresh the UI
            RefreshCraftingMenu();
            
            Debug.Log($"✅ Crafted {recipe.resultAmount}x {recipe.resultItem}!");
        }
        else
        {
            Debug.Log("❌ Cannot craft - insufficient materials!");
        }
    }
    
    void RemoveItemsFromInventory(CraftingRecipe recipe)
    {
        if (ContainerManager.Main?.playerInventory == null) return;
        
        var inventory = ContainerManager.Main.playerInventory;
        
        foreach (var ingredient in recipe.ingredients)
        {
            int toRemove = ingredient.amount;
            
            foreach (string slotID in inventory.SlotIDs)
            {
                if (toRemove <= 0) break;
                
                ItemStack itemStack = inventory.Peek(slotID);
                if (itemStack?.item?.name == ingredient.itemName)
                {
                    int removeFromSlot = Mathf.Min(toRemove, itemStack.amount);
                    itemStack.amount -= removeFromSlot;
                    toRemove -= removeFromSlot;
                    
                    if (itemStack.amount <= 0)
                        inventory.ClearSlot(slotID);
                    else
                        inventory.UpdateSlot(slotID, itemStack);
                }
            }
        }
    }
    
    void AddItemToInventory(string itemName, int amount)
    {
        if (ContainerManager.Main?.playerInventory == null) return;
        
        Item item = ItemDatabase.Main.GetCopy(itemName);
        if (item != null)
        {
            ItemStack itemStack = new ItemStack(item, amount);
            ContainerManager.Main.playerInventory.TryAlter(itemStack);
        }
    }
    
    void OnDisable()
    {
        if (isOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}