using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int health = 50;
    public int damage = 20;
    public float moveSpeed = 3f;
    public float attackRange = 2f;
    public float attackCooldown = 2f;
    
    [Header("AI Settings")]
    public float detectionRange = 10f;
    public float wanderRadius = 5f;
    
    private Transform player;
    private Vector3 startPosition;
    private Vector3 wanderTarget;
    private float lastAttackTime;
    private bool isChasing = false;
    
    enum EnemyState { Wandering, Chasing, Attacking }
    private EnemyState currentState = EnemyState.Wandering;
    
    void Start()
    {
        player = PlayerManager.Main?.transform;
        startPosition = transform.position;
        SetNewWanderTarget();
    }
    
    void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        switch (currentState)
        {
            case EnemyState.Wandering:
                Wander();
                if (distanceToPlayer <= detectionRange)
                {
                    currentState = EnemyState.Chasing;
                    Debug.Log($"{gameObject.name} detected player!");
                }
                break;
                
            case EnemyState.Chasing:
                ChasePlayer();
                if (distanceToPlayer > detectionRange * 1.5f) // Lose interest
                {
                    currentState = EnemyState.Wandering;
                    SetNewWanderTarget();
                }
                else if (distanceToPlayer <= attackRange)
                {
                    currentState = EnemyState.Attacking;
                }
                break;
                
            case EnemyState.Attacking:
                AttackPlayer();
                if (distanceToPlayer > attackRange)
                {
                    currentState = EnemyState.Chasing;
                }
                break;
        }
        
        // Always face movement direction
        if (GetComponent<Rigidbody>().velocity.magnitude > 0.1f)
        {
            transform.LookAt(transform.position + GetComponent<Rigidbody>().velocity.normalized);
        }
    }
    
    void Wander()
    {
        Vector3 direction = (wanderTarget - transform.position).normalized;
        GetComponent<Rigidbody>().MovePosition(transform.position + direction * moveSpeed * Time.deltaTime);
        
        if (Vector3.Distance(transform.position, wanderTarget) < 1f)
        {
            SetNewWanderTarget();
        }
    }
    
    void SetNewWanderTarget()
    {
        Vector2 randomDirection = Random.insideUnitCircle * wanderRadius;
        wanderTarget = startPosition + new Vector3(randomDirection.x, 0, randomDirection.y);
    }
    
    void ChasePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        GetComponent<Rigidbody>().MovePosition(transform.position + direction * moveSpeed * Time.deltaTime);
    }
    
    void AttackPlayer()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            // Attack the player
            if (PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.TakeDamage(damage);
                Debug.Log($"{gameObject.name} attacked player for {damage} damage!");
            }
            lastAttackTime = Time.time;
        }
    }
    
    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        Debug.Log($"{gameObject.name} took {damageAmount} damage. Health: {health}");
        
        // Force chase state when damaged
        currentState = EnemyState.Chasing;
        
        if (health <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        Debug.Log($"{gameObject.name} died!");
        
        // Drop random loot
        DropLoot();
        
        Destroy(gameObject);
    }
    
    void DropLoot()
    {
        // Random chance to drop items
        if (Random.value < 0.5f) // 50% chance
        {
            string[] possibleDrops = { "Stone", "Wood", "Coal", "Iron" };
            string dropItem = possibleDrops[Random.Range(0, possibleDrops.Length)];
            int dropAmount = Random.Range(1, 4);
            
            // Create item stack and try to spawn it
            Item item = ItemDatabase.Main.GetCopy(dropItem);
            if (item != null)
            {
                ItemStack itemStack = new ItemStack(item, dropAmount);
                SpawnManager.Main.TryPopulate(Vector3.zero, transform.position, itemStack);
                Debug.Log($"Enemy dropped {dropAmount} {dropItem}");
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw attack range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw wander area
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(startPosition, wanderRadius);
    }
}