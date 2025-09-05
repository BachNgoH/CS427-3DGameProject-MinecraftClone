using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

public class ChunkObjectSpawner : Singleton<ChunkObjectSpawner>
{

    Queue<Chunk> chunksToItemize = new Queue<Chunk>();

    int maxItemizeChunksInFrame = 10;

    public void Enqueue(Chunk chunk)
    {
        chunksToItemize.Enqueue(chunk);
    }

    public void Itemize()
    {
        StartCoroutine(ProcessItemization());
    }

    IEnumerator ProcessItemization()
    {
        int itemizedChunks = 0;
        while (chunksToItemize.Count > 0)
        {
            if (itemizedChunks >= maxItemizeChunksInFrame)
                yield return null;
            Chunk chunk = chunksToItemize.Dequeue();
            ChunkRandomSpawn(chunk);
            chunk.Itemized = true;
            itemizedChunks++;
        }
    }

    // Assumes all items are snapped on center
    public void ChunkRandomSpawn(Chunk chunk)
    {
        HashSet<int3> occupiedGround = new HashSet<int3>();

        // Generate trees first (they need more space)
        GenerateTrees(chunk, occupiedGround);

        // Spawn other items
        RandomSpawnItem(ItemDatabase.Main.GetCopy("Hobbit"), 3, 7, chunk.chunkPosition, chunk.groundVoxels, occupiedGround);
    }

    public void GenerateTrees(Chunk chunk, HashSet<int3> occupiedGround)
    {
        // Generate 1-3 trees per chunk randomly
        int treeCount = UnityEngine.Random.Range(1, 4);
        int treesSpawned = 0;
        int attempts = 0;
        
        while (treesSpawned < treeCount && attempts < 20)
        {
            attempts++;
            
            // Pick random ground voxel
            int randomIndex = UnityEngine.Random.Range(0, chunk.groundVoxels.Length);
            int3 groundPos = chunk.groundVoxels[randomIndex];
            
            // Check if area is clear (3x3 around tree)
            bool canPlaceTree = true;
            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    int3 checkPos = groundPos + new int3(x, 0, z);
                    if (occupiedGround.Contains(checkPos))
                    {
                        canPlaceTree = false;
                        break;
                    }
                }
                if (!canPlaceTree) break;
            }
            
            if (canPlaceTree)
            {
                // Generate tree at this position
                GenerateTree(chunk, groundPos, occupiedGround);
                treesSpawned++;
            }
        }
    }
    
    void GenerateTree(Chunk chunk, int3 groundPos, HashSet<int3> occupiedGround)
    {
        // Tree parameters
        int treeHeight = UnityEngine.Random.Range(4, 8); // Random height 4-7
        Vector3Int chunkPos = chunk.chunkPosition;
        
        // Generate trunk
        for (int y = 1; y <= treeHeight; y++)
        {
            Vector3Int trunkPos = new Vector3Int(groundPos.x, groundPos.y + y, groundPos.z);
            Vector3 worldPos = VoxelUtil.GridToWorld(trunkPos, chunkPos, Consts.ChunkSize);
            TerrainManager.Main.SetVoxel(worldPos, Voxel.Wood);
        }
        
        // Generate leaves (simple spherical pattern)
        int leavesRadius = 2;
        for (int x = -leavesRadius; x <= leavesRadius; x++)
        {
            for (int y = -1; y <= 2; y++) // Leaves around top of tree
            {
                for (int z = -leavesRadius; z <= leavesRadius; z++)
                {
                    // Skip center trunk area and create spherical shape
                    float distance = Mathf.Sqrt(x * x + y * y + z * z);
                    if (distance <= leavesRadius && !(x == 0 && z == 0 && y <= 0))
                    {
                        // Add some randomness to leaves
                        if (UnityEngine.Random.value > 0.3f)
                        {
                            Vector3Int leafPos = new Vector3Int(
                                groundPos.x + x, 
                                groundPos.y + treeHeight + y, 
                                groundPos.z + z
                            );
                            Vector3 worldPos = VoxelUtil.GridToWorld(leafPos, chunkPos, Consts.ChunkSize);
                            TerrainManager.Main.SetVoxel(worldPos, Voxel.Leaves);
                        }
                    }
                }
            }
        }
        
        // Mark 3x3 area as occupied
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                occupiedGround.Add(groundPos + new int3(x, 0, z));
            }
        }
    }

    public void RandomSpawnItem(Item item, int minAmount, int maxAmount, Vector3Int chunkPos, int3[] groundVoxels, HashSet<int3> occupiedGround, int minStack = 1, int maxStack = 1)
    {
        int amountToSpawn = UnityEngine.Random.Range(minAmount, maxAmount + 1);

        int spawnedAmount = 0;
        while (spawnedAmount < amountToSpawn)
        {
            int r = UnityEngine.Random.Range(0, groundVoxels.Length);
            if (occupiedGround.Contains(groundVoxels[r]))
                continue;

            Vector3Int gridPos = new Vector3Int(groundVoxels[r].x, groundVoxels[r].y, groundVoxels[r].z);
            Vector3 worldPos = VoxelUtil.GridToWorld(gridPos, chunkPos, Consts.ChunkSize);
            worldPos += ItemUtil.Corner2TopMid;

            // Voxel isn't occupied, will try to spawn!
            int stack = UnityEngine.Random.Range(minStack, maxStack + 1);
            ItemStack itemStack = new ItemStack(item, stack);

            if (SpawnManager.Main.TryPopulate(Vector3.zero, worldPos, itemStack))
            {
                spawnedAmount++;
                occupiedGround.Add(groundVoxels[r]);
            }
        }
    }
}
