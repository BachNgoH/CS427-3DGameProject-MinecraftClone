using UnityEngine;
using System.Collections.Generic;

public class SimpleCraftingManager : MonoBehaviour
{
    [Header("Crafting Settings")]
    public KeyCode craftingKey = KeyCode.C;
    
    private bool craftingMenuOpen = false;
    
    void Update()
    {
        if (Input.GetKeyDown(craftingKey))
        {
            ToggleCraftingMenu();
        }
    }
    
    void ToggleCraftingMenu()
    {
        craftingMenuOpen = !craftingMenuOpen;
        
        if (craftingMenuOpen)
        {
            ShowSimpleCraftingMenu();
        }
        else
        {
            Debug.Log("=== Crafting Menu Closed ===");
        }
    }
    
    void ShowSimpleCraftingMenu()
    {
        Debug.Log("=== CRAFTING MENU ===");
        Debug.Log("Press C to close | Press number keys to craft:");
        
        // Get player inventory
        Dictionary<string, int> playerItems = GetPlayerItems();
        
        Debug.Log("\nYour Items:");
        foreach (var item in playerItems)
        {
            Debug.Log($"  {item.Key}: {item.Value}");
        }
        
        // Show available recipes
        Debug.Log("\nAvailable Recipes:");
        ShowRecipes(playerItems);
        
        // Listen for crafting inputs
        StartCoroutine(ListenForCraftingInput());
    }
    
    Dictionary<string, int> GetPlayerItems()
    {
        Dictionary<string, int> items = new Dictionary<string, int>();
        
        if (ContainerManager.Main != null && ContainerManager.Main.playerInventory != null)
        {
            var inventory = ContainerManager.Main.playerInventory;
            
            foreach (string slotID in inventory.SlotIDs)
            {
                ItemStack itemStack = inventory.Peek(slotID);
                if (itemStack != null && itemStack.item != null)
                {
                    string itemName = itemStack.item.name;
                    if (items.ContainsKey(itemName))
                        items[itemName] += itemStack.amount;
                    else
                        items[itemName] = itemStack.amount;
                }
            }
        }
        
        return items;
    }
    
    void ShowRecipes(Dictionary<string, int> playerItems)
    {
        if (CraftingSystem.Main == null) return;
        
        var availableRecipes = CraftingSystem.Main.GetAvailableRecipes(playerItems);
        
        Debug.Log("1. Healing Potion (2 Stone + 1 Magic) " + (CanCraft("Stone", 2, "Magic", 1, playerItems) ? "[AVAILABLE]" : "[NEED MORE]"));
        Debug.Log("2. Stone Sword (1 Wood + 2 Stone) " + (CanCraft("Wood", 1, "Stone", 2, playerItems) ? "[AVAILABLE]" : "[NEED MORE]"));
        Debug.Log("3. Iron Sword (1 Wood + 3 Iron) " + (CanCraft("Wood", 1, "Iron", 3, playerItems) ? "[AVAILABLE]" : "[NEED MORE]"));
        Debug.Log("4. Brick (4 Stone) " + (CanCraft("Stone", 4, "", 0, playerItems) ? "[AVAILABLE]" : "[NEED MORE]"));
    }
    
    bool CanCraft(string item1, int amount1, string item2, int amount2, Dictionary<string, int> playerItems)
    {
        bool hasItem1 = playerItems.ContainsKey(item1) && playerItems[item1] >= amount1;
        bool hasItem2 = string.IsNullOrEmpty(item2) || (playerItems.ContainsKey(item2) && playerItems[item2] >= amount2);
        return hasItem1 && hasItem2;
    }
    
    System.Collections.IEnumerator ListenForCraftingInput()
    {
        while (craftingMenuOpen)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) { TryCraftByIndex(0); break; }
            if (Input.GetKeyDown(KeyCode.Alpha2)) { TryCraftByIndex(1); break; }
            if (Input.GetKeyDown(KeyCode.Alpha3)) { TryCraftByIndex(2); break; }
            if (Input.GetKeyDown(KeyCode.Alpha4)) { TryCraftByIndex(3); break; }
            yield return null;
        }
    }
    
    void TryCraftByIndex(int index)
    {
        if (CraftingSystem.Main == null || index >= CraftingSystem.Main.recipes.Count) return;
        
        var recipe = CraftingSystem.Main.recipes[index];
        Dictionary<string, int> playerItems = GetPlayerItems();
        
        if (CraftingSystem.Main.TryCraft(recipe, playerItems))
        {
            // Remove ingredients from inventory
            RemoveItemsFromInventory(recipe);
            
            // Add result to inventory
            AddItemToInventory(recipe.resultItem, recipe.resultAmount);
            
            Debug.Log($"✓ Crafted {recipe.resultAmount}x {recipe.resultItem}!");
        }
        else
        {
            Debug.Log("✗ Cannot craft - insufficient materials!");
        }
        
        craftingMenuOpen = false;
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
}