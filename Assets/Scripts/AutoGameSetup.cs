using UnityEngine;

[System.Serializable]
public class AutoGameSetup : MonoBehaviour
{
    [Header("Auto Setup Settings")]
    public bool autoSetupOnStart = true;
    public bool debugMessages = true;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupGame();
        }
    }
    
    [ContextMenu("Setup Game Systems")]
    public void SetupGame()
    {
        Log("Starting automatic game setup...");
        
        SetupPlayerSystems();
        SetupHobbitEnemies();
        SetupCraftingSystem();
        SetupHealthSystem();
        SetupHealthUI();
        
        Log("✅ Automatic game setup completed!");
    }
    
    void SetupPlayerSystems()
    {
        Log("Setting up player systems...");
        
        // Find player character
        GameObject player = FindPlayerCharacter();
        if (player == null)
        {
            Log("❌ Could not find player character!");
            return;
        }
        
        // Add PlayerHealth
        if (player.GetComponent<PlayerHealth>() == null)
        {
            PlayerHealth health = player.AddComponent<PlayerHealth>();
            health.maxHealth = 100;
            health.healKey = KeyCode.H;
            Log("✅ Added PlayerHealth component");
        }
        
        // Add PlayerCombat
        if (player.GetComponent<PlayerCombat>() == null)
        {
            PlayerCombat combat = player.AddComponent<PlayerCombat>();
            combat.baseDamage = 15;
            combat.attackRange = 2f;
            combat.attackCooldown = 0.8f;
            combat.attackKey = KeyCode.Mouse0;
            combat.altAttackKey = KeyCode.F;
            Log("✅ Added PlayerCombat component");
        }
        
        // Add crafting system with fallback
        GameObject craftingManager = GameObject.Find("CraftingManager");
        if (craftingManager == null)
        {
            craftingManager = new GameObject("CraftingManager");
            
            // Try advanced UI first, fallback to simple if it fails
            try 
            {
                AutoCraftingUI crafting = craftingManager.AddComponent<AutoCraftingUI>();
                crafting.craftingKey = KeyCode.C;
                crafting.setupUIOnStart = true;
                Log("✅ Created AutoCraftingUI with proper UI");
            }
            catch (System.Exception e)
            {
                Log($"⚠️ AutoCraftingUI failed ({e.Message}), using fallback...");
                DestroyImmediate(craftingManager.GetComponent<AutoCraftingUI>());
                FallbackCraftingUI fallback = craftingManager.AddComponent<FallbackCraftingUI>();
                fallback.craftingKey = KeyCode.C;
                Log("✅ Created FallbackCraftingUI (Console-based)");
            }
        }
    }
    
    void SetupHobbitEnemies()
    {
        Log("Setting up Hobbit enemies...");
        
        // Find all existing Hobbits in the scene
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        int hobbitCount = 0;
        
        foreach (GameObject obj in allObjects)
        {
            // Check if this looks like a Hobbit (by name or has WorldObject with NPC item)
            bool isHobbit = obj.name.ToLower().Contains("hobbit");
            bool hasNPCItem = false;
            
            // Check if this GameObject has a WorldObject component with an NPC item
            WorldObject worldObj = obj.GetComponent<WorldObject>();
            if (worldObj != null && worldObj.itemStack != null && worldObj.itemStack.item != null)
            {
                hasNPCItem = worldObj.itemStack.item is NPC;
            }
            
            if (isHobbit || hasNPCItem)
            {
                SetupSingleHobbit(obj);
                hobbitCount++;
            }
        }
        
        Log($"✅ Setup {hobbitCount} Hobbit enemies");
        
        if (hobbitCount > 0)
        {
            Log("Hobbit enemy locations:");
            foreach (GameObject obj in allObjects)
            {
                bool isHobbit = obj.name.ToLower().Contains("hobbit");
                bool hasNPCItem = false;
                
                WorldObject worldObj = obj.GetComponent<WorldObject>();
                if (worldObj != null && worldObj.itemStack != null && worldObj.itemStack.item != null)
                {
                    hasNPCItem = worldObj.itemStack.item is NPC;
                }
                
                if ((isHobbit || hasNPCItem) && obj.GetComponent<SimpleHobbitEnemy>() != null)
                {
                    Log($"  - {obj.name} at position {obj.transform.position}");
                }
            }
        }
        
        // If no Hobbits found, create one for testing
        if (hobbitCount == 0)
        {
            CreateTestHobbit();
            Log("⚠️ No existing Hobbits found - created test Hobbit");
            Log("💡 Hobbits will also spawn automatically from chunks as they load");
        }
        
        // Start periodic enemy check to catch dynamically spawned NPCs
        StartCoroutine(PeriodicEnemySetup());
    }
    
    void SetupSingleHobbit(GameObject hobbit)
    {
        // Remove old enemy components
        HobbitEnemy oldEnemy = hobbit.GetComponent<HobbitEnemy>();
        if (oldEnemy != null)
        {
            DestroyImmediate(oldEnemy);
        }
        
        // Add Rigidbody if missing
        Rigidbody rb = hobbit.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = hobbit.AddComponent<Rigidbody>();
            rb.freezeRotation = true;
            rb.mass = 1f;
        }
        
        // Add Collider if missing
        if (hobbit.GetComponent<Collider>() == null)
        {
            CapsuleCollider col = hobbit.AddComponent<CapsuleCollider>();
            col.height = 2f;
            col.radius = 0.5f;
            col.center = new Vector3(0, 1f, 0);
        }
        
        // Add SimpleHobbitEnemy
        if (hobbit.GetComponent<SimpleHobbitEnemy>() == null)
        {
            SimpleHobbitEnemy enemy = hobbit.AddComponent<SimpleHobbitEnemy>();
            enemy.health = 75;
            enemy.attackDamage = 20;
            enemy.attackRange = 1.5f;
            enemy.detectionRange = 8f;
            enemy.moveSpeed = 3f;
        }
        
        Log($"✅ Setup Hobbit: {hobbit.name}");
    }
    
    void CreateTestHobbit()
    {
        Log("Creating test Hobbit for combat testing...");
        
        GameObject testHobbit = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        testHobbit.name = "Test Hobbit Enemy";
        testHobbit.transform.position = new Vector3(10, 1, 10);
        testHobbit.transform.localScale = new Vector3(1, 1, 1);
        
        // Make it look different
        Renderer renderer = testHobbit.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.green;
        }
        
        SetupSingleHobbit(testHobbit);
        
        Log("✅ Created test Hobbit at position (10, 1, 10)");
        
        // Log player position for distance reference
        GameObject player = FindPlayerCharacter();
        if (player != null)
        {
            float distance = Vector3.Distance(testHobbit.transform.position, player.transform.position);
            Log($"Player is at {player.transform.position}, distance to test Hobbit: {distance:F1} units");
        }
    }
    
    void SetupCraftingSystem()
    {
        Log("Setting up crafting system...");
        
        // Make sure CraftingSystem singleton exists
        if (CraftingSystem.Main == null)
        {
            GameObject craftingSystem = new GameObject("CraftingSystem");
            craftingSystem.AddComponent<CraftingSystem>();
            Log("✅ Created CraftingSystem singleton");
        }
        
        // Make sure ItemDatabase is setup
        if (ItemDatabase.Main == null)
        {
            Log("⚠️ ItemDatabase not found - make sure it exists in scene");
        }
    }
    
    void SetupHealthSystem()
    {
        Log("Setting up health UI system...");
        
        // Try to find existing UI elements for health
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Log("⚠️ No Canvas found - health UI will use debug messages");
            return;
        }
        
        Log("✅ Health system ready (using debug output)");
    }
    
    void SetupHealthUI()
    {
        Log("Setting up player health UI...");
        
        // Create or find health UI manager
        GameObject healthUIManager = GameObject.Find("HealthUIManager");
        if (healthUIManager == null)
        {
            healthUIManager = new GameObject("HealthUIManager");
            PlayerHealthUI healthUI = healthUIManager.AddComponent<PlayerHealthUI>();
            healthUI.setupUIOnStart = true;
            Log("✅ Created PlayerHealthUI system");
        }
        else
        {
            Log("✅ Found existing PlayerHealthUI system");
        }
    }
    
    GameObject FindPlayerCharacter()
    {
        // Try multiple ways to find the player
        
        // Method 1: Look for PlayerManager
        if (PlayerManager.Main != null)
        {
            return PlayerManager.Main.gameObject;
        }
        
        // Method 2: Look for PlayerController
        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null)
        {
            return pc.gameObject;
        }
        
        // Method 3: Look for object with "Player" in name
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.ToLower().Contains("player"))
            {
                return obj;
            }
        }
        
        // Method 4: Look for object with VoxelPlacer (likely the player)
        VoxelPlacer vp = FindObjectOfType<VoxelPlacer>();
        if (vp != null)
        {
            return vp.gameObject;
        }
        
        return null;
    }
    
    void Log(string message)
    {
        if (debugMessages)
        {
            Debug.Log($"[AutoGameSetup] {message}");
        }
    }
    
    // Manual setup buttons for Inspector
    [ContextMenu("Setup Player Only")]
    public void SetupPlayerOnly()
    {
        SetupPlayerSystems();
    }
    
    [ContextMenu("Setup Hobbits Only")]
    public void SetupHobbitsOnly()
    {
        SetupHobbitEnemies();
    }
    
    [ContextMenu("Create Test Hobbit")]
    public void CreateTestHobbitManual()
    {
        CreateTestHobbit();
    }
    
    // Periodic check for dynamically spawned NPCs that need enemy setup
    System.Collections.IEnumerator PeriodicEnemySetup()
    {
        yield return new WaitForSeconds(5f); // Wait 5 seconds before first check
        
        while (true)
        {
            int newEnemiesSetup = 0;
            
            // Find all GameObjects that might be NPCs but don't have enemy components
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                // Check if this looks like a Hobbit NPC that needs setup
                bool isHobbit = obj.name.ToLower().Contains("hobbit");
                bool hasNPCItem = false;
                
                WorldObject worldObj = obj.GetComponent<WorldObject>();
                if (worldObj != null && worldObj.itemStack != null && worldObj.itemStack.item != null)
                {
                    hasNPCItem = worldObj.itemStack.item is NPC;
                }
                
                if ((isHobbit || hasNPCItem) && 
                    obj.GetComponent<SimpleHobbitEnemy>() == null && obj.GetComponent<HobbitEnemy>() == null)
                {
                    // Skip if it's an item (not spawned as GameObject yet)
                    if (worldObj != null)
                    {
                        SetupSingleHobbit(obj);
                        newEnemiesSetup++;
                        Log($"🔄 Auto-setup new Hobbit enemy: {obj.name}");
                    }
                }
            }
            
            if (newEnemiesSetup > 0)
            {
                Log($"✅ Periodic check setup {newEnemiesSetup} new Hobbit enemies");
            }
            
            yield return new WaitForSeconds(10f); // Check every 10 seconds
        }
    }
}