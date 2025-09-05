using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemDatabase : Singleton<ItemDatabase>
{

    Dictionary<string, Item> itemDatabase = new Dictionary<string, Item>();
    Dictionary<string, Item> generatedBlockItems = new Dictionary<string, Item>();

    public Item[] items;

    public void Awake()
    {
        //Item[] items = (Item[]) Resources.FindObjectsOfTypeAll(typeof(Item));

        foreach (Item i in items)
        {
            itemDatabase.Add(i.name, i);
            print("Item added to database: " + i.name);
        }
        
        // Pre-generate all voxel block items
        CreateAllVoxelBlockItems();
        
        // Create missing crafting items
        CreateMissingCraftingItems();
    }

    void CreateAllVoxelBlockItems()
    {
        // Create items for all voxel types
        foreach (Voxel voxelType in System.Enum.GetValues(typeof(Voxel)))
        {
            if (voxelType != Voxel.Air && voxelType != Voxel.Object)
            {
                string voxelName = voxelType.ToString();
                if (!itemDatabase.ContainsKey(voxelName))
                {
                    Block blockItem = ScriptableObject.CreateInstance<Block>();
                    blockItem.name = voxelName;
                    blockItem.voxelType = voxelType;
                    
                    // Set appropriate stack sizes
                    switch (voxelType)
                    {
                        case Voxel.Stone:
                        case Voxel.Dirt:
                        case Voxel.Sand:
                        case Voxel.Clay:
                            blockItem.maxStack = 64;
                            break;
                        case Voxel.Coal:
                        case Voxel.Wood:
                        case Voxel.Leaves:
                            blockItem.maxStack = 32;
                            break;
                        case Voxel.Iron:
                        case Voxel.Gold:
                            blockItem.maxStack = 16;
                            break;
                        case Voxel.Diamond:
                        case Voxel.Magic:
                            blockItem.maxStack = 8;
                            break;
                        default:
                            blockItem.maxStack = 64;
                            break;
                    }
                    
                    // Assign appropriate thumbnail icons
                    blockItem.thumbnail = GetVoxelThumbnail(voxelType);
                    
                    blockItem.description = $"A {voxelName} block";
                    blockItem.reusable = false; // Consumable when used
                    
                    generatedBlockItems.Add(voxelName, blockItem);
                    Debug.Log($"Pre-generated block item for: {voxelName} (maxStack: {blockItem.maxStack})");
                }
            }
        }
    }

    public Item GetCopy(string itemName)
    {
        // First check explicit items
        if (itemDatabase.ContainsKey(itemName))
            return itemDatabase[itemName].GetCopy();
            
        // Then check generated block items
        if (generatedBlockItems.ContainsKey(itemName))
            return generatedBlockItems[itemName]; // Return same instance for stacking
            
        // Fallback: try to create a basic block item for voxel types
        return CreateBlockItem(itemName);
    }
    
    Item CreateBlockItem(string voxelName)
    {
        // Create a basic block item for new voxel types
        if (System.Enum.TryParse<Voxel>(voxelName, out Voxel voxelType))
        {
            Block blockItem = ScriptableObject.CreateInstance<Block>();
            blockItem.name = voxelName;
            blockItem.voxelType = voxelType;
            blockItem.maxStack = 64;
            blockItem.thumbnail = GetVoxelThumbnail(voxelType);
            blockItem.description = $"A {voxelName} block";
            blockItem.reusable = true;
            Debug.Log($"Created block item for: {voxelName}");
            return blockItem;
        }
        
        Debug.LogError($"Could not create item for: {voxelName}");
        return null;
    }
    
    Sprite GetVoxelThumbnail(Voxel voxelType)
    {
        // Load appropriate thumbnail sprite for each voxel type
        string iconPath = "";
        
        switch (voxelType)
        {
            case Voxel.Stone:
                iconPath = "Art/Items/block-icon-stone";
                break;
            case Voxel.Brick:
                iconPath = "Art/Items/block-icon-brick";
                break;
            case Voxel.Magic:
                iconPath = "Art/Items/block-icon-magic";
                break;
            // For blocks without specific icons, use stone as default
            case Voxel.Dirt:
            case Voxel.Grass:
            case Voxel.Sand:
            case Voxel.Clay:
            case Voxel.Coal:
            case Voxel.Iron:
            case Voxel.Gold:
            case Voxel.Diamond:
            case Voxel.Wood:
            case Voxel.Leaves:
            case Voxel.Water:
            case Voxel.Lava:
            default:
                iconPath = "Art/Items/block-icon-stone";
                break;
        }
        
        Sprite thumbnail = Resources.Load<Sprite>(iconPath);
        if (thumbnail == null)
        {
            Debug.LogWarning($"Could not load thumbnail for {voxelType} at path: {iconPath}");
            // Try to load stone as absolute fallback
            thumbnail = Resources.Load<Sprite>("Art/Items/block-icon-stone");
        }
        
        return thumbnail;
    }
    
    void CreateMissingCraftingItems()
    {
        // Create HealingPotion if it doesn't exist
        if (!itemDatabase.ContainsKey("HealingPotion"))
        {
            HealingPotion healingPotion = ScriptableObject.CreateInstance<HealingPotion>();
            healingPotion.name = "HealingPotion";
            healingPotion.healAmount = 50;
            healingPotion.maxStack = 10;
            healingPotion.description = "Restores 50 health points";
            healingPotion.reusable = false; // Consumable
            healingPotion.thumbnail = Resources.Load<Sprite>("Art/Items/block-icon-magic"); // Use magic icon for now
            
            itemDatabase.Add("HealingPotion", healingPotion);
            Debug.Log("Created HealingPotion item");
        }
        
        // Create StoneSword if it doesn't exist
        if (!itemDatabase.ContainsKey("StoneSword"))
        {
            Equipment stoneSword = ScriptableObject.CreateInstance<Equipment>();
            stoneSword.name = "StoneSword";
            stoneSword.type = EquipmentType.Weapon;
            stoneSword.meleeDamage = 20;
            stoneSword.maxStack = 1;
            stoneSword.description = "A sword made of stone. Deals 20 damage.";
            stoneSword.reusable = true;
            stoneSword.thumbnail = Resources.Load<Sprite>("Art/Items/block-icon-stone");
            
            itemDatabase.Add("StoneSword", stoneSword);
            Debug.Log("Created StoneSword item");
        }
        
        // Create IronSword if it doesn't exist
        if (!itemDatabase.ContainsKey("IronSword"))
        {
            Equipment ironSword = ScriptableObject.CreateInstance<Equipment>();
            ironSword.name = "IronSword";
            ironSword.type = EquipmentType.Weapon;
            ironSword.meleeDamage = 30;
            ironSword.maxStack = 1;
            ironSword.description = "A sword made of iron. Deals 30 damage.";
            ironSword.reusable = true;
            ironSword.thumbnail = Resources.Load<Sprite>("Art/Items/block-icon-stone"); // Use stone icon for now
            
            itemDatabase.Add("IronSword", ironSword);
            Debug.Log("Created IronSword item");
        }
        
        // Create DiamondSword if it doesn't exist (for PlayerCombat compatibility)
        if (!itemDatabase.ContainsKey("DiamondSword"))
        {
            Equipment diamondSword = ScriptableObject.CreateInstance<Equipment>();
            diamondSword.name = "DiamondSword";
            diamondSword.type = EquipmentType.Weapon;
            diamondSword.meleeDamage = 45;
            diamondSword.maxStack = 1;
            diamondSword.description = "A sword made of diamond. Deals 45 damage.";
            diamondSword.reusable = true;
            diamondSword.thumbnail = Resources.Load<Sprite>("Art/Items/block-icon-stone"); // Use stone icon for now
            
            itemDatabase.Add("DiamondSword", diamondSword);
            Debug.Log("Created DiamondSword item");
        }
    }

    public GameObject GetPrefab(string itemName)
    {
        return itemDatabase[itemName].prefab;
    }
}
