using UnityEngine;

public class VoxelPlacer : MonoBehaviour
{
    public GameObject blockSelection;

    public bool highlightVoxel = false;

    public bool canPlace = true;

    public bool canGrab = true;

    void Update()
    {
        // Place selector on voxel player is currently looking
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, Consts.InteractionDistance, Consts.VoxelMask))
        {
            if (highlightVoxel && blockSelection)
            {
                PlacementData posRot = ItemUtil.Snap(ray.direction, hit.point + ray.direction * 0.01f, Vector3Int.one, ItemPivot.MidCenter, SnapType.Center);
                blockSelection.transform.position = posRot.position;
                blockSelection.SetActive(true);
            }

            // Debug info
            Debug.Log($"Raycast hit: {hit.collider.name} on layer {hit.collider.gameObject.layer}");
            
            // Set voxel if necessary
            if (!GameController.IsPaused)
            {
                // Place block: Left mouse OR 'B' key
                if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.B)) && canPlace)
                {
                    Debug.Log("Placing block at: " + hit.point);
                    TerrainManager.Main.SetVoxel(hit.point - ray.direction * 0.01f, Voxel.Stone);
                }

                // Break block: Right mouse OR 'N' key (easier for Mac)
                if ((Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.N)) && canGrab)
                {
                    Debug.Log("Attempting to break block at: " + hit.point);
                    Voxel voxel;
                    TerrainManager.Main.GetVoxel(hit.point + ray.direction * 0.01f, out voxel);
                    if (voxel != Voxel.Air)
                    {
                        Debug.Log($"Breaking voxel: {voxel}");
                        TerrainManager.Main.SetVoxel(hit.point + ray.direction * 0.01f, Voxel.Air);
                        ItemStack itemStack = new ItemStack(ItemDatabase.Main.GetCopy(voxel.ToString()));
                        ContainerManager.Main.TryAlterContainer(itemStack);
                        Debug.Log("Block broken and added to inventory!");
                    }
                    else
                    {
                        Debug.Log("No block to break (Air voxel)");
                    }
                }
            }
        }
        else
        {
            blockSelection?.SetActive(false);
        }
    }
}