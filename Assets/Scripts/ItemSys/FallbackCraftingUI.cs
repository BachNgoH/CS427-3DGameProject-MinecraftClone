using UnityEngine;
using System.Collections.Generic;

public class FallbackCraftingUI : MonoBehaviour
{
    [Header("Fallback Crafting")]
    public KeyCode craftingKey = KeyCode.C;
    
    private bool menuOpen = false;
    private float lastToggleTime = 0f;
    
    void Update()
    {
        if (Input.GetKeyDown(craftingKey) && Time.time - lastToggleTime > 0.5f)
        {
            lastToggleTime = Time.time;
            ToggleCraftingMenu();
        }
        
        if (menuOpen)
        {
            CheckCraftingInputs();
        }
    }
    
    void ToggleCraftingMenu()
    {
        menuOpen = !menuOpen;
        
        if (menuOpen)
        {
            ShowCraftingMenu();
        }
        else
        {
            Debug.Log("[FallbackCrafting] Crafting menu closed.");
        }
    }
    
    void ShowCraftingMenu()
    {
        Debug.Log("=== FALLBACK CRAFTING MENU ===");
        Debug.Log("Press C to close | Press number keys (1-4) to craft:");
        
        // Show player inventory
        Dictionary<string, int> playerItems = GetPlayerItems();
        string inventoryText = "Your Items: ";
        foreach (var item in playerItems)
        {
            inventoryText += $"{item.Key}({item.Value}) ";
        }
        Debug.Log(inventoryText);
        
        Debug.Log("");
        Debug.Log("Recipes:");
        Debug.Log("1. Healing Potion (2 Stone + 1 Magic) " + (CanCraft(playerItems, "Stone", 2, "Magic", 1) ? "✅" : "❌"));
        Debug.Log("2. Stone Sword (1 Wood + 2 Stone) " + (CanCraft(playerItems, "Wood", 1, "Stone", 2) ? "✅" : "❌"));
        Debug.Log("3. Iron Sword (1 Wood + 3 Iron) " + (CanCraft(playerItems, "Wood", 1, "Iron", 3) ? "✅" : "❌"));
        Debug.Log("4. Brick (4 Stone) " + (CanCraft(playerItems, "Stone", 4, "", 0) ? "✅" : "❌"));
        Debug.Log("================================");
    }
    
    bool CanCraft(Dictionary<string, int> items, string item1, int amount1, string item2, int amount2)
    {
        bool has1 = items.ContainsKey(item1) && items[item1] >= amount1;
        bool has2 = string.IsNullOrEmpty(item2) || (items.ContainsKey(item2) && items[item2] >= amount2);
        return has1 && has2;
    }
    
    void CheckCraftingInputs()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) TryCraftItem(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) TryCraftItem(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) TryCraftItem(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) TryCraftItem(3);
    }
    
    void TryCraftItem(int recipeIndex)
    {
        menuOpen = false;
        
        Dictionary<string, int> playerItems = GetPlayerItems();
        
        switch (recipeIndex)
        {
            case 0: // Healing Potion
                if (CanCraft(playerItems, "Stone", 2, "Magic", 1))
                {
                    RemoveItems("Stone", 2);
                    RemoveItems("Magic", 1);
                    AddItem("HealingPotion", 1);
                    Debug.Log("✅ Crafted Healing Potion!");
                }
                else Debug.Log("❌ Need 2 Stone + 1 Magic");
                break;
                
            case 1: // Stone Sword
                if (CanCraft(playerItems, "Wood", 1, "Stone", 2))
                {
                    RemoveItems("Wood", 1);
                    RemoveItems("Stone", 2);
                    AddItem("StoneSword", 1);
                    Debug.Log("✅ Crafted Stone Sword!");
                }
                else Debug.Log("❌ Need 1 Wood + 2 Stone");
                break;
                
            case 2: // Iron Sword
                if (CanCraft(playerItems, "Wood", 1, "Iron", 3))
                {
                    RemoveItems("Wood", 1);
                    RemoveItems("Iron", 3);
                    AddItem("IronSword", 1);
                    Debug.Log("✅ Crafted Iron Sword!");
                }
                else Debug.Log("❌ Need 1 Wood + 3 Iron");
                break;
                
            case 3: // Brick
                if (CanCraft(playerItems, "Stone", 4, "", 0))
                {
                    RemoveItems("Stone", 4);
                    AddItem("Brick", 1);
                    Debug.Log("✅ Crafted Brick!");
                }
                else Debug.Log("❌ Need 4 Stone");
                break;
        }
    }
    
    Dictionary<string, int> GetPlayerItems()
    {
        Dictionary<string, int> items = new Dictionary<string, int>();
        
        if (ContainerManager.Main?.playerInventory != null)
        {
            var inventory = ContainerManager.Main.playerInventory;
            
            foreach (string slotID in inventory.SlotIDs)
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
        
        return items;
    }
    
    void RemoveItems(string itemName, int amount)
    {
        if (ContainerManager.Main?.playerInventory == null) return;
        
        var inventory = ContainerManager.Main.playerInventory;
        int toRemove = amount;
        
        foreach (string slotID in inventory.SlotIDs)
        {
            if (toRemove <= 0) break;
            
            ItemStack itemStack = inventory.Peek(slotID);
            if (itemStack?.item?.name == itemName)
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
    
    void AddItem(string itemName, int amount)
    {
        if (ContainerManager.Main?.playerInventory == null) return;
        
        Item item = ItemDatabase.Main?.GetCopy(itemName);
        if (item != null)
        {
            ItemStack itemStack = new ItemStack(item, amount);
            ContainerManager.Main.playerInventory.TryAlter(itemStack);
        }
    }
}