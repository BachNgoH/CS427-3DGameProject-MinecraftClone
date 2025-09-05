# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Unity Requirements

- Unity version: 2022.3.7f1 LTS or newer
- Compatible with any Unity 2020+ version
- Packages import automatically when opening for the first time

## Project Structure

This is a Unity C# project implementing a Minecraft-like voxel world with optimized chunk-based terrain generation.

### Core Systems Architecture

**Terrain Generation Pipeline:**
- `NoiseGenerator` - Creates 3D Simplex noise for terrain height maps
- `Chunk` - Individual terrain chunks with voxel data (`Assets/Scripts/VoxelSys/Chunk.cs`)
- `TerrainManager` - Manages chunk loading/unloading around player (`Assets/Scripts/TerrainManager.cs`)
- `ChunkMeshBuilder` - Generates optimized meshes using greedy meshing algorithm
- `ChunkColliderBuilder` - Creates physics colliders for terrain
- `ChunkObjectSpawner` - Places non-voxel objects on terrain

**Key Configuration Constants** (`Assets/Scripts/Utils/Main/Consts.cs`):
- Chunk size: 32x64x32 voxels (horizontal x vertical x horizontal)
- Chunk spawn radius: 2x1 chunks around player
- Max chunks generated per frame: 1

**Data Persistence System:**
- Player data: Single file with position, stats, and inventory
- World data: Two files per chunk (terrain voxels + object placement data)
- Autosave triggers when new chunks generate
- Files stored as byte arrays for terrain, serialized objects for entities

**Inventory System:**
- Item stacking with type-defined limits
- Hotbar + backpack UI similar to Minecraft
- Drag-drop, half-stack, and eyedropper mechanics

### Main Scenes
- `MainMenu.unity` - Main menu and save/load interface
- `VoxelWorld.unity` - Primary gameplay scene with terrain generation

### Key Entry Points
- `GameController` - Global game state and version info
- `PlayerManager` - Player character coordination
- `SaveManager` - Save/load system coordination
- `SceneStarter` - Scene initialization logic

## Development Notes

The project uses Unity's Job System for performance-critical operations like mesh generation. The codebase follows a component-based architecture with singleton managers for core systems.

The voxel system implements greedy meshing for optimization, reducing triangle count significantly compared to naive cube rendering.