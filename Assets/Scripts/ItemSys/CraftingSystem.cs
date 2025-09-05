using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class CraftingRecipe
{
    public string recipeName;
    public List<CraftingIngredient> ingredients;
    public string resultItem;
    public int resultAmount = 1;
    
    public bool CanCraft(Dictionary<string, int> playerItems)
    {
        foreach (var ingredient in ingredients)
        {
            if (!playerItems.ContainsKey(ingredient.itemName) || 
                playerItems[ingredient.itemName] < ingredient.amount)
                return false;
        }
        return true;
    }
}

[System.Serializable]
public class CraftingIngredient
{
    public string itemName;
    public int amount;
}

public class CraftingSystem : Singleton<CraftingSystem>
{
    public List<CraftingRecipe> recipes = new List<CraftingRecipe>();
    
    void Awake()
    {
        InitializeRecipes();
    }
    
    void InitializeRecipes()
    {
        // Healing Potion: 2 Stone + 1 Magic = 1 Healing Potion
        recipes.Add(new CraftingRecipe
        {
            recipeName = "Healing Potion",
            ingredients = new List<CraftingIngredient>
            {
                new CraftingIngredient { itemName = "Stone", amount = 2 },
                new CraftingIngredient { itemName = "Magic", amount = 1 }
            },
            resultItem = "HealingPotion",
            resultAmount = 1
        });
        
        // Tools: Wood + Stone = Better tools
        recipes.Add(new CraftingRecipe
        {
            recipeName = "Stone Sword",
            ingredients = new List<CraftingIngredient>
            {
                new CraftingIngredient { itemName = "Wood", amount = 1 },
                new CraftingIngredient { itemName = "Stone", amount = 2 }
            },
            resultItem = "StoneSword",
            resultAmount = 1
        });
        
        // Brick crafting: 4 Stone = 1 Brick
        recipes.Add(new CraftingRecipe
        {
            recipeName = "Brick",
            ingredients = new List<CraftingIngredient>
            {
                new CraftingIngredient { itemName = "Stone", amount = 4 }
            },
            resultItem = "Brick",
            resultAmount = 1
        });
        
        // Iron tools: Wood + Iron
        recipes.Add(new CraftingRecipe
        {
            recipeName = "Iron Sword",
            ingredients = new List<CraftingIngredient>
            {
                new CraftingIngredient { itemName = "Wood", amount = 1 },
                new CraftingIngredient { itemName = "Iron", amount = 3 }
            },
            resultItem = "IronSword",
            resultAmount = 1
        });
        
        Debug.Log($"Initialized {recipes.Count} crafting recipes");
    }
    
    public List<CraftingRecipe> GetAvailableRecipes(Dictionary<string, int> playerItems)
    {
        return recipes.Where(recipe => recipe.CanCraft(playerItems)).ToList();
    }
    
    public bool TryCraft(CraftingRecipe recipe, Dictionary<string, int> playerItems)
    {
        if (!recipe.CanCraft(playerItems))
            return false;
            
        // Remove ingredients
        foreach (var ingredient in recipe.ingredients)
        {
            playerItems[ingredient.itemName] -= ingredient.amount;
            if (playerItems[ingredient.itemName] <= 0)
                playerItems.Remove(ingredient.itemName);
        }
        
        // Add result
        if (playerItems.ContainsKey(recipe.resultItem))
            playerItems[recipe.resultItem] += recipe.resultAmount;
        else
            playerItems[recipe.resultItem] = recipe.resultAmount;
            
        Debug.Log($"Crafted: {recipe.resultItem}");
        return true;
    }
}