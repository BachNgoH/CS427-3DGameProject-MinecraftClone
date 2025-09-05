using UnityEngine;

public class SystemTester : MonoBehaviour
{
    [Header("Testing Controls")]
    public KeyCode testDeathKey = KeyCode.F9;
    public KeyCode spawnEnemyKey = KeyCode.F10;
    public KeyCode testSaveKey = KeyCode.F11;
    public KeyCode testCombatKey = KeyCode.F12;
    
    [Header("Test Settings")]
    public GameObject hobbitEnemyPrefab;
    public float spawnDistance = 10f;
    
    void Start()
    {
        Debug.Log("=== SystemTester Started ===");
        Debug.Log("F9: Test player death (reduces health to 0)");
        Debug.Log("F10: Spawn hobbit enemy near player");
        Debug.Log("F11: Test chunk save system");
        Debug.Log("F12: Test combat system (give player weapon)");
        Debug.Log("========================");
    }
    
    void Update()
    {
        if (Input.GetKeyDown(testDeathKey))
        {
            TestPlayerDeath();
        }
        
        if (Input.GetKeyDown(spawnEnemyKey))
        {
            TestSpawnEnemy();
        }
        
        if (Input.GetKeyDown(testSaveKey))
        {
            TestSaveSystem();
        }
        
        if (Input.GetKeyDown(testCombatKey))
        {
            TestCombatSystem();
        }
    }
    
    void TestPlayerDeath()
    {
        Debug.Log("🧪 Testing player death system...");
        
        if (PlayerHealth.Instance != null)
        {
            Debug.Log($"Player health before: {PlayerHealth.Instance.GetCurrentHealth()}");
            PlayerHealth.Instance.TakeDamage(9999); // Force death
            Debug.Log("Death test initiated - should return to main menu");
        }
        else
        {
            Debug.LogError("PlayerHealth.Instance is null - system not initialized properly");
        }
    }
    
    void TestSpawnEnemy()
    {
        Debug.Log("🧪 Testing enemy spawn...");
        
        if (SpawnManager.Main != null)
        {
            // Try to find hobbit prefab if not assigned
            if (hobbitEnemyPrefab == null)
            {
                // Try to get from ItemDatabase
                Item hobbitItem = ItemDatabase.Main?.GetCopy("Hobbit");
                if (hobbitItem != null && hobbitItem.prefab != null)
                {
                    hobbitEnemyPrefab = hobbitItem.prefab;
                }
            }
            
            if (hobbitEnemyPrefab != null)
            {
                Vector3 playerPos = transform.position;
                Vector3 spawnPos = playerPos + (Vector3.right * spawnDistance);
                
                GameObject enemy = Instantiate(hobbitEnemyPrefab, spawnPos, Quaternion.identity);
                Debug.Log($"Spawned enemy at {spawnPos}");
                
                // Ensure enemy has proper components
                SimpleHobbitEnemy enemyScript = enemy.GetComponent<SimpleHobbitEnemy>();
                if (enemyScript != null)
                {
                    Debug.Log("Enemy has SimpleHobbitEnemy component ✅");
                }
                else
                {
                    Debug.LogWarning("Enemy missing SimpleHobbitEnemy component ⚠️");
                }
            }
            else
            {
                Debug.LogError("Hobbit enemy prefab not found!");
            }
        }
        else
        {
            Debug.LogError("SpawnManager.Main is null");
        }
    }
    
    void TestSaveSystem()
    {
        Debug.Log("🧪 Testing save system...");
        
        if (TerrainManager.Main != null)
        {
            Debug.Log("TerrainManager found ✅");
            
            // Test save all chunks
            try
            {
                TerrainManager.Main.SaveAll();
                Debug.Log("SaveAll() completed successfully ✅");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SaveAll() failed: {e.Message} ❌");
            }
        }
        else
        {
            Debug.LogError("TerrainManager.Main is null");
        }
        
        if (SaveManager.Main != null)
        {
            Debug.Log("SaveManager found ✅");
            
            try
            {
                SaveManager.Main.Save("test_save");
                Debug.Log("Game save completed successfully ✅");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Game save failed: {e.Message} ❌");
            }
        }
        else
        {
            Debug.LogError("SaveManager.Main is null");
        }
    }
    
    void TestCombatSystem()
    {
        Debug.Log("🧪 Testing combat system...");
        
        if (ContainerManager.Main != null && ContainerManager.Main.playerInventory != null)
        {
            var inventory = ContainerManager.Main.playerInventory;
            
            // Give player a diamond sword for testing
            Item diamondSword = ItemDatabase.Main?.GetCopy("DiamondSword");
            if (diamondSword != null)
            {
                ItemStack swordStack = new ItemStack(diamondSword, 1);
                bool added = inventory.TryAlter(swordStack);
                if (added)
                {
                    Debug.Log("Added DiamondSword to player inventory ✅");
                    Debug.Log("Now attack an enemy to test combat system");
                }
                else
                {
                    Debug.LogWarning("Could not add DiamondSword - inventory full?");
                }
            }
            else
            {
                Debug.LogWarning("DiamondSword item not found in database");
            }
            
            // Check PlayerCombat component
            PlayerCombat playerCombat = FindObjectOfType<PlayerCombat>();
            if (playerCombat != null)
            {
                Debug.Log("PlayerCombat component found ✅");
            }
            else
            {
                Debug.LogError("PlayerCombat component not found ❌");
            }
        }
        else
        {
            Debug.LogError("ContainerManager or playerInventory is null");
        }
    }
    
    void OnGUI()
    {
        // Simple on-screen display for test status
        GUILayout.BeginArea(new Rect(10, 10, 400, 200));
        GUILayout.Label("System Tester - Debug Mode");
        GUILayout.Label($"Player Health: {(PlayerHealth.Instance != null ? PlayerHealth.Instance.GetCurrentHealth().ToString() : "N/A")}");
        GUILayout.Label($"TerrainManager: {(TerrainManager.Main != null ? "✅" : "❌")}");
        GUILayout.Label($"SaveManager: {(SaveManager.Main != null ? "✅" : "❌")}");
        GUILayout.Label($"ContainerManager: {(ContainerManager.Main != null ? "✅" : "❌")}");
        
        if (GUILayout.Button("Test Death System"))
        {
            TestPlayerDeath();
        }
        
        if (GUILayout.Button("Spawn Enemy"))
        {
            TestSpawnEnemy();
        }
        
        GUILayout.EndArea();
    }
}