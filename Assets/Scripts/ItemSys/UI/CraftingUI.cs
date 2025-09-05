using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class CraftingUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject craftingPanel;
    public Transform recipeContainer;
    public GameObject recipeButtonPrefab;
    public KeyCode craftingKey = KeyCode.C;
    
    [Header("Recipe Display")]
    public Text selectedRecipeText;
    public Text ingredientsText;
    public Button craftButton;
    
    private List<CraftingRecipe> availableRecipes = new List<CraftingRecipe>();
    private CraftingRecipe selectedRecipe;
    private Dictionary<string, int> playerItems = new Dictionary<string, int>();
    
    void Start()
    {
        if (craftingPanel != null)
            craftingPanel.SetActive(false);
            
        if (craftButton != null)
            craftButton.onClick.AddListener(CraftSelectedItem);
    }
    
    void Update()
    {
        if (Input.GetKeyDown(craftingKey))
        {
            ToggleCraftingUI();
        }
    }
    
    void ToggleCraftingUI()
    {
        bool isActive = craftingPanel.activeSelf;
        craftingPanel.SetActive(!isActive);
        
        if (!isActive)
        {
            UpdatePlayerInventory();
            RefreshRecipeList();
        }
        
        // Pause/unpause game when crafting menu is open
        Time.timeScale = isActive ? 1f : 0f;
        
        // Enable/disable cursor
        Cursor.lockState = isActive ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isActive;
    }
    
    void UpdatePlayerInventory()
    {
        playerItems.Clear();
        
        if (ContainerManager.Main != null && ContainerManager.Main.playerInventory != null)
        {
            var inventory = ContainerManager.Main.playerInventory;
            
            // Check all inventory slots
            foreach (string slotID in inventory.SlotIDs)
            {
                ItemStack itemStack = inventory.Peek(slotID);
                if (itemStack != null && itemStack.item != null)
                {
                    string itemName = itemStack.item.name;
                    if (playerItems.ContainsKey(itemName))
                        playerItems[itemName] += itemStack.amount;
                    else
                        playerItems[itemName] = itemStack.amount;
                }
            }
        }
    }
    
    void RefreshRecipeList()
    {
        // Clear existing recipe buttons
        foreach (Transform child in recipeContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Get available recipes
        availableRecipes = CraftingSystem.Main.GetAvailableRecipes(playerItems);
        
        // Create recipe buttons
        foreach (var recipe in availableRecipes)
        {
            CreateRecipeButton(recipe);
        }
        
        // Also show unavailable recipes but grayed out
        var allRecipes = CraftingSystem.Main.recipes;
        foreach (var recipe in allRecipes)
        {
            if (!availableRecipes.Contains(recipe))
            {
                CreateRecipeButton(recipe, false);
            }
        }
    }
    
    void CreateRecipeButton(CraftingRecipe recipe, bool canCraft = true)
    {
        if (recipeButtonPrefab == null) return;
        
        GameObject buttonObj = Instantiate(recipeButtonPrefab, recipeContainer);
        Button button = buttonObj.GetComponent<Button>();
        Text buttonText = buttonObj.GetComponentInChildren<Text>();
        
        if (buttonText != null)
        {
            buttonText.text = recipe.recipeName;
            buttonText.color = canCraft ? Color.white : Color.gray;
        }
        
        if (button != null)
        {
            button.interactable = canCraft;
            button.onClick.AddListener(() => SelectRecipe(recipe));
        }
    }
    
    void SelectRecipe(CraftingRecipe recipe)
    {
        selectedRecipe = recipe;
        
        if (selectedRecipeText != null)
        {
            selectedRecipeText.text = $"Selected: {recipe.recipeName}";
        }
        
        if (ingredientsText != null)
        {
            string ingredients = "Requires:\n";
            foreach (var ingredient in recipe.ingredients)
            {
                int playerAmount = playerItems.ContainsKey(ingredient.itemName) ? playerItems[ingredient.itemName] : 0;
                Color color = playerAmount >= ingredient.amount ? Color.green : Color.red;
                
                ingredients += $"• {ingredient.itemName}: {playerAmount}/{ingredient.amount}\n";
            }
            ingredients += $"\nProduces: {recipe.resultAmount}x {recipe.resultItem}";
            
            ingredientsText.text = ingredients;
        }
        
        if (craftButton != null)
        {
            craftButton.interactable = recipe.CanCraft(playerItems);
        }
    }
    
    void CraftSelectedItem()
    {
        if (selectedRecipe == null) return;
        
        bool success = CraftingSystem.Main.TryCraft(selectedRecipe, playerItems);
        if (success)
        {
            // Add crafted item to inventory
            AddCraftedItemToInventory(selectedRecipe.resultItem, selectedRecipe.resultAmount);
            
            // Refresh UI
            UpdatePlayerInventory();
            RefreshRecipeList();
            
            // Update selected recipe display
            if (selectedRecipe != null)
                SelectRecipe(selectedRecipe);
                
            Debug.Log($"Successfully crafted {selectedRecipe.resultItem}!");
        }
        else
        {
            Debug.Log("Failed to craft item!");
        }
    }
    
    void AddCraftedItemToInventory(string itemName, int amount)
    {
        Item item = ItemDatabase.Main.GetCopy(itemName);
        if (item != null && ContainerManager.Main.playerInventory != null)
        {
            ItemStack itemStack = new ItemStack(item, amount);
            ContainerManager.Main.playerInventory.TryAlter(itemStack);
        }
    }
    
    void OnDisable()
    {
        // Make sure to restore time scale when disabled
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}