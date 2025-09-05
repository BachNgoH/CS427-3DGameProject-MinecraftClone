using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public int baseDamage = 15; // Bare hands damage
    public float attackRange = 2f;
    public float attackCooldown = 0.8f;
    public KeyCode attackKey = KeyCode.Mouse0; // Left mouse click
    public KeyCode altAttackKey = KeyCode.F; // Alternative attack key
    
    [Header("Weapon Damage Multipliers")]
    public float stoneSwordMultiplier = 1.5f;
    public float ironSwordMultiplier = 2.0f;
    public float diamondSwordMultiplier = 3.0f;
    
    [Header("Knockback Settings")]
    public float knockbackForce = 5f;
    public float knockbackUpwardForce = 2f;
    
    private float lastAttackTime;
    private Camera playerCamera;
    
    void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindObjectOfType<Camera>();
    }
    
    void Update()
    {
        // Check for attack input
        if ((Input.GetKeyDown(attackKey) || Input.GetKeyDown(altAttackKey)) && Time.time - lastAttackTime >= attackCooldown)
        {
            TryAttack();
        }
    }
    
    void TryAttack()
    {
        lastAttackTime = Time.time;
        
        // Ensure camera is available
        if (playerCamera == null)
        {
            Debug.LogWarning("[PlayerCombat] Player camera is null, cannot attack");
            return;
        }
        
        // Raycast from camera center
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, attackRange))
        {
            // Check if we hit an enemy
            SimpleHobbitEnemy simpleEnemy = hit.collider.GetComponent<SimpleHobbitEnemy>();
            if (simpleEnemy != null)
            {
                int damage = CalculateDamage();
                simpleEnemy.TakeDamage(damage);
                
                // Apply knockback
                ApplyKnockback(simpleEnemy.gameObject, hit.point);
                
                Debug.Log($"Attacked {simpleEnemy.name} for {damage} damage!");
                
                // Add screen shake or hit effects here
                return;
            }
            
            // Also check for old HobbitEnemy (backward compatibility)
            HobbitEnemy enemy = hit.collider.GetComponent<HobbitEnemy>();
            if (enemy != null)
            {
                int damage = CalculateDamage();
                enemy.TakeDamage(damage);
                
                // Apply knockback
                ApplyKnockback(enemy.gameObject, hit.point);
                
                Debug.Log($"Attacked {enemy.name} for {damage} damage!");
                
                // Add screen shake or hit effects here
                return;
            }
            
            // Check for other enemies (if you add more later)
            Enemy genericEnemy = hit.collider.GetComponent<Enemy>();
            if (genericEnemy != null)
            {
                int damage = CalculateDamage();
                genericEnemy.TakeDamage(damage);
                
                // Apply knockback
                ApplyKnockback(genericEnemy.gameObject, hit.point);
                
                Debug.Log($"Attacked {genericEnemy.name} for {damage} damage!");
                return;
            }
        }
        
        Debug.Log("Attack missed!");
    }
    
    int CalculateDamage()
    {
        try
        {
            int damage = baseDamage;
            
            // Check equipped weapon
            string equippedWeapon = GetEquippedWeapon();
            
            if (!string.IsNullOrEmpty(equippedWeapon))
            {
                switch (equippedWeapon)
                {
                    case "StoneSword":
                        damage = Mathf.RoundToInt(baseDamage * stoneSwordMultiplier);
                        break;
                    case "IronSword":
                        damage = Mathf.RoundToInt(baseDamage * ironSwordMultiplier);
                        break;
                    case "DiamondSword":
                        damage = Mathf.RoundToInt(baseDamage * diamondSwordMultiplier);
                        break;
                    default:
                        damage = baseDamage; // Fists or unknown weapon
                        break;
                }
            }
            
            return damage;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PlayerCombat] Error in CalculateDamage: {e.Message}");
            return baseDamage; // Fallback to base damage
        }
    }
    
    string GetEquippedWeapon()
    {
        try
        {
            // Check hotbar for equipped weapon
            if (ContainerManager.Main != null && ContainerManager.Main.playerInventory != null)
            {
                var inventory = ContainerManager.Main.playerInventory;
                
                // Check if SlotIDs is available and not null
                if (inventory.SlotIDs != null)
                {
                    // Check hotbar slots (H0, H1, H2, etc.) for equipped weapon
                    foreach (string slotID in inventory.SlotIDs)
                    {
                        if (slotID != null && slotID.StartsWith("H")) // Hotbar slots
                        {
                            ItemStack itemStack = inventory.Peek(slotID);
                            if (itemStack != null && itemStack.item != null && !string.IsNullOrEmpty(itemStack.item.name))
                            {
                                string itemName = itemStack.item.name;
                                if (itemName.Contains("Sword"))
                                {
                                    return itemName;
                                }
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("[PlayerCombat] Inventory SlotIDs is null");
                }
            }
            else
            {
                Debug.LogWarning("[PlayerCombat] ContainerManager or playerInventory is null");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PlayerCombat] Error in GetEquippedWeapon: {e.Message}");
        }
        
        return "Fists"; // No weapon equipped or error occurred
    }
    
    void ApplyKnockback(GameObject target, Vector3 hitPoint)
    {
        // Safety check: Don't apply knockback to player
        if (target == gameObject || target.GetComponent<PlayerController>() != null)
        {
            Debug.LogWarning($"[PlayerCombat] Attempted to apply knockback to player - skipping");
            return;
        }
        
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            // Calculate knockback direction (away from player)
            Vector3 knockbackDirection = (target.transform.position - transform.position).normalized;
            
            // Ensure we have a valid direction
            if (knockbackDirection.magnitude < 0.1f)
            {
                knockbackDirection = Vector3.forward; // Fallback direction
            }
            
            // Add some upward force
            knockbackDirection.y = knockbackUpwardForce / knockbackForce;
            
            // Apply the knockback force
            Vector3 knockbackVelocity = knockbackDirection * knockbackForce;
            targetRb.AddForce(knockbackVelocity, ForceMode.Impulse);
            
            Debug.Log($"Applied knockback to {target.name}: {knockbackVelocity}");
            
            // Optional: Add a brief stun effect
            StartCoroutine(KnockbackStun(target, 0.3f));
        }
    }
    
    System.Collections.IEnumerator KnockbackStun(GameObject target, float stunDuration)
    {
        // Safety check: Don't stun player
        if (target == gameObject || target.GetComponent<PlayerController>() != null)
        {
            yield break;
        }
        
        // Temporarily disable enemy movement during knockback
        SimpleHobbitEnemy enemy = target.GetComponent<SimpleHobbitEnemy>();
        if (enemy != null && enemy.enabled) // Check if enemy is active
        {
            enemy.enabled = false; // Disable AI temporarily
            yield return new WaitForSeconds(stunDuration);
            if (enemy != null && enemy.health > 0) // Check if enemy still exists and is alive
                enemy.enabled = true; // Re-enable AI
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}