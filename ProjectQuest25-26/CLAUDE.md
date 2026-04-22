# CLAUDE.md

Guidance for Claude Code development on **Project Quest 25/26** — a 2.5D time-travel courier obstacle course game.

## Project Overview

**Project Quest** is a 2.5D level-based obstacle course game where the player acts as a courier delivering packages across historical and future eras. Each era presents unique environmental hazards, and the package grants a special ability to overcome them. Using the ability reduces end-of-level pay, creating a risk/reward trade-off.

**Engine & Platform:**
- Unity 6000.3.10f1
- Universal Render Pipeline (URP)
- New Input System (com.unity.inputsystem)
- PC Primary (Windows), WebGL optional
- Target Resolution: 1920×1080

## Project Structure (Per spec.md)

```
Assets/
├── _Game/                        # All game systems and assets
│   ├── Scripts/
│   │   ├── Player/              # PlayerController, PlayerAbility, PlayerAnimator
│   │   ├── Abilities/           # IAbility, SpeedBoostAbility, BurnIceAbility, etc.
│   │   ├── Economy/             # PayManager, QuotaManager
│   │   ├── Levels/              # LevelManager, LevelConfig, ShrinkingLevel
│   │   ├── Hazards/             # BaseHazard, IceWall, ElectronicObstacle
│   │   └── UI/                  # HUDController, EndOfLevelUI
│   ├── ScriptableObjects/
│   │   └── Levels/              # LevelConfig assets for each level
│   ├── Prefabs/                 # Game object prefabs
│   ├── Animations/              # Animation clips and controllers
│   ├── Audio/                   # Sound effects and music
│   └── Materials/               # Shader materials and physics materials
├── Scenes/                       # Scene files (MainMenu, Level1_Cave, Level2_IceAge, etc.)
├── Settings/                     # URP rendering profiles
├── TutorialInfo/                 # Sample tutorial (can be removed)
└── Prefabs/                      # Pre-existing prefabs (ice_tile, penguin, etc. — to be organized)

ProjectSettings/                  # Unity project configuration
Packages/                         # Package dependencies
Library/                          # Unity internal cache (gitignored)
Logs/                            # Build logs (gitignored)
```

## Common Development Commands

### Opening the Project

```bash
# Open the project in the Unity Editor
unity -projectPath . &

# Or use the Unity Hub to open the project
```

### Building

```bash
# Build for development (PC/Linux)
# Use the Unity Editor: File > Build Settings > Build

# Standalone builds are located in the Builds/ directory after compilation
```

### Running Tests

```bash
# Run tests via the Unity Editor
# Window > Testing > Test Runner
# Or run from command line:
unity -projectPath . -runTests -testPlatform editmode -logFile -
unity -projectPath . -runTests -testPlatform playmode -logFile -
```

### Scene Management

- **Main Scene**: `Assets/Scenes/SampleScene.unity`
- Add new scenes to `ProjectSettings/EditorBuildSettings.asset` for inclusion in builds

## Architecture Notes

### Rendering Pipeline

The project uses **Universal Render Pipeline (URP)** configured with:
- **PC Renderer**: High-quality settings for desktop platforms (`Assets/Settings/PC_RPAsset.asset`)
- **Mobile Renderer**: Optimized settings for mobile devices (`Assets/Settings/Mobile_Renderer.asset`)

Switch renderers in `ProjectSettings/QualitySettings.asset` or via code using `QualitySettings.SetQualityLevel()`.

### Input System

The project uses the **New Input System** (com.unity.inputsystem 1.18.0):
- Input actions defined in `Assets/InputSystem_Actions.inputactions`
- Create custom input maps in the Input Actions editor (double-click .inputactions file)
- Reference actions in scripts via `InputAction` or the generated code

### Script Organization (Spec-Driven)

All scripts must be placed in `Assets/_Game/Scripts/` organized by domain:

**Player Domain:**
- `Player/PlayerController.cs` — Handles physics movement (WASD, jump, crouch), reads input, applies gravity
- `Player/PlayerAbility.cs` — Ability dispatcher; calls ability.Use() on Q press, updates PayManager
- `Player/PlayerAnimator.cs` — Handles player animation state

**Ability System (Interface-Driven):**
- `Abilities/IAbility.cs` — Interface: `AbilityName`, `CanUse()`, `Use()`, `GetResourcePercent()`
- `Abilities/SpeedBoostAbility.cs` — Level 1 ability
- `Abilities/BurnIceAbility.cs` — Level 2 ability
- `Abilities/RevivalAbility.cs` — Level 3 ability (passive)
- `Abilities/DisableObstaclesAbility.cs` — Level 4 ability
- `Abilities/BlinkAbility.cs` — Level 5 ability

**Economy & Progression:**
- `Economy/PayManager.cs` — Singleton; manages pay, fires `OnPayChanged` event
- `Economy/QuotaManager.cs` — Handles level completion and failure states

**Level System:**
- `Levels/LevelManager.cs` — Scene setup, dependency injection, level start/end
- `Levels/LevelConfig.cs` — ScriptableObject defining basePay, ability, gravity, hazards
- `Levels/ShrinkingLevel.cs` — Implements shrinking safe-space mechanic

**Hazards:**
- `Hazards/BaseHazard.cs` — Base class for environmental hazards
- `Hazards/IceWall.cs` — Burnable ice wall obstacle
- `Hazards/ElectronicObstacle.cs` — Disableable electronic hazard

**UI:**
- `UI/HUDController.cs` — In-game HUD (pay display, ability bar, level name)
- `UI/EndOfLevelUI.cs` — End screen (pay breakdown, buttons)

## MVP Implementation Priority (Per spec.md Section 8)

**CRITICAL (Must complete first):**
1. `PlayerController.cs` — Walk, run, jump, crouch with proper IsGrounded() detection
2. `IAbility` interface + `PlayerAbility.cs` dispatcher
3. `PayManager` singleton with `OnPayChanged` event
4. `LevelConfig` ScriptableObject template
5. `SpeedBoostAbility.cs` — Level 1 ability
6. Level 1 scene (Cave) blockout with ability wired
7. `BurnIceAbility.cs` + `IceWall.cs` hazard
8. Level 2 scene (Ice Age) with `ShrinkingLevel` timer pressure
9. Basic HUD (pay display + ability resource bar)
10. End-of-level UI + `QuotaManager.cs`

**Post-MVP:** Levels 3, 4, 5 and their abilities

## Key Architectural Contracts (MUST implement exactly)

**IAbility Interface:**
```csharp
public interface IAbility
{
    string AbilityName { get; }
    bool CanUse();                    // Returns false when resource exhausted
    void Use();                       // Executes ability effect
    float GetResourcePercent();       // 0–1 for HUD bar
}
```

**PayManager Events:**
```csharp
public class PayManager : MonoBehaviour
{
    public static event Action<int> OnPayChanged;  // Fired on every change
    public static int CurrentPay { get; private set; }
    public static void DeductAbilityUsage(int amount)
    {
        CurrentPay = Mathf.Max(0, CurrentPay - amount);
        OnPayChanged?.Invoke(CurrentPay);
    }
}
```

**No FindObjectOfType() — Use dependency injection via LevelManager instead.**

## Important Files & Configuration

- **ProjectSettings/ProjectSettings.asset**: Core project settings (resolution, quality levels, platforms)
- **ProjectSettings/URPProjectSettings.asset**: URP-specific global settings
- **ProjectSettings/Physics2DSettings.asset**: 2D physics configuration
- **Packages/manifest.json**: Declared package dependencies
- **Packages/packages-lock.json**: Locked package versions (auto-generated)

## Testing

The project includes **com.unity.test-framework 1.6.0**:

1. Create test scripts in `Assets/Tests/` (create this folder if needed)
2. Test classes should inherit from `UnityTest` or use the `[Test]` attribute
3. Use the Test Runner window to run and debug tests
4. Tests run in both Edit Mode and Play Mode

Example test structure:
```csharp
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MyGameTests
{
    [Test]
    public void TestGameLogic()
    {
        // Test code here
        Assert.IsTrue(true);
    }
}
```

## Git Workflow

- **Main branch**: Default development branch
- Recent commits include project initialization with corrected .gitignore
- Standard Unity gitignore is in place (Library/, Temp/, Logs/, etc. are excluded)

**When committing:**
- Avoid committing `Library/`, `Temp/`, `Logs/`, `UserSettings/` directories
- Include `ProjectSettings/`, `Assets/`, `Packages/manifest.json`, and `Packages/packages-lock.json`
- Use meaningful commit messages for scene/prefab changes and script additions

## IDE & Editor Integration

The project is configured for both **Rider** and **Visual Studio**:
- Open scripts with IDE via double-click in Project window
- IDE integration is handled by `com.unity.ide.rider` and `com.unity.ide.visualstudio`
- The project generates `.csproj` and `.sln` files (gitignored, regenerated on load)

## Performance & Quality Settings

The project includes multiple quality levels in `ProjectSettings/QualitySettings.asset`:
- Adjust draw call counts, shadow settings, and LOD bias per quality level
- Use `QualitySettings.GetQualityLevel()` / `SetQualityLevel()` to change quality at runtime

## Coding Conventions (Per spec.md Section 9)

**Naming:**
- Classes: `PascalCase` (e.g., `PlayerController`, `BurnIceAbility`)
- Methods: `PascalCase` (e.g., `Use()`, `CanUse()`, `GetResourcePercent()`)
- Private fields: `_camelCase` with underscore (e.g., `_moveSpeed`, `_isGrounded`)
- Public serialized fields: `camelCase` without prefix (e.g., `moveSpeed`, `basePay`). Use `[SerializeField]` for Inspector exposure.
- Constants: `ALL_CAPS_SNAKE_CASE`

**Documentation:**
- Every public method must have a one-line XML doc comment: `/// <summary>`
- Complex algorithms need inline comments explaining logic
- ScriptableObject fields must have `[Tooltip("...")]` for non-technical users

**Unity Best Practices:**
- **NO FindObjectOfType() at runtime** — use dependency injection instead
- Cache component references in `Awake()` — never call `GetComponent()` in `Update()`
- Physics ops go in `FixedUpdate()`, input reads in `Update()`
- All magic numbers must be extracted to `[SerializeField]` fields with defaults
- Use `Physics2D.OverlapCircle()` for ground detection, not trigger colliders

## Debugging Tips

- Use the **Console** window for runtime logs
- **Debug.Log()**, **Debug.LogWarning()**, **Debug.LogError()** for output
- The Profiler (Window > Analysis > Profiler) shows CPU, GPU, memory usage
- Use **Play Mode Debug** in the Inspector to inspect objects while the game runs
- Physics2D debugging: enable gizmos and constraint visualization in Scene view