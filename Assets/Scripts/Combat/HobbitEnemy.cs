using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class HobbitEnemy : MonoBehaviour
{
    [Header("Combat Stats")]
    public int health = 75;
    public int attackDamage = 25;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    
    [Header("AI Behavior")]
    public float detectionRange = 12f;
    public float chaseRange = 20f; // Will give up if player gets too far
    public float wanderRadius = 8f;
    public float wanderWaitTime = 3f;
    
    [Header("Movement")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    
    // Components
    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;
    
    // AI State
    public enum HobbitState { Idle, Wandering, Chasing, Attacking, Dead }
    public HobbitState currentState = HobbitState.Idle;
    
    // Private variables
    private Vector3 startPosition;
    private Vector3 wanderTarget;
    private float lastAttackTime;
    private float wanderTimer;
    private float lastPlayerSightTime;
    
    void Start()
    {
        // Get components
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        // Find player
        if (PlayerManager.Main != null)
            player = PlayerManager.Main.transform;
            
        // Set initial values
        startPosition = transform.position;
        agent.speed = walkSpeed;
        agent.stoppingDistance = attackRange - 0.5f;
        
        // Start with wandering
        SetState(HobbitState.Wandering);
        
        Debug.Log($"Hobbit Enemy initialized at {transform.position}");
    }
    
    void Update()
    {
        if (currentState == HobbitState.Dead || player == null) 
            return;
            
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool canSeePlayer = CanSeePlayer();
        
        if (canSeePlayer)
            lastPlayerSightTime = Time.time;
        
        // State machine
        switch (currentState)
        {
            case HobbitState.Idle:
                HandleIdleState(distanceToPlayer, canSeePlayer);
                break;
                
            case HobbitState.Wandering:
                HandleWanderingState(distanceToPlayer, canSeePlayer);
                break;
                
            case HobbitState.Chasing:
                HandleChasingState(distanceToPlayer, canSeePlayer);
                break;
                
            case HobbitState.Attacking:
                HandleAttackingState(distanceToPlayer);
                break;
        }
        
        // Update animator if available
        UpdateAnimator();
    }
    
    void HandleIdleState(float distanceToPlayer, bool canSeePlayer)
    {
        wanderTimer -= Time.deltaTime;
        
        if (canSeePlayer && distanceToPlayer <= detectionRange)
        {
            SetState(HobbitState.Chasing);
        }
        else if (wanderTimer <= 0)
        {
            SetState(HobbitState.Wandering);
        }
    }
    
    void HandleWanderingState(float distanceToPlayer, bool canSeePlayer)
    {
        // Check for player detection
        if (canSeePlayer && distanceToPlayer <= detectionRange)
        {
            SetState(HobbitState.Chasing);
            return;
        }
        
        // Continue wandering
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            SetRandomWanderTarget();
        }
    }
    
    void HandleChasingState(float distanceToPlayer, bool canSeePlayer)
    {
        // Lost player or too far away
        if (distanceToPlayer > chaseRange || (!canSeePlayer && Time.time - lastPlayerSightTime > 5f))
        {
            SetState(HobbitState.Idle);
            return;
        }
        
        // Close enough to attack
        if (distanceToPlayer <= attackRange)
        {
            SetState(HobbitState.Attacking);
            return;
        }
        
        // Chase the player
        agent.SetDestination(player.position);
    }
    
    void HandleAttackingState(float distanceToPlayer)
    {
        // Player moved away
        if (distanceToPlayer > attackRange + 1f)
        {
            SetState(HobbitState.Chasing);
            return;
        }
        
        // Stop moving and face player
        agent.ResetPath();
        Vector3 lookDirection = (player.position - transform.position).normalized;
        lookDirection.y = 0; // Keep horizontal
        if (lookDirection != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(lookDirection);
        
        // Attack if cooldown is over
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            AttackPlayer();
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
        
        // Play attack animation if available
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }
    
    void SetState(HobbitState newState)
    {
        currentState = newState;
        
        switch (newState)
        {
            case HobbitState.Idle:
                agent.ResetPath();
                agent.speed = walkSpeed;
                wanderTimer = wanderWaitTime;
                break;
                
            case HobbitState.Wandering:
                agent.speed = walkSpeed;
                SetRandomWanderTarget();
                break;
                
            case HobbitState.Chasing:
                agent.speed = runSpeed;
                Debug.Log("Hobbit started chasing player!");
                break;
                
            case HobbitState.Attacking:
                agent.speed = 0;
                break;
        }
    }
    
    void SetRandomWanderTarget()
    {
        Vector2 randomDirection = Random.insideUnitCircle * wanderRadius;
        Vector3 targetPosition = startPosition + new Vector3(randomDirection.x, 0, randomDirection.y);
        
        // Make sure target is on NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, wanderRadius, NavMesh.AllAreas))
        {
            wanderTarget = hit.position;
            agent.SetDestination(wanderTarget);
        }
    }
    
    bool CanSeePlayer()
    {
        if (player == null) return false;
        
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Raycast to check for obstacles
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, directionToPlayer, out hit, distanceToPlayer))
        {
            return hit.transform == player;
        }
        
        return true;
    }
    
    void UpdateAnimator()
    {
        if (animator == null) return;
        
        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);
        animator.SetBool("IsChasing", currentState == HobbitState.Chasing);
    }
    
    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log($"Hobbit took {damage} damage. Health: {health}");
        
        // Force aggro when damaged
        if (currentState != HobbitState.Attacking && currentState != HobbitState.Dead)
        {
            SetState(HobbitState.Chasing);
        }
        
        if (health <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        SetState(HobbitState.Dead);
        agent.ResetPath();
        
        Debug.Log("Hobbit enemy died!");
        
        // Drop loot
        DropLoot();
        
        // Disable components
        agent.enabled = false;
        this.enabled = false;
        
        // Destroy after delay
        Destroy(gameObject, 3f);
    }
    
    void DropLoot()
    {
        // 70% chance to drop items
        if (Random.value < 0.7f)
        {
            string[] possibleDrops = { "Wood", "Stone", "Coal", "Dirt" };
            string dropItem = possibleDrops[Random.Range(0, possibleDrops.Length)];
            int dropAmount = Random.Range(2, 6);
            
            Item item = ItemDatabase.Main.GetCopy(dropItem);
            if (item != null)
            {
                ItemStack itemStack = new ItemStack(item, dropAmount);
                SpawnManager.Main.TryPopulate(Vector3.zero, transform.position, itemStack);
                Debug.Log($"Hobbit dropped {dropAmount} {dropItem}");
            }
        }
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
        
        // Chase range
        Gizmos.color = new Color(1f, 0.5f, 0f); // Orange color
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        
        // Wander area
        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(startPosition, wanderRadius);
        }
    }
}