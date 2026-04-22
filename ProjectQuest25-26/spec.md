**PROJECT QUEST 25/26**

Claude Code Specification

Version 1.0  |  Generated March 2026

| Document Purpose This document is a complete technical specification for the development of Project Quest — a 2.5D time-travel courier obstacle course game. It is intended to be passed directly to Claude Code as a priming context, enabling autonomous or semi-autonomous implementation of the project. It covers: architecture, file structure, implementation priorities, system contracts, and level-by-level feature specifications. |
| :---- |

# **1\. Project Overview**

## **1.1 Vision Statement**

Project Quest is a 2.5D level-based obstacle course game in which the player acts as a courier delivering packages across distinct eras of time. Each era presents unique environmental hazards, and the package delivered within that level grants a special ability to overcome them. The central tension is a risk/reward trade-off: using the package's ability reduces its value, directly lowering the player's end-of-level pay.

## **1.2 Unique Selling Points**

* **Each level is a self-contained historical or future era with mechanics derived from its setting (e.g. low gravity on Mars, melting ice in the Ice Age).**Time-Travel Theming: 

* **The package grants power, but using it costs you. Every activation of Q reduces end-of-level pay, creating meaningful player decisions.**Risk/Reward Economy: 

* **Available safe space shrinks over time, forcing the player to keep moving and preventing camping.**Dynamic Level Pressure: 

## **1.3 Target Platform & Engine**

| Property | Value |
| ----- | ----- |
| Engine | Unity 2022 LTS (2D/2.5D pipeline) |
| Platform | PC (Windows primary), WebGL export optional |
| Render Pipeline | Universal Render Pipeline (URP) |
| Target Resolution | 1920×1080 (16:9), scalable |
| Input System | Unity New Input System (Keyboard \+ Gamepad ready) |

# **2\. Architecture & File Structure**

## **2.1 Recommended Folder Structure**

Claude Code should create and maintain the following directory layout inside the Unity project's Assets/ folder:

Assets/  
├── \_Game/  
│   ├── Scripts/  
│   │   ├── Player/  
│   │   │   ├── PlayerController.cs  
│   │   │   ├── PlayerAbility.cs  
│   │   │   └── PlayerAnimator.cs  
│   │   ├── Abilities/  
│   │   │   ├── IAbility.cs              (interface)  
│   │   │   ├── SpeedBoostAbility.cs  
│   │   │   ├── BurnIceAbility.cs  
│   │   │   ├── RevivalAbility.cs  
│   │   │   ├── DisableObstaclesAbility.cs  
│   │   │   └── BlinkAbility.cs  
│   │   ├── Economy/  
│   │   │   ├── PayManager.cs  
│   │   │   └── QuotaManager.cs  
│   │   ├── Levels/  
│   │   │   ├── LevelManager.cs  
│   │   │   ├── LevelConfig.cs           (ScriptableObject)  
│   │   │   └── ShrinkingLevel.cs  
│   │   ├── Hazards/  
│   │   │   ├── BaseHazard.cs  
│   │   │   ├── IceWall.cs  
│   │   │   └── ElectronicObstacle.cs  
│   │   └── UI/  
│   │       ├── HUDController.cs  
│   │       └── EndOfLevelUI.cs  
│   ├── ScriptableObjects/  
│   │   └── Levels/  
│   │       ├── Level1\_Cave.asset  
│   │       └── Level2\_IceAge.asset  
│   ├── Prefabs/  
│   ├── Animations/  
│   ├── Audio/  
│   └── Materials/  
└── Scenes/  
    ├── MainMenu.unity  
    ├── Level1\_Cave.unity  
    └── Level2\_IceAge.unity

## **2.2 Core Architectural Principles**

* **Each level's configuration (base pay, ability type, hazard list, gravity modifier) is defined in a LevelConfig ScriptableObject. LevelManager reads this at runtime.**ScriptableObject-Driven Levels: 

* **All abilities implement IAbility, allowing PlayerAbility.cs to call Use() without knowing the concrete type. This is the open/closed principle: adding a new level never requires editing existing ability code.**Interface-Based Abilities: 

* **PayManager exposes a static C\# event (OnPayChanged). UI components subscribe to this event rather than polling, keeping systems decoupled.**Event-Driven Economy: 

* **No script handles more than one domain. PlayerController handles physics/input only; it never touches pay or ability logic directly.**Single Responsibility: 

## **2.3 Key Interfaces & Contracts**

The following contracts must be implemented exactly as specified. Claude Code should treat these as the API boundary between systems.

### **IAbility Interface**

public interface IAbility  
{  
    string AbilityName { get; }  
    bool CanUse();        // Returns false when resource is exhausted  
    void Use();           // Executes the ability effect  
    float GetResourcePercent();  // 0–1, used by HUD bar  
}

### **PayManager Events**

public class PayManager : MonoBehaviour  
{  
    public static event Action\<int\> OnPayChanged;  // fires on every change  
    public static int CurrentPay { get; private set; }

    public static void DeductAbilityUsage(int amount)  
    {  
        CurrentPay \= Mathf.Max(0, CurrentPay \- amount);  
        OnPayChanged?.Invoke(CurrentPay);  
    }  
}

# **3\. Player Systems**

## **3.1 PlayerController.cs — Movement**

The PlayerController handles all physics-based movement. It must read input via Unity's new Input System and apply forces/velocity to a Rigidbody2D.

| Action | Input | Behaviour |
| ----- | ----- | ----- |
| Walk / Run | WASD / Arrows | Horizontal velocity applied to Rigidbody2D. Use a configurable moveSpeed float. |
| Jump | Spacebar | Apply upward impulse force. Only allowed when IsGrounded() returns true. Gravity scale \= 2f for snappy feel. |
| Crouch | Left Ctrl | Scale collider height to 50%. Reduce moveSpeed by 40%. |
| Gravity Override | LevelConfig | LevelConfig.gravityModifier multiplies Physics2D.gravity at level start. Default \= 1.0f. Mars level \= 0.3f. |

### **Ground Detection**

Use an OverlapCircle or BoxCast below the player's feet to determine IsGrounded. Do not use trigger colliders for this; they produce inconsistent results with moving platforms.

private bool IsGrounded()  
{  
    return Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);  
}

## **3.2 PlayerAbility.cs — Ability Dispatch**

PlayerAbility holds a reference to the current level's IAbility implementation. When the player presses Q, it calls ability.Use() and informs PayManager.

* Assigned at level start by LevelManager via dependency injection (no FindObjectOfType).

* Calls CanUse() before Use(). If false, play a 'no charge' audio cue and return.

* Each Use() triggers PayManager.DeductAbilityUsage(levelConfig.abilityPayCost).

# **4\. Ability Specifications**

Each ability below must be implemented as a class in Assets/\_Game/Scripts/Abilities/ and must implement IAbility. The LevelConfig ScriptableObject will hold an instance reference.

## **4.1 SpeedBoostAbility (Level 1 — Caveman)**

| Property | Specification |  |
| ----- | ----- | :---- |
| Effect | Multiply player moveSpeed by 2.5x for boostDuration seconds |  |
| Duration | boostDuration \= 2.0f seconds (configurable via Inspector) |  |
| Resource | No hard limit. Each use costs pay. CanUse() always returns true. |  |
| Visual Cue | Particle trail on player while active. Camera FOV \+5 during boost. |  |
| Pay Cost | 15 per use (default; configurable in LevelConfig) |  |

## **4.2 BurnIceAbility (Level 2 — Ice Age)**

| Property | Specification |  |
| ----- | ----- | :---- |
| Effect | Destroy all IceWall objects within burnRadius (default 5f) units of the player |  |
| Resource | Level-wide burn time (burnTimeRemaining). Shared pool, not per-use cooldown. |  |
| Drain Rate | Subtract burnDrainPerUse (= 8f seconds) from burnTimeRemaining per press |  |
| Failure Condition | If burnTimeRemaining \<= 0, CanUse() returns false and fire is extinguished visually |  |
| HUD | GetResourcePercent() \= burnTimeRemaining / burnTimeTotal |  |
| Pay Cost | 20 per use |  |

## **4.3 RevivalAbility (Level 3 — Ancient Civilisation)**

| Property | Specification |  |
| ----- | ----- | :---- |
| Type | Passive — does not require Q press. Triggers automatically on death. |  |
| Charges | 1 charge total. Once used, the ability is consumed. |  |
| Effect | On player death event, restore player to last safe position with full health. Deduct pay. |  |
| CanUse() | Returns true only if charges \> 0 |  |
| Weight Penalty | LevelConfig.gravityModifier \= 1.0f but moveSpeed multiplied by 0.7f |  |
| Pay Cost | 50 (deducted automatically on revival) |  |

## **4.4 DisableObstaclesAbility (Level 4 — Modern Era)**

| Property | Specification |  |
| ----- | ----- | :---- |
| Effect | Temporarily disable all ElectronicObstacle objects within disableRadius for disableDuration seconds |  |
| Battery | Phone battery: starts at 100%. Each use drains batteryDrainPercent (default 25%). |  |
| Failure Condition | If battery reaches 0%, the level is immediately failed (QuotaManager.FailLevel() called) |  |
| CanUse() | Returns true if batteryPercent \> 0 |  |
| HUD | GetResourcePercent() \= batteryPercent / 100f |  |
| Pay Cost | 25 per use |  |

## **4.5 BlinkAbility (Level 5 — Future / Mars)**

| Property | Specification |  |
| ----- | ----- | :---- |
| Effect | Teleport player in movement direction by (blinkDistance ± randomOffset) units |  |
| Random Offset | randomOffset \= Random.Range(-1.5f, 1.5f) applied to blink distance |  |
| Charges | Fixed charges (default 5). CanUse() returns charges \> 0\. |  |
| Low Gravity | This level sets LevelConfig.gravityModifier \= 0.3f — jump is floatier |  |
| Blink Distance | blinkDistance \= 6f units (configurable) |  |
| Pay Cost | 30 per use |  |

# **5\. Economy & Progression System**

## **5.1 PayManager**

PayManager is a singleton MonoBehaviour that persists across scene loads (DontDestroyOnLoad). It initialises CurrentPay to the level's basePay value at level start.

| Method / Property | Description |  |
| ----- | ----- | :---- |
| CurrentPay (int) | Current pay amount. Read-only externally. |  |
| InitialiseLevel(int basePay) | Called by LevelManager at level start. Sets CurrentPay \= basePay. |  |
| DeductAbilityUsage(int cost) | Subtracts cost from CurrentPay. Fires OnPayChanged event. Clamps to 0\. |  |
| FinaliseLevel() | Called on level completion. Saves CurrentPay to PlayerPrefs as total earnings. |  |
| OnPayChanged (event) | Action\<int\> fired on every change. HUD subscribes to this. |  |

## **5.2 QuotaManager**

The quota is simply whether the player reaches the level exit. There is no minimum pay required. However, certain ability exhaustions (phone battery \= 0%) trigger an immediate fail via QuotaManager.FailLevel().

* FailLevel(): Show failure UI, stop gameplay, offer retry.

* CompleteLevel(): Trigger end-of-level pay display, unlock next level in PlayerPrefs.

# **6\. Level Specifications**

## **6.1 Level 1 — Caveman / Dinosaurs**

| Property | Value |  |
| ----- | ----- | :---- |
| Scene File | Level1\_Cave.unity |  |
| Package | The Wheel |  |
| Ability | SpeedBoostAbility |  |
| Base Pay | 200 |  |
| Gravity Modifier | 1.0f (standard) |  |
| Pressure Mechanic | Rolling boulder object pursues player; fixed velocity increases over time. |  |
| Hazards | Static rocks, logs (jump over). Dinosaur patrol enemies (avoid or outrun). |  |
| MVP Priority | YES — required for MVP |  |

## **6.2 Level 2 — Ice Age**

| Property | Value |  |
| ----- | ----- | :---- |
| Scene File | Level2\_IceAge.unity |  |
| Package | Fire / Matches |  |
| Ability | BurnIceAbility |  |
| Base Pay | 250 |  |
| Gravity Modifier | 1.0f (standard) |  |
| Pressure Mechanic | Level geometry slowly collapses from the left (ShrinkingLevel.cs). Player must keep moving. |  |
| Hazards | IceWall objects block path — must be burned. Slippery floor (reduce friction on ice tiles). |  |
| Failure Condition | Burn time exhausted: fire extinguished, remaining ice walls become permanent. |  |
| MVP Priority | YES — required for MVP |  |

## **6.3 Level 3 — Ancient Civilisation**

| Property | Value |  |
| ----- | ----- | :---- |
| Scene File | Level3\_AncientCiv.unity |  |
| Package | Gold Totem |  |
| Ability | RevivalAbility (passive) |  |
| Base Pay | 300 |  |
| Gravity Modifier | 1.0f |  |
| Speed Penalty | 0.7x move speed from package weight |  |
| Hazards | Spike traps, collapsing floors, patrolling guards. |  |
| MVP Priority | No — post-MVP |  |

## **6.4 Level 4 — Modern Era**

| Property | Value |  |
| ----- | ----- | :---- |
| Scene File | Level4\_Modern.unity |  |
| Package | Smartphone |  |
| Ability | DisableObstaclesAbility |  |
| Base Pay | 350 |  |
| Gravity Modifier | 1.0f |  |
| Hazards | ElectronicObstacle objects (laser grids, security cameras, automated turrets). |  |
| Failure Condition | Phone battery reaches 0%. Instant level fail. |  |
| MVP Priority | No — post-MVP |  |

## **6.5 Level 5 — Future / Mars**

| Property | Value |  |
| ----- | ----- | :---- |
| Scene File | Level5\_Mars.unity |  |
| Package | Warp Drive |  |
| Ability | BlinkAbility |  |
| Base Pay | 400 |  |
| Gravity Modifier | 0.3f — jumps are high and floaty |  |
| Hazards | Unstable platforms, vacuum zones (instant death), alien sentries. |  |
| Special | Blink distance has ±1.5f random offset. Player must account for imprecision. |  |
| MVP Priority | No — post-MVP |  |

# **7\. UI Requirements**

## **7.1 In-Game HUD**

| Element | Specification |  |
| ----- | ----- | :---- |
| Pay Display | Top-left. Text: '£{CurrentPay}'. Updates via OnPayChanged event subscription. |  |
| Ability Resource Bar | Top-right. Filled bar 0–100%. Calls ability.GetResourcePercent() each frame. |  |
| Ability Name | Below resource bar. Displays IAbility.AbilityName. |  |
| Level Name / Era | Top-centre. Static text set from LevelConfig.levelName. |  |
| Pressure Indicator | (Optional) Visual warning (screen vignette) when shrink/timer pressure is high. |  |

## **7.2 End-of-Level Screen**

* Show 'Package Delivered\!' header.

* Display final pay amount with animation (count-up tween).

* Show breakdown: Base Pay, minus Ability Uses (e.g. '−15 x 3 uses \= −45').

* Buttons: 'Next Level' (if unlocked) and 'Retry'.

## **7.3 Failure Screen**

* Show reason for failure (e.g. 'Phone battery died\!', 'Crushed by glacier\!').

* Show current pay at time of failure.

* Buttons: 'Retry Level' and 'Main Menu'.

# **8\. MVP Implementation Scope**

Claude Code should implement the MVP first, in the following priority order. Do not start on post-MVP features until all MVP tasks pass basic testing.

| \# | Task | Priority | Status |
| ----- | ----- | ----- | ----- |
| 1 | PlayerController.cs — walk, run, jump, crouch | MVP CRITICAL | TODO |
| 2 | IAbility interface and PlayerAbility dispatcher | MVP CRITICAL | TODO |
| 3 | PayManager singleton with events | MVP CRITICAL | TODO |
| 4 | LevelConfig ScriptableObject definition | MVP CRITICAL | TODO |
| 5 | SpeedBoostAbility.cs | MVP CRITICAL | TODO |
| 6 | Level 1 scene (Cave) — basic blockout \+ ability wired up | MVP CRITICAL | TODO |
| 7 | BurnIceAbility.cs \+ IceWall hazard | MVP CRITICAL | TODO |
| 8 | Level 2 scene (Ice Age) — burn mechanic \+ timer pressure | MVP CRITICAL | TODO |
| 9 | Basic HUD (pay \+ ability bar) | MVP CRITICAL | TODO |
| 10 | End-of-level UI \+ QuotaManager | MVP CRITICAL | TODO |
| 11 | RevivalAbility \+ Level 3 | Post-MVP | TODO |
| 12 | DisableObstaclesAbility \+ Level 4 | Post-MVP | TODO |
| 13 | BlinkAbility \+ Level 5 (low gravity) | Post-MVP | TODO |
| 14 | Shop system | Future Scope | TODO |
| 15 | Story / cutscenes | Future Scope | TODO |

# **9\. Coding Conventions for Claude Code**

## **9.1 Naming**

* Classes: PascalCase (PlayerController, BurnIceAbility).

* Methods: PascalCase (Use(), CanUse(), GetResourcePercent()).

* Private fields: camelCase with underscore prefix (\_moveSpeed, \_isGrounded).

* Public serialised fields: camelCase without prefix (moveSpeed, basePay). Use \[SerializeField\] for Inspector-exposed private fields.

* Constants: ALL\_CAPS\_SNAKE\_CASE.

## **9.2 Comments & Documentation**

* Every public method must have a one-line XML doc comment (/// \<summary\>).

* Complex algorithms (e.g. blink randomisation) must have inline comments explaining the maths.

* ScriptableObject fields must have \[Tooltip("..")\] attributes so non-technical team members can use the Inspector.

## **9.3 Unity Best Practices**

* Never use FindObjectOfType at runtime. Use dependency injection via LevelManager.

* Cache component references in Awake() — never call GetComponent() in Update().

* Physics operations (AddForce, velocity mutation) must occur in FixedUpdate().

* Input reads must occur in Update() and stored to a field for FixedUpdate() to consume.

* All magic numbers must be extracted to \[SerializeField\] private fields with sensible defaults.

# **10\. Future Scope Reference**

The following features are explicitly out of MVP scope but are documented here so that Claude Code's architecture does not preclude them.

## **10.1 Shop System**

* A between-level menu where accumulated pay is spent.

* Players can purchase up to 2 additional abilities (bound to E, Z, X, C keys).

* Passive upgrades (e.g. increased base move speed, extra jump height).

* Architecture note: PlayerAbility.cs should support a List\<IAbility\> of up to 3 abilities from day one, even if only index 0 (Q) is used in MVP.

## **10.2 Visual Polish**

* Particle systems for all ability activations.

* Screen shake on death/failure.

* Post-processing: bloom, chromatic aberration per-era.

## **10.3 Story / Narrative**

* Pre-level cutscenes explaining the delivery premise per era.

* Text dialogue system (no voice acting required for initial scope).

# **11\. Instructions to Claude Code**

| How to use this document 1\. Read this entire document before writing any code. 2\. Begin with Section 8 (MVP scope) and implement tasks in order. 3\. Follow all interface contracts in Section 2.3 exactly. 4\. After completing each MVP task, write a brief comment in the relevant script noting its status. 5\. Do not invent new systems not covered here without flagging it as a deviation. 6\. If a specification is ambiguous, implement the simplest reasonable interpretation and add a TODO comment. |
| :---- |

| Out of Scope — Do not implement unless asked • Multiplayer / networking of any kind • Save/load system beyond PlayerPrefs for level unlocks • Mobile input / touch controls • Procedurally generated levels • Any AI enemy pathfinding beyond simple patrol patterns |
| :---- |

*— End of Specification —*