using UnityEngine;
using System.Collections.Generic;
using Priority_Queue;
using Unity.Mathematics;
using System.Collections;

public class SpawnManager : Singleton<SpawnManager>
{
    new Transform transform;

    private void Awake()
    {
        transform = GetComponent<Transform>();
    }

    public bool TryPopulate(Vector3 rayDir, Vector3 worldPosition, ItemStack itemStack)
    {
        PlacementData placementData = new PlacementData();

        placementData = ItemUtil.Snap(rayDir, worldPosition, itemStack.item.volume, itemStack.item.pivot, itemStack.item.snapType);

        // Check if it's solid here?
        bool canPlace = itemStack.item.ValidatePopulation(placementData.volumeCenter);
        if (canPlace)
        {
            GameObject g = Instantiate(itemStack.item.prefab, transform);
            WorldObject wi = g.GetComponent<WorldObject>();
            wi.Initialize(placementData.position, placementData.rotation, itemStack.item.prefab.transform.localScale, itemStack);
            TerrainManager.Main.AddItem(worldPosition, wi);

            // Auto-setup Hobbit enemies when spawned
            SetupHobbitEnemyIfNeeded(g, itemStack.item);

            return true;
        }
        return false;
    }

    public bool TrySpawnItem(Vector3 rayDir, Vector3 worldPosition, string slotID)
    {
        ItemStack itemStack = ContainerManager.Main.PeekSlot(slotID);
        return TrySpawnOneItem(rayDir, worldPosition, itemStack, slotID);
    }

    // MAKE A DROP FUNCTION HERE

    // Raydir = Vector3.zero gets random vector
    public bool TrySpawnOneItem(Vector3 rayDir, Vector3 worldPosition, ItemStack itemStack, string itemSlot)
    {
        PlacementData placementData = new PlacementData();

        placementData = ItemUtil.Snap(rayDir, worldPosition, itemStack.item.volume, itemStack.item.pivot, itemStack.item.snapType);

        // Check if it's solid here?
        bool canPlace = itemStack.item.ValidatePlacement(placementData.volumeCenter);
        if (canPlace)
        {
            GameObject g = Instantiate(itemStack.item.prefab, transform);
            WorldObject wi = g.GetComponent<WorldObject>();
            ItemStack newItemStack = new ItemStack(itemStack.item, 1);
            wi.Initialize(placementData.position, placementData.rotation, itemStack.item.prefab.transform.localScale, newItemStack);

            TerrainManager.Main.AddItem(worldPosition, wi);

            // Auto-setup Hobbit enemies when spawned
            SetupHobbitEnemyIfNeeded(g, itemStack.item);

            return true;
        }
        return false;
    }

    public void SpawnByLoad(WorldObjectData WorldObjectData, out WorldObject WorldObject)
    {
        GameObject prefab = ItemDatabase.Main.GetPrefab(WorldObjectData.itemStackData.itemData.itemName);
        GameObject g = Instantiate(prefab, transform);
        WorldObject wi = g.GetComponent<WorldObject>();
        wi.SetData(WorldObjectData);
        WorldObject = wi;
        
        // Auto-setup Hobbit enemies when loaded from save
        if (WorldObjectData.itemStackData?.itemData != null)
        {
            Item item = ItemDatabase.Main.GetCopy(WorldObjectData.itemStackData.itemData.itemName);
            SetupHobbitEnemyIfNeeded(g, item);
        }
    }
    
    private void SetupHobbitEnemyIfNeeded(GameObject spawnedObject, Item item)
    {
        if (item == null) return;
        
        // Check if this is a Hobbit NPC
        if (item.name.ToLower().Contains("hobbit") || item is NPC)
        {
            Debug.Log($"[SpawnManager] Setting up Hobbit enemy: {spawnedObject.name}");
            
            // Add Rigidbody if missing
            Rigidbody rb = spawnedObject.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = spawnedObject.AddComponent<Rigidbody>();
                rb.freezeRotation = true;
                rb.mass = 1f;
            }
            
            // Add Collider if missing
            if (spawnedObject.GetComponent<Collider>() == null)
            {
                CapsuleCollider col = spawnedObject.AddComponent<CapsuleCollider>();
                col.height = 2f;
                col.radius = 0.5f;
                col.center = new Vector3(0, 1f, 0);
            }
            
            // Remove old enemy components if any
            SimpleHobbitEnemy oldEnemy = spawnedObject.GetComponent<SimpleHobbitEnemy>();
            if (oldEnemy != null)
            {
                DestroyImmediate(oldEnemy);
            }
            
            HobbitEnemy oldEnemy2 = spawnedObject.GetComponent<HobbitEnemy>();
            if (oldEnemy2 != null)
            {
                DestroyImmediate(oldEnemy2);
            }
            
            // Add SimpleHobbitEnemy component
            if (spawnedObject.GetComponent<SimpleHobbitEnemy>() == null)
            {
                SimpleHobbitEnemy enemy = spawnedObject.AddComponent<SimpleHobbitEnemy>();
                enemy.health = 75;
                enemy.attackDamage = 20;
                enemy.attackRange = 1.5f;
                enemy.detectionRange = 8f;
                enemy.moveSpeed = 3f;
                
                Debug.Log($"✅ [SpawnManager] Setup Hobbit enemy: {spawnedObject.name} at {spawnedObject.transform.position}");
            }
        }
    }
}
