# Mine3DCraft Unity - CS427 3D Game Development Project

A comprehensive voxel-based sandbox game built in Unity, featuring infinite procedural world generation, combat system, crafting mechanics, and survival gameplay. This project was developed as part of CS427 - 3D Game Development course.

## 👥 Team Members
- **22125006** - Ngo Hoang Bach
- **22125086** - Ngo Tri Si  
- **22125074** - Le Du Phu
- **22125083** - Phan Minh Quang

Demo Video: [Demo](https://drive.google.com/file/d/1K-4qRNrNseYOESxu-jkxZK5YQmvpUw3Q/view?usp=sharing)

<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/74787b46-d43e-4ef5-a0b9-26d5c7cc5194" />

## :sparkles: Features

### :mountain: Terrain Generation
- **Infinite procedural world generation** using 3D Simplex noise
- **Realistic ore distribution** with depth-based resource spawning
- **Multiple block types**: Stone, Dirt, Sand, Coal, Iron, Gold, Diamond, Wood, Leaves
- **Procedural tree generation** with natural oak-style trees

### :crossed_swords: Combat System
- **Real-time melee combat** with raycast-based targeting
- **AI-controlled enemies** (Hobbit enemies with state machine behavior)
- **Weapon progression**: Fists → Stone Sword → Iron Sword → Diamond Sword
- **Physics-based knockback** and damage effects
- **Health system** with healing potions

<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/5df80f98-0d30-45b1-bedb-f5110764902a" />


### :hammer_and_wrench: Crafting System
- **Recipe-based crafting** with automatic ingredient detection
- **Dynamic crafting UI** with real-time inventory scanning
- **Multiple recipes**: Tools, weapons, building materials, consumables
- **Auto-generated item thumbnails** and descriptions

<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/46ac4717-1192-4fa7-9a65-f16236942b0a" />


### :package: Advanced Inventory System
- **64-slot inventory** (10 hotbar + 54 backpack slots)
- **Intelligent item stacking** with material-appropriate stack limits
- **Equipment management** with weapon detection and damage calculation
- **Visual item icons** with proper texture assignments

### :floppy_disk: Save/Load System
- **Binary chunk serialization** for optimal performance
- **Persistent world state** with automatic save on death
- **Incremental saving** to prevent performance hitches
- **World object persistence** including enemies and items

### :robot: AI System
- **State machine-based enemy AI** (Idle → Chasing → Attacking)
- **Physics-based movement** with collision detection
- **Loot drops** with configurable drop rates
- **Player detection** with multiple fallback systems

## :fire: Basic Setup:

Just clone the project and open it in Unity version 2022.3.7f1 LTS or newer, it should also run in any 2020+ version with no issues.
All the packages needed for the project will import automatically when opening it for the first time.

## :video_game: Controls
- **Movement**: WASD keys + Mouse look
- **Jump**: Spacebar
- **Attack**: Left Mouse Click or F key
- **Place/Break Blocks**: Right Mouse Click (place), Left Mouse Click (break)
- **Inventory**: I key to open/close inventory
- **Crafting**: C key to open/close crafting menu
- **Heal**: H key to use healing potion
- **Debug Commands**: F9-F12 keys for testing systems (see SystemTester.cs)

## :desktop_computer: Technical Architecture

### Procedural Generation System
The world generation system creates infinite, varied terrain using multiple noise layers:

> **3D Simplex Noise** → **Depth-based Ore Distribution** → **Block Data Generation** → **Greedy Meshing** → **Tree Generation** → **Enemy Spawning**

- **Realistic Mining Progression**: Diamonds spawn deep (Y: 0-15), Coal near surface (Y: 20-45)
- **Biome Support**: Different noise parameters create terrain variation
- **Performance Optimized**: Unity Job System for multithreaded chunk generation
- **Memory Efficient**: 32x64x32 chunk system with automatic cleanup

### Combat & AI Architecture
The combat system features state-machine driven AI with physics integration:

```
Enemy AI: IDLE (wandering) → CHASING (follow player) → ATTACKING (damage + knockback)
```

- **Raycast-based Combat**: Accurate hit detection from camera center
- **Damage Calculation**: Weapon-specific multipliers (Stone Sword: 1.5x, Diamond: 3.0x)
- **Physics Knockback**: Impulse-based enemy displacement with stun effects
- **Health Management**: Player health system with potion healing integration

### Item & Crafting Systems
Advanced item management with automatic generation and recipe-based crafting:

**ItemDatabase Architecture**:
- **Auto-generated Block Items**: All voxel types automatically become craftable items
- **Equipment System**: Weapons with damage stats and equipment types
- **Dynamic Item Creation**: Missing crafting result items created at runtime
- **Thumbnail Assignment**: Automatic icon loading from sprite resources

**Crafting Implementation**:
- **Recipe Engine**: Ingredient validation with automatic UI updates
- **Dynamic UI Generation**: Crafting menu created at runtime with scroll support
- **Real-time Inventory Scanning**: Live ingredient availability checking
- **Error Handling**: Graceful failure with user feedback


### Data Persistence & Performance
Advanced save system with incremental loading and error recovery:

**Save System Architecture**:
- **Binary Chunk Serialization**: Compressed voxel data with WorldObject persistence
- **Incremental Saving**: Distance-based chunk management (max 5 chunks/frame)
- **Memory Management**: 18-27 chunk limit with automatic cleanup
- **Death Recovery**: Automatic save before returning to main menu
- **Error Handling**: Graceful recovery from corrupted save files

**Performance Metrics**:
- **Target FPS**: 60+ at 1920x1080 resolution
- **Chunk Generation**: 5-20ms per chunk (multithreaded)
- **Memory per Chunk**: 50-200KB mesh data
- **World Size**: Virtually unlimited (2³¹ theoretical limit)
- **Render Distance**: 5-10 chunks (160-320 blocks)

### Advanced Inventory System
Enhanced inventory management with automatic item generation and visual feedback:

<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/e935eb80-3e3a-4281-8225-7e3377dfed77" />

**Features**:
- **64 Total Slots**: 10 hotbar slots (H0-H9) + 54 backpack slots (B0-B53)
- **Intelligent Stacking**: Material-appropriate stack limits (Diamond: 8, Stone: 64)
- **Visual Item Icons**: All block items automatically have proper thumbnails
- **Equipment Integration**: Weapons automatically detected for combat damage
- **Advanced Interactions**: Item swap, take half, eyedropper functionality

## :mortar_board: Academic Project Information

This project demonstrates advanced 3D game development concepts including:
- **Procedural Content Generation** with multi-layered noise functions
- **Performance Optimization** using Unity Job System and memory management
- **Component-based Architecture** with modular, reusable systems  
- **State Machine Design** for AI behavior and game flow
- **Data Serialization** with binary formats for optimal performance
- **UI/UX Design** with dynamic interface generation

**Technical Documentation**: See `TECHNICAL_DOCUMENTATION.md` for detailed system analysis and implementation details (85 pages of comprehensive technical documentation).

## :chart_with_upward_trend: Development Statistics
- **~15,000 lines of code** across 12 major systems
- **50+ C# classes** with clear separation of concerns
- **Unity 2022.3.7f1 LTS** with modern development practices
- **Binary serialization** for world persistence
- **Multithreaded rendering** with greedy meshing optimization
 
## :page_facing_up: Credits & Acknowledgments

**Original Voxel Engine**: Special thanks to [bbtarzan12](https://github.com/bbtarzan12/Unity-Procedural-Voxel-Terrain) for the foundational procedural voxel terrain system.

**CS427 Course**: 3D Game Development - Advanced game programming concepts and Unity development.

**Team Contributions**:
- **System Architecture & Combat**: Advanced AI state machines and combat mechanics
- **Crafting & Item Systems**: Recipe-based crafting with dynamic UI generation
- **Performance Optimization**: Multithreaded chunk generation and memory management
- **Save/Load Systems**: Binary serialization and world persistence

## :rocket: Future Enhancements
- **Multiplayer Support**: Client-server architecture with chunk synchronization
- **Biome System**: Multiple terrain types with unique generation patterns
- **Advanced Graphics**: Shader improvements and particle effects
- **Mod Support**: Script loading and asset replacement systems

---
*Minecraft4Unity - CS427 3D Game Development Project*  
*Unity 2022.3.7f1 LTS | C# | ~15,000 LOC*
