using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    
    [Header("UI References")]
    public Slider healthBar;
    public Text healthText;
    
    [Header("Healing")]
    public KeyCode healKey = KeyCode.H;
    
    public static PlayerHealth Instance;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            currentHealth = maxHealth;
        }
        else
        {
            Destroy(this);
        }
    }
    
    void Start()
    {
        UpdateHealthUI();
    }
    
    void Update()
    {
        // Heal with H key (if player has healing potions)
        if (Input.GetKeyDown(healKey))
        {
            TryUseHealingPotion();
        }
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        Debug.Log($"Player took {damage} damage. Health: {currentHealth}/{maxHealth}");
        UpdateHealthUI();
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        Debug.Log($"Player healed {amount}. Health: {currentHealth}/{maxHealth}");
        UpdateHealthUI();
    }
    
    void TryUseHealingPotion()
    {
        // Check if player has healing potions in inventory
        if (ContainerManager.Main != null && ContainerManager.Main.playerInventory != null)
        {
            var inventory = ContainerManager.Main.playerInventory;
            
            // Check all inventory slots for healing potions
            foreach (string slotID in inventory.SlotIDs)
            {
                ItemStack itemStack = inventory.Peek(slotID);
                if (itemStack != null && 
                    itemStack.item != null && 
                    itemStack.item.name == "HealingPotion")
                {
                    // Use the potion
                    Heal(50); // Heal 50 HP
                    
                    // Remove one potion from inventory
                    itemStack.amount--;
                    if (itemStack.amount <= 0)
                    {
                        inventory.ClearSlot(slotID);
                    }
                    else
                    {
                        inventory.UpdateSlot(slotID, itemStack);
                    }
                    
                    Debug.Log("Used healing potion!");
                    return;
                }
            }
            Debug.Log("No healing potions in inventory!");
        }
    }
    
    void Die()
    {
        Debug.Log("Player died! Returning to main menu...");
        
        // Show death message
        StartCoroutine(HandlePlayerDeath());
    }
    
    System.Collections.IEnumerator HandlePlayerDeath()
    {
        // Optional: Show death screen or fade effect
        Debug.Log("💀 GAME OVER - You have died!");
        
        // Wait a moment for the death to register
        yield return new WaitForSeconds(2f);
        
        // Try to save the game state before returning to menu
        try
        {
            if (SaveManager.Main != null)
            {
                // Trigger autosave before returning to menu
                SaveManager.Main.Save("autosave");
                Debug.Log("Game state saved before returning to menu");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Could not save game state on death: {e.Message}");
        }
        
        // Return to main menu
        LoadMainMenu();
    }
    
    void LoadMainMenu()
    {
        try
        {
            // Load the main menu scene
            SceneManager.LoadScene("MainMenu");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Could not load MainMenu scene: {e.Message}");
            // Fallback: try scene index 0
            try
            {
                SceneManager.LoadScene(0);
            }
            catch (System.Exception e2)
            {
                Debug.LogError($"Could not load scene by index: {e2.Message}");
                // Final fallback: quit application
                Debug.Log("Unable to load menu, quitting application");
                Application.Quit();
            }
        }
    }
    
    void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.value = (float)currentHealth / maxHealth;
        }
        
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }
    
    // Public method for other scripts to check health
    public bool IsAlive()
    {
        return currentHealth > 0;
    }
    
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }
    
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public float GetMaxHealth()
    {
        return maxHealth;
    }
}