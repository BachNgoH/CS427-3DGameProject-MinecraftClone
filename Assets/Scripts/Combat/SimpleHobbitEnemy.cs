using UnityEngine;

public class SimpleHobbitEnemy : MonoBehaviour
{
    [Header("Combat Stats")]
    public int health = 75;
    public int attackDamage = 20;
    public float attackRange = 1.5f;
    public float attackCooldown = 2f;
    
    [Header("AI Behavior")]
    public float detectionRange = 8f;
    public float moveSpeed = 3f;
    public float wanderRadius = 5f;
    
    // Components
    private Transform player;
    private Rigidbody rb;
    
    // AI State
    public enum State { Idle, Chasing, Attacking }
    public State currentState = State.Idle;
    
    // Private variables
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float lastAttackTime;
    private float idleTimer = 0f;
    
    void Start()
    {
        // Get components
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.freezeRotation = true; // Prevent tipping over
        }
        
        // Find player with multiple fallback methods
        if (PlayerManager.Main != null)
        {
            player = PlayerManager.Main.transform;
            Debug.Log($"Hobbit found player via PlayerManager: {player.name}");
        }
        else
        {
            PlayerController pc = FindObjectOfType<PlayerController>();
            if (pc != null)
            {
                player = pc.transform;
                Debug.Log($"Hobbit found player via PlayerController: {player.name}");
            }
            else
            {
                // Try finding by VoxelPlacer (likely the player)
                VoxelPlacer vp = FindObjectOfType<VoxelPlacer>();
                if (vp != null)
                {
                    player = vp.transform;
                    Debug.Log($"Hobbit found player via VoxelPlacer: {player.name}");
                }
                else
                {
                    // Try finding GameObject with "Player" in name
                    GameObject[] allObjects = FindObjectsOfType<GameObject>();
                    foreach (GameObject obj in allObjects)
                    {
                        if (obj.name.ToLower().Contains("player"))
                        {
                            player = obj.transform;
                            Debug.Log($"Hobbit found player by name search: {player.name}");
                            break;
                        }
                    }
                }
            }
        }
        
        if (player == null)
        {
            Debug.LogError($"Hobbit {gameObject.name} could not find player!");
        }
            
        // Set initial values
        startPosition = transform.position;
        targetPosition = transform.position;
        
        Debug.Log($"Simple Hobbit Enemy initialized at {transform.position} with detection range {detectionRange}");
    }
    
    void Update()
    {
        if (player == null || health <= 0) 
            return;
            
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Debug every few seconds to avoid spam
        if (Time.time % 3f < 0.1f)
        {
            Debug.Log($"Hobbit {gameObject.name}: State={currentState}, Distance={distanceToPlayer:F1}, DetectionRange={detectionRange}");
        }
        
        // State machine
        switch (currentState)
        {
            case State.Idle:
                HandleIdleState(distanceToPlayer);
                break;
                
            case State.Chasing:
                HandleChasingState(distanceToPlayer);
                break;
                
            case State.Attacking:
                HandleAttackingState(distanceToPlayer);
                break;
        }
        
        // Move towards target
        MoveTowardsTarget();
    }
    
    void HandleIdleState(float distanceToPlayer)
    {
        // Check for player detection
        if (distanceToPlayer <= detectionRange)
        {
            SetState(State.Chasing);
            return;
        }
        
        // Wander around
        idleTimer += Time.deltaTime;
        if (idleTimer >= 3f) // Change direction every 3 seconds
        {
            SetRandomWanderTarget();
            idleTimer = 0f;
        }
    }
    
    void HandleChasingState(float distanceToPlayer)
    {
        // Lost player or too far away
        if (distanceToPlayer > detectionRange * 2f)
        {
            SetState(State.Idle);
            return;
        }
        
        // Close enough to attack
        if (distanceToPlayer <= attackRange)
        {
            SetState(State.Attacking);
            return;
        }
        
        // Chase the player
        targetPosition = player.position;
    }
    
    void HandleAttackingState(float distanceToPlayer)
    {
        // Player moved away
        if (distanceToPlayer > attackRange + 1f)
        {
            SetState(State.Chasing);
            return;
        }
        
        // Stop moving and face player
        targetPosition = transform.position;
        LookAtPlayer();
        
        // Attack if cooldown is over
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            AttackPlayer();
        }
    }
    
    void SetState(State newState)
    {
        currentState = newState;
        
        switch (newState)
        {
            case State.Idle:
                SetRandomWanderTarget();
                break;
                
            case State.Chasing:
                Debug.Log("Hobbit started chasing player!");
                break;
                
            case State.Attacking:
                break;
        }
    }
    
    void SetRandomWanderTarget()
    {
        Vector2 randomDirection = Random.insideUnitCircle * wanderRadius;
        targetPosition = startPosition + new Vector3(randomDirection.x, 0, randomDirection.y);
    }
    
    void MoveTowardsTarget()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // Keep horizontal movement only
        
        if (direction.magnitude > 0.1f && currentState != State.Attacking)
        {
            rb.MovePosition(transform.position + direction * moveSpeed * Time.deltaTime);
            
            // Face movement direction
            if (direction != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(direction);
        }
    }
    
    void LookAtPlayer()
    {
        if (player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(direction);
        }
    }
    
    void AttackPlayer()
    {
        lastAttackTime = Time.time;
        
        // Deal damage to player
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.TakeDamage(attackDamage);
            Debug.Log($"Hobbit attacked player for {attackDamage} damage!");
        }
        
        // Visual attack effect (you can add animations here)
        Debug.Log("Hobbit attacks!");
    }
    
    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log($"Hobbit took {damage} damage. Health: {health}");
        
        // Force aggro when damaged
        if (currentState == State.Idle)
        {
            SetState(State.Chasing);
        }
        
        // Visual damage effect
        StartCoroutine(DamageFlash());
        
        if (health <= 0)
        {
            Die();
        }
    }
    
    System.Collections.IEnumerator DamageFlash()
    {
        // Enhanced damage flash effect that works with various shaders
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            Material mat = renderer.material;
            Color originalColor = Color.white;
            bool hasColorProperty = false;
            string colorPropertyName = "";
            
            // Check for common color property names
            string[] commonColorProperties = { "_Color", "_BaseColor", "_MainColor", "_TintColor", "_Albedo" };
            
            foreach (string propName in commonColorProperties)
            {
                if (mat.HasProperty(propName))
                {
                    originalColor = mat.GetColor(propName);
                    colorPropertyName = propName;
                    hasColorProperty = true;
                    break;
                }
            }
            
            if (hasColorProperty)
            {
                // Traditional color flash
                mat.SetColor(colorPropertyName, Color.red);
                yield return new WaitForSeconds(0.1f);
                mat.SetColor(colorPropertyName, originalColor);
            }
            else
            {
                // Alternative flash effect using scale animation
                Vector3 originalScale = transform.localScale;
                float flashDuration = 0.1f;
                float timer = 0f;
                
                while (timer < flashDuration)
                {
                    timer += Time.deltaTime;
                    float scaleMultiplier = 1f + Mathf.Sin(timer / flashDuration * Mathf.PI * 10f) * 0.1f;
                    transform.localScale = originalScale * scaleMultiplier;
                    yield return null;
                }
                
                transform.localScale = originalScale;
                Debug.Log($"Used scale flash for {gameObject.name} (custom shader detected)");
            }
        }
        else
        {
            // Final fallback: visibility blink
            yield return StartCoroutine(VisibilityBlinkFlash());
        }
    }
    
    System.Collections.IEnumerator VisibilityBlinkFlash()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            // Rapid blink effect
            for (int i = 0; i < 3; i++)
            {
                renderer.enabled = false;
                yield return new WaitForSeconds(0.05f);
                renderer.enabled = true;
                yield return new WaitForSeconds(0.05f);
            }
            Debug.Log($"Used visibility blink for {gameObject.name}");
        }
    }
    
    void CleanupFromChunk()
    {
        // Remove this enemy's WorldObject from any chunk's worldItems list
        WorldObject worldObj = GetComponent<WorldObject>();
        if (worldObj != null && TerrainManager.Main != null)
        {
            try
            {
                // Find which chunk contains this WorldObject and remove it
                Vector3 worldPos = transform.position;
                TerrainManager.Main.RemoveItem(worldPos, worldObj);
                Debug.Log($"Removed {gameObject.name} from chunk worldItems list");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not remove {gameObject.name} from chunk: {e.Message}");
            }
        }
    }
    
    void Die()
    {
        Debug.Log("Hobbit enemy died!");
        
        // Disable enemy AI and physics to prevent any unwanted interactions
        this.enabled = false;
        
        // Stop any ongoing movement
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true; // Make it kinematic to prevent physics interference
        }
        
        // Disable collider to prevent further collisions
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        // Drop loot
        DropLoot();
        
        // Remove from chunk's worldItems list to prevent save errors
        CleanupFromChunk();
        
        // Death effect
        StartCoroutine(DeathEffect());
    }
    
    void DropLoot()
    {
        // 70% chance to drop items
        if (Random.value < 0.7f)
        {
            string[] possibleDrops = { "Wood", "Stone", "Coal", "Dirt" };
            string dropItem = possibleDrops[Random.Range(0, possibleDrops.Length)];
            int dropAmount = Random.Range(1, 4);
            
            Item item = ItemDatabase.Main?.GetCopy(dropItem);
            if (item != null && item.prefab != null)
            {
                ItemStack itemStack = new ItemStack(item, dropAmount);
                // Use a random downward direction for loot drop
                Vector3 dropDirection = new Vector3(
                    Random.Range(-1f, 1f), 
                    -0.5f, 
                    Random.Range(-1f, 1f)
                ).normalized;
                
                SpawnManager.Main.TryPopulate(dropDirection, transform.position, itemStack);
                Debug.Log($"Hobbit dropped {dropAmount} {dropItem}");
            }
            else
            {
                // Fallback: Add directly to player inventory if spawning fails
                Debug.Log($"⚠️ Couldn't spawn {dropItem}, adding to player inventory instead");
                if (ContainerManager.Main?.playerInventory != null && item != null)
                {
                    ItemStack itemStack = new ItemStack(item, dropAmount);
                    bool added = ContainerManager.Main.playerInventory.TryAlter(itemStack);
                    if (added)
                    {
                        Debug.Log($"✅ Added {dropAmount} {dropItem} to player inventory");
                    }
                    else
                    {
                        Debug.Log($"❌ Player inventory full, {dropItem} lost");
                    }
                }
            }
        }
    }
    
    System.Collections.IEnumerator DeathEffect()
    {
        // Simple death animation
        float timer = 0f;
        Vector3 originalScale = transform.localScale;
        
        while (timer < 1f)
        {
            timer += Time.deltaTime * 2f;
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, timer);
            yield return null;
        }
        
        Destroy(gameObject);
    }
    
    // Visual debug
    void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Wander area
        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(startPosition, wanderRadius);
        }
    }
}