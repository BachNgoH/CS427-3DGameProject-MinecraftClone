using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SimpleCraftingMenu : MonoBehaviour
{
    [Header("Controls")]
    public KeyCode craftingKey = KeyCode.C;
    
    private bool isMenuOpen = false;
    private Dictionary<string, int> playerItems = new Dictionary<string, int>();
    
    void Update()
    {
        if (Input.GetKeyDown(craftingKey))
        {
            ToggleCraftingMenu();
        }
        
        // If menu is open, check for number key inputs
        if (isMenuOpen)
        {
            CheckCraftingInputs();
        }
    }
    
    void ToggleCraftingMenu()
    {
        isMenuOpen = !isMenuOpen;
        
        if (isMenuOpen)
        {
            UpdatePlayerInventory();
            ShowCraftingMenu();
            
            // Pause game and show cursor
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            HideCraftingMenu();
            
            // Resume game
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    void UpdatePlayerInventory()
    {
        playerItems.Clear();
        
        if (ContainerManager.Main != null && ContainerManager.Main.playerInventory != null)
        {
            var inventory = ContainerManager.Main.playerInventory;
            
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
    
    void ShowCraftingMenu()
    {
        Debug.Log("=== CRAFTING MENU ===");
        Debug.Log("Press 'C' again to close");
        Debug.Log("");
        Debug.Log("Your Items:");
        
        foreach (var item in playerItems)
        {
            Debug.Log($"  {item.Key}: {item.Value}");
        }
        
        Debug.Log("");
        Debug.Log("Available Recipes (Press number key to craft):");
        
        var availableRecipes = CraftingSystem.Main.GetAvailableRecipes(playerItems);
        var allRecipes = CraftingSystem.Main.recipes;
        
        for (int i = 0; i < allRecipes.Count; i++)
        {
            var recipe = allRecipes[i];
            bool canCraft = availableRecipes.Contains(recipe);
            string status = canCraft ? "[AVAILABLE]" : "[NEED MORE ITEMS]";
            
            Debug.Log($"  {i + 1}. {recipe.recipeName} {status}");
            Debug.Log($"     Requires: {GetRecipeIngredients(recipe)}");
            Debug.Log($"     Produces: {recipe.resultAmount}x {recipe.resultItem}");
            Debug.Log("");
        }
    }
    
    string GetRecipeIngredients(CraftingRecipe recipe)
    {
        return string.Join(", ", recipe.ingredients.Select(ing => $"{ing.amount}x {ing.itemName}"));
    }
    
    void HideCraftingMenu()
    {
        Debug.Log("Crafting menu closed");
    }
    
    void CheckCraftingInputs()
    {
        // Check for number keys 1-9
        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                TryCraftRecipe(i - 1); // Convert to 0-based index
                break;
            }
        }
    }
    
    void TryCraftRecipe(int recipeIndex)
    {
        var allRecipes = CraftingSystem.Main.recipes;
        
        if (recipeIndex >= 0 && recipeIndex < allRecipes.Count)
        {
            var recipe = allRecipes[recipeIndex];
            
            // Check if we can craft this recipe
            if (recipe.CanCraft(playerItems))
            {
                // Try to craft
                bool success = CraftingSystem.Main.TryCraft(recipe, playerItems);
                if (success)
                {
                    // Add crafted item to inventory
                    AddCraftedItemToInventory(recipe.resultItem, recipe.resultAmount);
                    
                    Debug.Log($"Successfully crafted {recipe.resultAmount}x {recipe.resultItem}!");
                    
                    // Refresh the menu
                    UpdatePlayerInventory();
                    ShowCraftingMenu();
                }
                else
                {
                    Debug.Log("Failed to craft item!");
                }
            }
            else
            {
                Debug.Log($"Cannot craft {recipe.recipeName} - insufficient materials!");
            }
        }
        else
        {
            Debug.Log("Invalid recipe number!");
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
        if (isMenuOpen)
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}