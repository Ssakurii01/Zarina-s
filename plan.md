# FluxFury — Hot Potato Bomb Game Setup Plan

> All code is written. Follow these steps in Unity Editor.
> The game is a **Hot Potato Bomb** party game: 1 player + 5 bots on a platformer map.
> A bomb auto-passes on contact. 15s timer per round. Holder at 0 explodes. Last one standing wins.

---

## Code Changes Summary

| What changed | Files |
|-------------|-------|
| Removed all shooting, HP, power-ups from Player | `PlayerController.cs` |
| Stripped fire/aim from input system | `IInputAdapter.cs`, `KeyboardInputAdapter.cs`, `ArcadeInputAdapter.cs` |
| Created bomb mechanic (follow holder, auto-transfer, 15s timer, explode) | `BombController.cs` (NEW) |
| Created bot AI (chase when holding bomb, flee when not) | `BotController.cs` (NEW) |
| Created round manager (spawns bots, assigns bomb, tracks eliminations) | `RoundManager.cs` (NEW) |
| Created jump pad launcher | `JumpPad.cs` (NEW) |
| Rewrote portal for teleportation + self-destruct | `Portal.cs` |
| GameManager uses RoundManager.OnGameOver instead of player death | `GameManager.cs` |
| ScoreManager tracks rounds survived only | `ScoreManager.cs` |
| GameUI shows bomb timer, alive count, round, bomb holder | `GameUI.cs` |
| Deleted: Bullet.cs, PowerUp.cs, EnemyDasher/Orbiter/Swarmer.cs, EnemyBase.cs, WaveManager.cs | — |

### Controls (keyboard — for testing):
| Key | Action |
|-----|--------|
| A / D / Arrows | Move left / right |
| Space / W / Up | Jump |

> No fire button — the bomb passes automatically on contact!

---

## STEP 1: Create the Bomb Prefab

1. In Hierarchy: **right-click → 3D Object → Sphere**
2. Name it `Bomb`
3. In Inspector:
   - **Transform → Scale** = `0.5, 0.5, 0.5`
   - **Sphere Collider → Is Trigger** = `✓`
4. Click **Add Component** → search `Rigidbody` → add it
   - **Use Gravity** = `☐` (uncheck)
   - **Is Kinematic** = `✓` (check)
5. Click **Add Component** → search `BombController` → add it
6. Change the material color to **yellow** (or create a yellow material and assign it)
7. **Create a Prefabs folder** (if not exists): right-click in Project → Create → Folder → name `Prefabs`
8. **Drag** `Bomb` from Hierarchy **into** `Assets/Prefabs/`
9. **Keep** it in the scene (don't delete it — it stays as the bomb singleton)

**Verify:** BombController shows in Inspector with Round Time = 15, Transfer Cooldown = 0.5, Detect Radius = 1.5

---

## STEP 2: Create the Bot Prefab

1. In Hierarchy: **right-click → 3D Object → Capsule**
2. Name it `Bot`
3. In Inspector:
   - **Transform → Scale** = `1, 1, 1`
   - **Tag** = add a new tag `Bot` (Edit → Project Settings → Tags and Layers → add "Bot" tag), then select it
4. **Capsule Collider** should already exist:
   - **Is Trigger** = `☐` (leave unchecked — bots stand on ground)
5. **Rigidbody** — click Add Component → Rigidbody:
   - **Use Gravity** = `✓`
6. Click **Add Component** → search `BotController` → add it
7. In BotController:
   - **Move Speed** = `5`
   - **Jump Force** = `8`
   - **Ground Layer** = click dropdown → check `Ground`
8. Give it a **different color material** (e.g., red) so you can tell bots apart from player
9. **Drag** `Bot` from Hierarchy **into** `Assets/Prefabs/`
10. **Delete** `Bot` from Hierarchy (it's now a prefab — RoundManager spawns them)

---

## STEP 3: Set Up the Player

If the Player is already in the scene, select it. Otherwise drag it in from Project window.

1. Select **Player** in Hierarchy
2. Make sure it has the **PlayerController** script (Add Component → PlayerController if missing)
3. In PlayerController:
   - **Move Speed** = `8`
   - **Jump Force** = `8`
   - **Ground Layer** = click dropdown → check `Ground`
4. Make sure **Tag** = `Player`
5. Make sure it has a **Collider** (CapsuleCollider) with **Is Trigger** = `☐`
6. Make sure it has a **Rigidbody** with **Use Gravity** = `✓`
7. **Delete** any old references (FirePoint child, Bullet Prefab field — these no longer exist)

**Position:** Place player on a platform or at `0, -14, 0` (bottom of arena)

---

## STEP 4: Add JumpPad Script to Jump Objects

Your scene already has jump pad objects (the disc-shaped things).

1. Select each **Jump** / **Jumper** tagged object in Hierarchy
2. Click **Add Component** → search `JumpPad` → add it
3. Set **Launch Force** = `18` (adjust to taste — higher = bigger jump)
4. Make sure the object has a **Collider** with **Is Trigger** = `✓`

---

## STEP 5: Set Up Portal Prefab

Your scene already has a Portal object. We need to make it a prefab for RoundManager to spawn.

1. Select the **Portal** in Hierarchy
2. Make sure it has the **Portal** script (should already be there)
3. Make sure it has a **Collider** with **Is Trigger** = `✓`
4. **Drag** the Portal from Hierarchy **into** `Assets/Prefabs/`
5. You can **delete** the original from the scene (portals now spawn automatically every 2 deaths)
   - OR keep it if you want one portal at start

---

## STEP 6: Create Spawn Points

Bots need places to spawn at the start of the game.

1. Create **6 empty GameObjects** in Hierarchy (right-click → Create Empty)
2. Name them `SpawnPoint1` through `SpawnPoint6`
3. Place them on different platforms around the map
   - Spread them out so bots don't all start in the same spot
   - Example positions: `(-6, -10, 0)`, `(6, -10, 0)`, `(-4, 0, 0)`, `(4, 0, 0)`, `(-2, 8, 0)`, `(2, 8, 0)`
4. Optional: group them under an empty parent called `SpawnPoints`

---

## STEP 7: Set Up RoundManager

1. Find the **GameManager** object in Hierarchy (or create a new empty object)
2. **Add Component** → search `RoundManager` → add it
3. Assign in Inspector:
   - **Bot Prefab** → drag `Assets/Prefabs/Bot` from Project window
   - **Portal Prefab** → drag `Assets/Prefabs/Portal` from Project window
   - **Spawn Points** → set size to 6, then drag each SpawnPoint1-6 into the slots
   - **Bot Count** = `5`
   - **Delay Between Rounds** = `3`
   - **Deaths Per Portal** = `2`

---

## STEP 8: Verify GameManager + ScoreManager

1. Make sure **GameManager** object has the `GameManager` script
2. Make sure it also has the `ScoreManager` script (or on a separate object)
3. Both should already exist from before — just verify they're present

---

## STEP 9: Set Up Camera for Portrait

1. Select **Main Camera** in Hierarchy
2. In Inspector:
   - **Projection** → `Orthographic`
   - **Size** → `18`
   - **Position** → `0, 0, -10`
   - **Background** → black
3. **Add Component** → `CameraShake` (if not already there)

---

## STEP 10: Add Portrait Game View

1. Click **Game** tab → resolution dropdown → **+**
2. Width = `1080`, Height = `1920`, name it `Luxodd Portrait`
3. Select it

---

## STEP 11: Canvas Scaler Settings

1. Select **Canvas** in Hierarchy
2. **Canvas Scaler** component:
   - **UI Scale Mode** = `Scale With Screen Size`
   - **Reference Resolution** = `1080 x 1920`
   - **Match** = `0.5`

---

## STEP 12: Set Up HUD Elements

The GameUI script needs these UI elements. Create them under Canvas.

### Top of screen (anchor to top):

| Element | Type | Assign to GameUI field | Notes |
|---------|------|----------------------|-------|
| BombTimerText | TextMeshPro - Text | `_bombTimerText` | Big number "15", flashes red <5s |
| AliveCountText | TextMeshPro - Text | `_aliveCountText` | Shows "6 ALIVE" |
| RoundText | TextMeshPro - Text | `_roundText` | Shows "ROUND 1" |

### Center of screen:

| Element | Type | Assign to GameUI field | Notes |
|---------|------|----------------------|-------|
| BombHolderText | TextMeshPro - Text | `_bombHolderText` | "YOU HAVE THE BOMB!" or "Alpha has the bomb" |

### Bottom of screen (anchor to bottom):

| Element | Type | Assign to GameUI field | Notes |
|---------|------|----------------------|-------|
| SessionTimerText | TextMeshPro - Text | `_sessionTimerText` | Shows "9:45" (10-min session) |
| ButtonHintsText | TextMeshPro - Text | `_buttonHintsText` | Shows "MOVE: A/D | JUMP: Space" |

### How to create a TextMeshPro text:
1. Right-click Canvas → **UI → Text - TextMeshPro**
2. If prompted to import TMP Essentials → click **Import**
3. Name it (e.g., `BombTimerText`)
4. Position it using **Rect Transform** anchors

### How to assign to GameUI:
1. Select the Canvas (which should have the **GameUI** script)
2. Drag each UI element into its matching field

### Target layout:
```
┌──────────────────────────┐
│  ROUND 1       6 ALIVE   │
│          15               │
│                           │
│  YOU HAVE THE BOMB!       │
│                           │
│       (game area)         │
│                           │
│                           │
│                  ⏱ 9:45  │
│  MOVE: A/D | JUMP: Space │
└──────────────────────────┘
```

---

## STEP 13: Set Up Game Over Panel

1. Under Canvas, create a **GameOverPanel** (UI → Panel)
2. Make it cover most of the screen, centered
3. Inside it create:

| Element | Type | Assign to GameUI field | On Click |
|---------|------|----------------------|----------|
| "GAME OVER" title | TextMeshPro - Text | (none) | — |
| WinnerText | TextMeshPro - Text | `_winnerText` | — |
| RoundsSurvivedText | TextMeshPro - Text | `_roundsSurvivedText` | — |
| RestartButton | UI → Button | `_restartButton` | `GameUI.OnRestartButton` |
| QuitButton | UI → Button | (none) | `GameUI.OnQuitButton` |

4. Set button On Click events (same as before)
5. Make buttons joystick-navigable (Explicit navigation)
6. Assign **GameOverPanel** to GameUI → `_gameOverPanel`

---

## STEP 14: Set Up InputManager + LuxoddSessionManager

These should already exist in the scene. Verify:
1. **InputManager** object exists with `InputManager` script
2. **LuxoddSessionManager** object exists with `LuxoddSessionManager` script

---

## STEP 15: Ensure Ground Layer

1. **Edit → Project Settings → Tags and Layers**
2. Layer 6 should be `Ground`
3. Make sure all platforms (Insky objects, bottom wall) are on the `Ground` layer
4. Select each platform → Inspector → **Layer** dropdown → `Ground`

---

## STEP 16: Test Everything

Press **Play** and verify:

| # | Test | Expected |
|---|------|----------|
| 1 | Player visible | Player capsule on a platform |
| 2 | Press A / D | Player moves left / right |
| 3 | Press Space | Player jumps |
| 4 | 5 bots spawn | Bots appear at spawn points |
| 5 | Bomb appears | Yellow sphere follows one character |
| 6 | Bomb timer | "15" counts down on screen |
| 7 | Bot/player touches bomb holder | Bomb transfers, text updates |
| 8 | Timer hits 0 | BOOM — holder disappears, alive count drops |
| 9 | Next round | 3s delay, then bomb assigned to random survivor |
| 10 | After 2 deaths | Portal spawns somewhere |
| 11 | Touch portal | Character teleports to random position |
| 12 | Step on jump pad | Character launches high |
| 13 | Last one standing | Game Over screen shows winner |
| 14 | Player eliminated | Game Over shows who won |
| 15 | Click Restart | Game restarts |
| 16 | Session timer | Counts down from 10:00 |

### Common issues:
- **Bots don't spawn** → Bot Prefab not assigned on RoundManager
- **Bomb doesn't appear** → BombController object not in scene (Step 1)
- **Bomb doesn't transfer** → Characters need non-trigger colliders. Bomb detects via OverlapSphere
- **Player falls forever** → Ground layer not set on platforms. Check Layer 6 = "Ground"
- **Can't jump** → Ground Layer not assigned on PlayerController/BotController
- **Jump pads don't work** → JumpPad script not added, or collider not set to trigger
- **Portals don't teleport** → Portal collider not trigger, or Portal script missing

---

## STEP 17: Install Luxodd Plugin (when ready for arcade)

1. Download from https://github.com/luxodd/unity-plugin/releases
2. **Assets → Import Package → Custom Package** → select the file → **Import**
3. If prompted for Newtonsoft.Json → **Install**
4. Drag `Assets/Luxodd.Game/Prefabs/UnityPluginPrefab` into scene
5. **Tools → Luxodd Plugin → Set Developer Token**
6. Select **LuxoddSessionManager**, assign:
   - `WebSocketService` → `_webSocketService`
   - `WebSocketCommandHandler` → `_commandHandler`
   - `HealthStatusCheckService` → `_healthCheckService`
7. **Edit → Project Settings → Player → Scripting Define Symbols** → Add `LUXODD_PLUGIN`

---

## STEP 18: WebGL Build

1. **File → Build Profiles** → **Web/WebGL** → **Switch Platform**
2. **Player Settings → Web:**
   - Canvas Width = `1080`, Height = `1920`
   - Run in Background = `✓`
   - WebGL Template = `LuxoddTemplate`
3. **Publishing Settings:** Gzip, Name Files as Hashes, Data Caching, Debug Symbols Off
4. **Build** → zip output → upload at https://app.luxodd.com

---

## Game Flow Summary

```
Scene Load → RoundManager spawns 5 bots
  → Round 1: random character gets bomb
  → 15s countdown, bomb follows holder
  → Characters collide → bomb auto-transfers
  → Timer hits 0 → BOOM → holder eliminated
  → If >1 alive → next round (3s delay)
  → Every 2 deaths → portal spawns
  → Last one standing → Game Over screen
  → Restart or quit
```
