# FluxFury — Luxodd Arcade Adaptation: Complete Step-by-Step Guide

> This guide takes you through every change needed to make FluxFury work on the Luxodd arcade cabinet. Each step tells you exactly WHAT to do, WHERE to do it, and shows you the exact code. Follow the steps in order.

---

## What is the Luxodd Arcade Cabinet?

Before we start, you need to understand the target platform:

- **Screen**: Portrait monitor (tall, not wide) at **1080 x 1920 pixels** (9:16 ratio)
- **Controls**: A physical **joystick** (8-way) + **6 colored buttons** (no mouse, no touchscreen)
- **Runs on**: A Windows 11 PC with Chrome browser
- **Your game runs as**: A **Unity WebGL build** loaded inside Chrome
- **No mouse, no keyboard** on the real cabinet — but we want keyboard to work for testing on your PC

**What this means for FluxFury:**
- The game currently uses mouse to aim and shoot → we must replace this with auto-aim + button shooting
- The game is currently landscape/square → we must make it portrait (tall and narrow)
- The game has a pause feature → Luxodd forbids pausing, we must remove it
- The game must connect to Luxodd's server for leaderboards, credits, and session management

---

## PHASE 1: INPUT SYSTEM — Remove Mouse, Add Joystick + Keyboard Support [CODE DONE]

> **All code for Phase 1 has been written.** The files below are already created/modified. You only need to do the Unity Editor setup steps listed at the bottom.

### What was done (code — already complete):

**New files created:**
- `Assets/Scripts/Input/IInputAdapter.cs` — Interface defining game input actions (MoveHorizontal, IsFireHeld, AimDirection, OnJumpPressed)
- `Assets/Scripts/Input/KeyboardInputAdapter.cs` — Keyboard adapter for dev/testing (A/D move, Space/W jump, J/Z fire, auto-aim at nearest enemy)
- `Assets/Scripts/Input/ArcadeInputAdapter.cs` — Arcade adapter for Luxodd cabinet (joystick move, Black=jump, Red=fire, auto-aim)
- `Assets/Scripts/Input/InputManager.cs` — Singleton that auto-selects the right adapter (keyboard for dev, arcade when LUXODD_PLUGIN is defined)

**Modified file:**
- `Assets/Scripts/Player/PlayerController.cs` — Complete rewrite:
  - Removed `using UnityEngine.InputSystem`
  - Removed all mouse/PlayerInput fields (`_mainCamera`, `_groundPlane`, `_playerInput`, `_moveAction`, `_attackAction`)
  - `HandleMovement()` now reads from `InputManager.Instance.Input.MoveHorizontal`
  - `HandleRotation()` now uses `InputManager.Instance.Input.AimDirection` (auto-aim at nearest enemy, no mouse)
  - `HandleShooting()` now uses `InputManager.Instance.Input.IsFireHeld` (J/Z key or Red button, no mouse click)
  - `HandleJump()` is now event-driven via `InputManager.Instance.Input.OnJumpPressed` (not polled in Update)
  - Added `OnDestroy()` to unsubscribe from jump event

### How auto-aim works:
The input adapters scan all GameObjects tagged "Enemy" and calculate the direction from the player to the closest one. The player automatically rotates to face that enemy and bullets fire in that direction. If no enemies exist, the player aims upward. The enemy list is cached and refreshed every 0.1 seconds for performance.

### Keyboard controls (for testing):
| Key | Action |
|-----|--------|
| A / D / Left / Right Arrow | Move left / right |
| Space / W / Up Arrow | Jump |
| J / Z (hold) | Fire |
| Aiming | Automatic (nearest enemy) |

### Arcade controls (on Luxodd cabinet):
| Control | Action |
|---------|--------|
| Joystick left/right | Move |
| Black button | Jump |
| Red button (hold) | Fire |
| Green / Yellow / Blue / Purple | Available for future use |
| Orange / White | DO NOT USE (system reserved) |

---

### YOUR SETUP STEPS (Unity Editor — do these manually):

#### Step A: Create the InputManager GameObject
1. In the **Hierarchy** window, right-click → **Create Empty**
2. Name it `InputManager`
3. In the **Inspector**, click **Add Component** → search for `InputManager` → add it
4. That's it — it automatically creates the correct input adapter at runtime

#### Step B: Remove PlayerInput component from Player
1. Select the **Player** GameObject (or Player prefab) in the Hierarchy
2. In the Inspector, find the **Player Input** component
3. Right-click it → **Remove Component**
4. This component used the old mouse/keyboard input system — we replaced it with our adapter

#### Step C: Verify Enemy tags
The auto-aim system needs enemies tagged correctly:
1. Open each enemy prefab (Swarmer, Dasher, Orbiter)
2. In the Inspector at the top, check the **Tag** dropdown
3. Make sure each one is set to `Enemy`
4. If the `Enemy` tag doesn't exist, go to **Edit → Project Settings → Tags and Layers** → add it

#### Step D: Test it
1. Press **Play** in the Unity Editor
2. Use **A/D** to move left/right
3. Hold **J** or **Z** to fire — the player should auto-aim at the nearest enemy
4. Press **Space** to jump
5. If no enemies are on screen, the player aims upward by default

---

## PHASE 2: PORTRAIT SCREEN LAYOUT (1080 x 1920)

### Why
The arcade cabinet screen is TALL (portrait), not wide (landscape). Our arena, camera, and UI all need to fit a 9:16 aspect ratio.

---

### Step 2.1: Change Arena Dimensions for Portrait

**What**: The arena is currently 40x40 (square). For portrait, we need it taller than it is wide. New size: **18 wide x 32 tall**.

**Where**: Modify `Assets/Scripts/Arena/ArenaSetup.cs`

**What to change:**

```csharp
// OLD:
[SerializeField] private float _arenaSize = 40f;

// NEW — replace the single size with width + height:
[SerializeField] private float _arenaWidth = 18f;
[SerializeField] private float _arenaHeight = 32f;
```

**Then update CreateFloor():**
```csharp
private void CreateFloor()
{
    GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
    floor.name = "Arena Floor";
    floor.transform.parent = transform;
    floor.transform.localPosition = Vector3.zero;
    floor.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
    // Plane primitive is 10x10 units by default, so we divide by 10
    floor.transform.localScale = new Vector3(_arenaWidth / 10f, 1f, _arenaHeight / 10f);

    if (_floorMaterial != null)
        floor.GetComponent<Renderer>().material = _floorMaterial;
}
```

**Then update CreateWalls():**
```csharp
private void CreateWalls()
{
    float halfW = _arenaWidth / 2f;
    float halfH = _arenaHeight / 2f;

    // Top wall (positive Y — top of the screen)
    CreateWall("Wall_Top", new Vector3(0, halfH, 0),
        new Vector3(_arenaWidth + _wallThickness, _wallThickness, _wallHeight));
    // Bottom wall (negative Y — bottom of the screen)
    CreateWall("Wall_Bottom", new Vector3(0, -halfH, 0),
        new Vector3(_arenaWidth + _wallThickness, _wallThickness, _wallHeight));
    // Right wall (positive X)
    CreateWall("Wall_Right", new Vector3(halfW, 0, 0),
        new Vector3(_wallThickness, _arenaHeight + _wallThickness, _wallHeight));
    // Left wall (negative X)
    CreateWall("Wall_Left", new Vector3(-halfW, 0, 0),
        new Vector3(_wallThickness, _arenaHeight + _wallThickness, _wallHeight));
}
```

**Also add public getters so other scripts can know the arena size:**
```csharp
public float ArenaWidth => _arenaWidth;
public float ArenaHeight => _arenaHeight;
```

---

### Step 2.2: Set Up the Camera for Portrait View

**In the Unity Editor:**

1. Select the **Main Camera** in the Hierarchy
2. In the Inspector, change **Projection** to **Orthographic**
   - Orthographic means no perspective distortion — objects don't get smaller when far away
   - This is standard for 2D games
3. Set **Orthographic Size** to `18`
   - This value is HALF the vertical view height in world units
   - So the camera sees 36 units tall, which fits our 32-unit arena with some margin
4. Set camera **Position** to `(0, 0, -10)`
   - X=0, Y=0 centers it on the arena
   - Z=-10 puts it behind the XY plane so it can see everything
5. Set camera **Rotation** to `(0, 0, 0)`
6. Set the **Background** color to something dark (e.g., black)

**To test the portrait aspect ratio in the Editor:**
1. In the **Game** window, click the aspect ratio dropdown (it might say "Free Aspect")
2. Click the **+** button to add a custom resolution
3. Set it to **1080 x 1920** and name it "Luxodd Portrait"
4. Select this ratio — the Game view now shows what the arcade screen looks like

---

### Step 2.3: Update WaveManager Spawn Radius

**What**: Enemies currently spawn in a circle with radius 20 — but our arena is now only 18 wide. We need to adjust so enemies spawn just outside the visible area but inside the walls.

**Where**: Modify `Assets/Scripts/Managers/WaveManager.cs`

**What to change:**

```csharp
// OLD:
[SerializeField] private float _arenaRadius = 20f;

// NEW — spawn from edges of the portrait arena:
[SerializeField] private float _spawnMarginX = 8f;  // Half arena width (18/2 = 9, minus 1 for margin)
[SerializeField] private float _spawnMarginY = 15f; // Half arena height (32/2 = 16, minus 1)
```

**Then update SpawnEnemy():**
```csharp
private void SpawnEnemy(GameObject prefab)
{
    if (prefab == null) return;

    // Spawn at a random edge of the arena (top, bottom, left, or right)
    Vector3 spawnPos;
    int edge = Random.Range(0, 4); // 0=top, 1=bottom, 2=left, 3=right

    switch (edge)
    {
        case 0: // Top edge
            spawnPos = new Vector3(Random.Range(-_spawnMarginX, _spawnMarginX), _spawnMarginY, 0f);
            break;
        case 1: // Bottom edge
            spawnPos = new Vector3(Random.Range(-_spawnMarginX, _spawnMarginX), -_spawnMarginY, 0f);
            break;
        case 2: // Left edge
            spawnPos = new Vector3(-_spawnMarginX, Random.Range(-_spawnMarginY, _spawnMarginY), 0f);
            break;
        default: // Right edge
            spawnPos = new Vector3(_spawnMarginX, Random.Range(-_spawnMarginY, _spawnMarginY), 0f);
            break;
    }

    Instantiate(prefab, spawnPos, Quaternion.identity);
}
```

---

## PHASE 3: REMOVE PAUSE SYSTEM (Luxodd Requirement)

### Why
Luxodd rules say: **"No pause mechanic allowed."** The game must always be running. This makes sense for an arcade — you're paying per session, no pausing.

---

### Step 3.1: Remove Pause from GameManager

**Where**: Modify `Assets/Scripts/Managers/GameManager.cs`

**Changes:**

1. **Remove the `Paused` state** from the enum:
```csharp
// OLD:
public enum GameState { Playing, Paused, GameOver }

// NEW:
public enum GameState { Playing, GameOver }
```

2. **Remove the entire Escape key handling in Update():**
```csharp
// OLD:
void Update()
{
    if (_currentState == GameState.GameOver) return;
    if (Input.GetKeyDown(KeyCode.Escape))
    {
        if (_currentState == GameState.Playing) PauseGame();
        else if (_currentState == GameState.Paused) ResumeGame();
    }
}

// NEW:
void Update()
{
    // No pause handling — Luxodd does not allow pausing
}
```

3. **Delete PauseGame() and ResumeGame() methods entirely:**
```csharp
// DELETE THESE METHODS:
public void PauseGame() { ... }
public void ResumeGame() { ... }
```

---

### Step 3.2: Remove Pause UI

**Where**: Modify `Assets/Scripts/UI/GameUI.cs`

1. **Remove the pause panel field:**
```csharp
// DELETE this line:
[SerializeField] private GameObject _pausePanel;
```

2. **Remove pause handling from HandleGameState():**
```csharp
// OLD:
private void HandleGameState(GameManager.GameState state)
{
    switch (state)
    {
        case GameManager.GameState.GameOver:
            ShowGameOver();
            break;
        case GameManager.GameState.Paused:
            if (_pausePanel != null) _pausePanel.SetActive(true);
            break;
        case GameManager.GameState.Playing:
            if (_pausePanel != null) _pausePanel.SetActive(false);
            break;
    }
}

// NEW:
private void HandleGameState(GameManager.GameState state)
{
    if (state == GameManager.GameState.GameOver)
    {
        ShowGameOver();
    }
}
```

3. **Remove OnResumeButton() method:**
```csharp
// DELETE this method:
public void OnResumeButton() { ... }
```

4. **Remove the pause panel hide in Start():**
```csharp
// DELETE this line from Start():
if (_pausePanel != null) _pausePanel.SetActive(false);
```

**In the Unity Editor:**
- Select your Canvas → find the Pause Panel GameObject → **Delete it** from the Hierarchy
- In the GameUI component Inspector, the `_pausePanel` field will disappear since we removed it from code

---

## PHASE 4: SESSION TIMER (Luxodd: Max 10 Minutes)

### Why
Luxodd rule: **"Session must end in ≤ 10 minutes."** Our game currently has no time limit — you just play until you die. We need to add a countdown timer. When it hits zero → game over.

---

### Step 4.1: Add Session Timer to GameManager

**Where**: Modify `Assets/Scripts/Managers/GameManager.cs`

**Add these fields:**
```csharp
[Header("Session Timer")]
[SerializeField] private float _maxSessionTime = 600f; // 600 seconds = 10 minutes
private float _sessionTimeRemaining;

public float SessionTimeRemaining => _sessionTimeRemaining;

public delegate void TimerEvent(float timeRemaining);
public static event TimerEvent OnTimerChanged;
```

**Initialize in Start():**
```csharp
void Start()
{
    _sessionTimeRemaining = _maxSessionTime;
}
```

**Countdown in Update():**
```csharp
void Update()
{
    if (_currentState != GameState.Playing) return;

    // Count down the session timer
    _sessionTimeRemaining -= Time.deltaTime;
    OnTimerChanged?.Invoke(_sessionTimeRemaining);

    if (_sessionTimeRemaining <= 0f)
    {
        _sessionTimeRemaining = 0f;
        // Time's up — trigger game over
        HandlePlayerDeath();
    }
}
```

---

### Step 4.2: Display the Timer in the HUD

**Where**: Modify `Assets/Scripts/UI/GameUI.cs`

**Add a timer text field:**
```csharp
[Header("Timer")]
[SerializeField] private TextMeshProUGUI _timerText;
```

**Subscribe/unsubscribe:**
```csharp
void OnEnable()
{
    // ... (keep existing subscriptions) ...
    GameManager.OnTimerChanged += UpdateTimer;
}

void OnDisable()
{
    // ... (keep existing unsubscriptions) ...
    GameManager.OnTimerChanged -= UpdateTimer;
}
```

**Add the timer update method:**
```csharp
private void UpdateTimer(float timeRemaining)
{
    if (_timerText == null) return;

    // Convert seconds to M:SS format
    int minutes = Mathf.FloorToInt(timeRemaining / 60f);
    int seconds = Mathf.FloorToInt(timeRemaining % 60f);
    _timerText.text = $"{minutes}:{seconds:00}";

    // Change color to red when under 30 seconds
    _timerText.color = timeRemaining < 30f ? Color.red : Color.white;
}
```

**In the Unity Editor:**
1. In your Canvas, create a new **TextMeshPro - Text** UI element
2. Name it `TimerText`
3. Position it in the bottom-right area of the HUD
4. Set font size to ~36, color white, alignment right
5. Drag it into the `_timerText` field on the GameUI component

---

## PHASE 5: MENU TIMEOUTS (Luxodd Requirement)

### Why
Luxodd rules:
- **"All menus auto-return to arcade menu after max 30 seconds of inactivity"**
- **"Max 1 minute wait for input before force-starting"**

If a player walks away from the cabinet, the game must not sit idle forever.

---

### Step 5.1: Add Inactivity Timer to GameManager

**Where**: Modify `Assets/Scripts/Managers/GameManager.cs`

**Add these fields:**
```csharp
[Header("Inactivity Timeout")]
[SerializeField] private float _menuTimeoutSeconds = 30f;
[SerializeField] private float _startTimeoutSeconds = 60f;
private float _inactivityTimer;
private bool _hasReceivedFirstInput = false;
```

**In Update(), add inactivity detection:**
```csharp
void Update()
{
    if (_currentState != GameState.Playing) return;

    // Session timer countdown (from Step 4.1)
    _sessionTimeRemaining -= Time.deltaTime;
    OnTimerChanged?.Invoke(_sessionTimeRemaining);
    if (_sessionTimeRemaining <= 0f)
    {
        _sessionTimeRemaining = 0f;
        HandlePlayerDeath();
        return;
    }

    // Check for any input to reset inactivity timer
    bool anyInput = InputManager.Instance?.Input != null
        && (Mathf.Abs(InputManager.Instance.Input.MoveHorizontal) > 0.1f
            || InputManager.Instance.Input.IsFireHeld);

    if (anyInput)
    {
        _inactivityTimer = 0f;
        _hasReceivedFirstInput = true;
    }
    else
    {
        _inactivityTimer += Time.deltaTime;
    }

    // Start timeout: if no input for 60 seconds after game loads
    if (!_hasReceivedFirstInput && _inactivityTimer >= _startTimeoutSeconds)
    {
        EndSession();
    }
}
```

**Add EndSession method (will be expanded in Luxodd integration phase):**
```csharp
public void EndSession()
{
    // For now, just quit. Later this will call Luxodd BackToSystem()
    QuitGame();
}
```

**Add timeout check for Game Over screen in a new method:**
```csharp
// Call this from GameUI when the game over panel is shown
private float _gameOverTimer;

public void UpdateGameOverTimeout()
{
    _gameOverTimer += Time.unscaledDeltaTime; // unscaled because timeScale might be 0

    if (_gameOverTimer >= _menuTimeoutSeconds)
    {
        EndSession();
    }
}

// Reset when game over starts
private void HandlePlayerDeath()
{
    _currentState = GameState.GameOver;
    _gameOverTimer = 0f;
    Time.timeScale = 1f;
    OnGameStateChanged?.Invoke(_currentState);
}
```

---

## PHASE 6: UI OVERHAUL FOR PORTRAIT ARCADE

### Why
All UI must be repositioned for portrait 1080x1920, and all buttons must work with joystick (no mouse clicking).

---

### Step 6.1: Redesign the HUD Layout in Unity Editor

The HUD should look like this on the portrait screen:

```
┌─────────────────────────┐
│ SCORE: 12,500   WAVE 3  │  ← Top bar (anchored to top)
│      x3 COMBO!          │
│                         │
│                         │
│                         │
│      (game area)        │
│                         │
│                         │
│                         │
│ ♥♥♥♥♥         ⏱ 8:32   │  ← Bottom bar (anchored to bottom)
│                         │
│ [BLACK]=Jump [RED]=Fire │  ← Button hints (small text)
└─────────────────────────┘
```

**Step-by-step in Unity Editor:**

1. **Select your Canvas** in the Hierarchy
   - Set Canvas Scaler → **UI Scale Mode** = "Scale With Screen Size"
   - **Reference Resolution** = 1080 x 1920
   - **Match** slider = 0.5 (balanced width/height scaling)

2. **Create the Top Bar:**
   - Create an empty UI object → name it `TopBar`
   - Set anchors to **top-stretch** (holds to top, stretches horizontally)
   - Height = 120 pixels
   - Add a dark semi-transparent **Image** component (background)
   - Put `ScoreText` (TextMeshPro) on the left side, anchored top-left
   - Put `WaveText` (TextMeshPro) on the right side, anchored top-right
   - Put `ComboText` (TextMeshPro) centered below, anchored top-center

3. **Create the Bottom Bar:**
   - Create an empty UI object → name it `BottomBar`
   - Set anchors to **bottom-stretch**
   - Height = 150 pixels
   - Put the `HealthBar` (Slider) on the left side
   - Put `TimerText` on the right side
   - Below that, add a `ButtonHints` TextMeshPro with text:
     `"[BLACK] Jump   [RED] Fire"`

4. **Reposition the Game Over Panel:**
   - Center it on screen (anchor = center-center)
   - Width = 900, Height = 1400
   - Stack the elements vertically:
     - "GAME OVER" title at top
     - Final Score
     - Final Wave
     - High Score / Rank
     - (Space for leaderboard — added in Phase 7)
     - Restart Button
     - Quit Button

---

### Step 6.2: Make Menu Buttons Work with Joystick

**What**: On the arcade cabinet there is no mouse cursor. Buttons must be navigated with the joystick (up/down to select, Black button to confirm). Unity's built-in **EventSystem** can handle this if set up correctly.

**Step-by-step in Unity Editor:**

1. **Select the EventSystem** in the Hierarchy (it should already exist)
   - In the Inspector, find `Standalone Input Module`
   - Set **Horizontal Axis** = `Horizontal`
   - Set **Vertical Axis** = `Vertical`
   - Set **Submit Button** = `Submit` (maps to JoystickButton0 = Black button)
   - Set **Cancel Button** = `Cancel`
   - This tells Unity's UI system to use joystick axes and buttons for navigation

2. **For each Button in your Game Over panel:**
   - Select the button in the Hierarchy
   - In the Inspector, find the **Navigation** section
   - Set **Navigation** = `Explicit`
   - Set **Select On Up** = the button above it
   - Set **Select On Down** = the button below it
   - This creates a clear up/down navigation chain

3. **Set the first selected button:**
   - On the EventSystem, there's a **First Selected** field
   - When the Game Over panel opens, set this via code:

```csharp
// Add to GameUI.cs in ShowGameOver():
using UnityEngine.EventSystems;

private void ShowGameOver()
{
    if (_gameOverPanel != null)
    {
        _gameOverPanel.SetActive(true);
        // ... existing score display code ...

        // Set the first button as selected so joystick navigation works immediately
        if (_restartButton != null)
        {
            EventSystem.current.SetSelectedGameObject(_restartButton.gameObject);
        }
    }
}
```

4. **Add a [SerializeField] for the restart button:**
```csharp
[Header("Buttons")]
[SerializeField] private Button _restartButton;
```

---

### Step 6.3: Add Button Hint Display

**What**: Show the player which physical buttons do what. On the arcade, the buttons are colored — the player needs to know "press the RED button to fire."

**Where**: Add to `Assets/Scripts/UI/GameUI.cs`

```csharp
[Header("Button Hints")]
[SerializeField] private TextMeshProUGUI _buttonHintsText;

void Start()
{
    // ... existing code ...

    // Show button mapping hints
    if (_buttonHintsText != null)
    {
        _buttonHintsText.text = "MOVE: Joystick  |  JUMP: Black  |  FIRE: Red";
    }
}
```

---

## PHASE 7: LUXODD PLUGIN INTEGRATION

### Why
The Luxodd platform requires your game to:
- Connect to their server via WebSocket
- Send health checks every 5 seconds (or the session auto-terminates)
- Track level start/end for the leaderboard
- Handle Continue/Restart flows when the player dies
- Call `BackToSystem()` when the game session ends

---

### Step 7.1: Install the Luxodd Unity Plugin

1. **Download** the plugin from: https://github.com/luxodd/unity-plugin/releases
   - Download the latest `.unitypackage` file (e.g., `LuxoddPlugin_v1.0.8.unitypackage`)

2. **Import into Unity:**
   - In Unity, go to **Assets → Import Package → Custom Package...**
   - Navigate to the downloaded `.unitypackage` file → click **Open**
   - In the Import window, make sure everything is checked → click **Import**
   - If prompted to install **Newtonsoft.Json**, click **Install**

3. **Verify installation:**
   - You should see a `Luxodd.Game/` folder appear in your Assets
   - In the menu bar, you should see **Tools → Luxodd Plugin**

4. **Add the prefab to your scene:**
   - In the Project window, navigate to `Assets/Luxodd.Game/Prefabs/`
   - Drag `UnityPluginPrefab` into your scene Hierarchy
   - This prefab contains: WebSocketService, WebSocketCommandHandler, HealthStatusCheckService, ReconnectService

5. **Set your Developer Token:**
   - Go to **Tools → Luxodd Plugin → Set Developer Token**
   - Paste your token from the Luxodd Admin Portal (https://app.luxodd.com)

---

### Step 7.2: Create the Luxodd Session Manager

**What**: A new script that handles ALL communication with the Luxodd server. This is the bridge between your game and the arcade platform.

**Where**: Create new file `Assets/Scripts/Managers/LuxoddSessionManager.cs`

**Code**:
```csharp
using UnityEngine;
// These namespaces come from the Luxodd plugin
using Luxodd.Game.Scripts.Network;
using Luxodd.Game.Scripts.Network.CommandHandler;
using Luxodd.Game.Scripts.Game.Leaderboard;

/// <summary>
/// Manages all communication with the Luxodd arcade platform.
/// Handles: connection, health checks, leaderboard, session lifecycle.
///
/// IMPORTANT: This script requires the Luxodd Unity Plugin to be installed.
/// Drag the UnityPluginPrefab into your scene and assign its components here.
/// </summary>
public class LuxoddSessionManager : MonoBehaviour
{
    public static LuxoddSessionManager Instance { get; private set; }

    [Header("Luxodd Plugin References (drag from UnityPluginPrefab)")]
    [SerializeField] private WebSocketService _webSocketService;
    [SerializeField] private WebSocketCommandHandler _commandHandler;
    [SerializeField] private HealthStatusCheckService _healthCheckService;

    [Header("State")]
    private string _playerName = "";
    private int _playerBalance = 0;
    private bool _isConnected = false;

    // Events for UI to listen to
    public delegate void PlayerInfoEvent(string name, int balance);
    public static event PlayerInfoEvent OnPlayerInfoReceived;

    public delegate void LeaderboardEvent(LeaderboardDataResponse data);
    public static event LeaderboardEvent OnLeaderboardReceived;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        ConnectToServer();
    }

    /// <summary>
    /// Step 1: Connect to the Luxodd WebSocket server.
    /// The session token is automatically read from the URL by the plugin.
    /// </summary>
    private void ConnectToServer()
    {
        if (_webSocketService == null)
        {
            Debug.LogError("LuxoddSessionManager: WebSocketService not assigned!");
            return;
        }

        _webSocketService.ConnectToServer(
            onSuccessCallback: () =>
            {
                Debug.Log("Luxodd: Connected to server!");
                _isConnected = true;

                // Step 2: Start health checks (REQUIRED — every 5 seconds)
                // If 3 checks are missed, Luxodd auto-terminates your session
                if (_healthCheckService != null)
                    _healthCheckService.Activate();

                // Step 3: Get player info
                GetPlayerProfile();
                GetPlayerBalance();

                // Step 4: Signal level begin
                SendLevelBegin(1);
            },
            onErrorCallback: () =>
            {
                Debug.LogError("Luxodd: Connection failed!");
            }
        );
    }

    private void GetPlayerProfile()
    {
        _commandHandler.SendProfileRequestCommand(
            onSuccessCallback: (name) =>
            {
                _playerName = name;
                Debug.Log($"Luxodd: Player = {name}");
                OnPlayerInfoReceived?.Invoke(_playerName, _playerBalance);
            },
            onFailureCallback: (code, msg) =>
                Debug.LogError($"Luxodd: Profile failed [{code}]: {msg}")
        );
    }

    private void GetPlayerBalance()
    {
        _commandHandler.SendUserBalanceRequestCommand(
            onSuccessCallback: (credits) =>
            {
                _playerBalance = (int)credits;
                Debug.Log($"Luxodd: Balance = {credits}");
                OnPlayerInfoReceived?.Invoke(_playerName, _playerBalance);
            },
            onFailureCallback: (code, msg) =>
                Debug.LogError($"Luxodd: Balance failed [{code}]: {msg}")
        );
    }

    /// <summary>
    /// Call this when a new wave/level starts.
    /// Required for leaderboard tracking.
    /// </summary>
    public void SendLevelBegin(int level)
    {
        if (!_isConnected) return;

        _commandHandler.SendLevelBeginRequestCommand(
            level: level,
            onSuccessCallback: () => Debug.Log($"Luxodd: Level {level} begin tracked"),
            onFailureCallback: (code, msg) =>
                Debug.LogError($"Luxodd: Level begin failed [{code}]: {msg}")
        );
    }

    /// <summary>
    /// Call this when the game ends (player dies or time runs out).
    /// This sends the final score to Luxodd for the leaderboard.
    /// YOU MUST CALL THIS before showing Continue/Restart popups.
    /// </summary>
    public void SendLevelEnd(int level, int score)
    {
        if (!_isConnected) return;

        _commandHandler.SendLevelEndRequestCommand(
            level: level,
            score: score,
            onSuccessCallback: () =>
            {
                Debug.Log($"Luxodd: Level {level} end tracked (score: {score})");
            },
            onFailureCallback: (code, msg) =>
                Debug.LogError($"Luxodd: Level end failed [{code}]: {msg}")
        );
    }

    /// <summary>
    /// Show the "Continue?" popup. Luxodd handles the payment UI.
    /// If the player pays, we restore their HP and resume.
    /// If they decline, we end the session.
    /// </summary>
    public void ShowContinuePopup()
    {
        if (!_isConnected || _webSocketService == null) return;

        _webSocketService.SendSessionOptionContinue((action) =>
        {
            switch (action)
            {
                case SessionOptionAction.Continue:
                    // Player paid! Resume gameplay
                    Debug.Log("Luxodd: Player chose CONTINUE");
                    GameManager.Instance?.RestartGame(); // or restore HP
                    break;
                case SessionOptionAction.End:
                    // Player declined — exit to arcade menu
                    Debug.Log("Luxodd: Player chose END");
                    BackToSystem();
                    break;
            }
        });
    }

    /// <summary>
    /// Fetch the leaderboard from Luxodd's server.
    /// Requires SendLevelEnd to have been called first.
    /// </summary>
    public void FetchLeaderboard()
    {
        if (!_isConnected) return;

        _commandHandler.SendLeaderboardRequestCommand(
            onSuccessCallback: (response) =>
            {
                Debug.Log($"Luxodd: Your rank = #{response.CurrentUserData.Rank}");
                OnLeaderboardReceived?.Invoke(response);
            },
            onFailureCallback: (code, msg) =>
                Debug.LogError($"Luxodd: Leaderboard failed [{code}]: {msg}")
        );
    }

    /// <summary>
    /// End the game session and return to the Luxodd arcade menu.
    /// ALWAYS call this when the game is completely done.
    /// Never let the game just sit idle after game over.
    /// </summary>
    public void BackToSystem()
    {
        if (_webSocketService != null)
        {
            _webSocketService.BackToSystem();
        }
    }
}
```

**In the Unity Editor:**
1. Create an empty GameObject → name it `LuxoddSessionManager`
2. Add the `LuxoddSessionManager` script to it
3. From the `UnityPluginPrefab` in your scene, drag:
   - `WebSocketService` component → into the `_webSocketService` field
   - `WebSocketCommandHandler` component → into the `_commandHandler` field
   - `HealthStatusCheckService` component → into the `_healthCheckService` field

---

### Step 7.3: Connect GameManager to Luxodd

**Where**: Modify `Assets/Scripts/Managers/GameManager.cs`

**Update HandlePlayerDeath():**
```csharp
private void HandlePlayerDeath()
{
    _currentState = GameState.GameOver;
    _gameOverTimer = 0f;
    Time.timeScale = 1f;
    OnGameStateChanged?.Invoke(_currentState);

    // Send final score to Luxodd
    int finalScore = ScoreManager.Instance != null ? ScoreManager.Instance.Score : 0;
    int finalWave = WaveManager.Instance != null ? WaveManager.Instance.CurrentWave : 1;

    if (LuxoddSessionManager.Instance != null)
    {
        LuxoddSessionManager.Instance.SendLevelEnd(finalWave, finalScore);
    }
}
```

**Update EndSession():**
```csharp
public void EndSession()
{
    // Tell Luxodd we're done — returns player to arcade menu
    if (LuxoddSessionManager.Instance != null)
    {
        LuxoddSessionManager.Instance.BackToSystem();
    }
    else
    {
        // Fallback for testing without Luxodd plugin
        QuitGame();
    }
}
```

**Update RestartGame():**
```csharp
public void RestartGame()
{
    Time.timeScale = 1f;

    // If Luxodd is connected, use the Continue/Restart flow
    if (LuxoddSessionManager.Instance != null)
    {
        LuxoddSessionManager.Instance.ShowContinuePopup();
    }
    else
    {
        // Fallback for testing: just reload the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
```

---

### Step 7.4: Connect WaveManager to Luxodd Level Tracking

**Where**: Modify `Assets/Scripts/Managers/WaveManager.cs`

**In StartNextWave(), after incrementing the wave:**
```csharp
private IEnumerator StartNextWave()
{
    yield return new WaitForSeconds(_timeBetweenWaves);

    _currentWave++;
    _spawning = true;

    // Tell Luxodd a new wave/level started
    if (LuxoddSessionManager.Instance != null)
    {
        LuxoddSessionManager.Instance.SendLevelBegin(_currentWave);
    }

    OnWaveStart?.Invoke(_currentWave);
    // ... rest of spawn code ...
}
```

---

### Step 7.5: Remove PlayerPrefs High Score (Use Luxodd Leaderboard Instead)

**Where**: Modify `Assets/Scripts/Managers/ScoreManager.cs`

**Remove PlayerPrefs usage:**
```csharp
// OLD (in Awake):
_highScore = PlayerPrefs.GetInt("HighScore", 0);

// NEW:
// Don't load from PlayerPrefs — Luxodd leaderboard handles this
_highScore = 0;

// OLD (in AddScore):
if (_score > _highScore)
{
    _highScore = _score;
    PlayerPrefs.SetInt("HighScore", _highScore);
}

// NEW:
if (_score > _highScore)
{
    _highScore = _score;
    // No PlayerPrefs — Luxodd tracks high scores via leaderboard API
}
```

**Why**: PlayerPrefs uses file I/O which doesn't work reliably in WebGL. Luxodd has its own leaderboard server that stores scores properly.

---

## PHASE 8: WEBGL BUILD CONFIGURATION

### Why
The arcade cabinet runs your game as a WebGL build in Chrome. This section covers the exact Unity settings you need.

---

### Step 8.1: Switch to WebGL Platform

1. In Unity, go to **File → Build Profiles** (or **File → Build Settings** in older versions)
2. In the Platform list, select **Web** (or **WebGL**)
3. Click **Switch Platform** — Unity will reimport assets (this takes a few minutes)

---

### Step 8.2: Configure Player Settings

1. Go to **Edit → Project Settings → Player**
2. Click the **Web** tab (globe icon)

**Resolution and Presentation:**
| Setting | Value | Why |
|---------|-------|-----|
| Default Canvas Width | `1080` | Luxodd screen width |
| Default Canvas Height | `1920` | Luxodd screen height |
| Run in Background | `✓ Checked` | Game must keep running even if browser loses focus |
| WebGL Template | `LuxoddTemplate` | Included in the Luxodd plugin — handles session tokens |

**Publishing Settings:**
| Setting | Value | Why |
|---------|-------|-----|
| Compression Format | `Gzip` | Required by Luxodd — reduces load time |
| Name Files as Hashes | `✓ Checked` | Better caching on the arcade |
| Data Caching | `✓ Checked` | Faster reloads |
| Debug Symbols | `Off` | Smaller build, no debug info in production |

---

### Step 8.3: Set Target Frame Rate

**Where**: Add to a startup script (e.g., GameManager.cs Awake)

```csharp
void Awake()
{
    // ... existing singleton code ...

    // WebGL: let the browser control frame rate for best performance
    // -1 means "use requestAnimationFrame" which syncs to the monitor
    Application.targetFrameRate = -1;
}
```

**Why**: Setting a fixed frame rate in WebGL can cause performance issues. The browser's `requestAnimationFrame` is the most efficient approach.

---

### Step 8.4: Build the Game

1. Go to **File → Build Profiles**
2. Make sure your scene (`SampleScene`) is in the **Scenes In Build** list
   - If it's not there, click **Add Open Scenes**
3. Click **Build**
4. Choose a folder for the output (e.g., `FluxFury_WebGL/`)
5. Wait for the build to complete

**The output folder will contain:**
```
FluxFury_WebGL/
├── index.html          ← The main file Luxodd loads
├── Build/
│   ├── [hash].data.gz
│   ├── [hash].framework.js.gz
│   ├── [hash].loader.js
│   └── [hash].wasm.gz
└── TemplateData/
```

---

### Step 8.5: Test the WebGL Build Locally

1. After building, Unity should open the game in your browser automatically
2. If not, you need a local web server (browsers block local file:// WebGL):
   - Open a terminal in the build output folder
   - Run: `python -m http.server 8080` (if Python is installed)
   - Open Chrome → go to `http://localhost:8080`

3. **To test Luxodd integration**, append your dev token to the URL:
   ```
   http://localhost:8080/index.html?token=YOUR_DEV_TOKEN
   ```

---

### Step 8.6: Upload to Luxodd

1. **Zip the build folder** — make sure `index.html` is in the ROOT of the zip:
   ```
   FluxFury.zip
   ├── index.html    ← MUST be here, not inside a subfolder
   ├── Build/
   └── TemplateData/
   ```

2. **Log into Luxodd Admin Portal**: https://app.luxodd.com
3. Go to **Games → your game → Edit**
4. Upload the zip file in the **Game File** field
5. **Test on sandbox**: https://app.luxodd.com/selectGame
   - All transactions are mocked in Draft state — no real money

---

## PHASE 9: PERFORMANCE OPTIMIZATION FOR WEBGL

### Why
WebGL has specific limitations — no threads, garbage collection every frame, limited memory. These optimizations prevent lag and crashes.

---

### Step 9.1: Cache Expensive Lookups

**Problem**: `FindGameObjectsWithTag("Enemy")` in the auto-aim code runs every frame, which is slow if called constantly.

**Fix**: Cache the result and only update it periodically.

**Where**: Modify `Assets/Scripts/Input/KeyboardInputAdapter.cs` (and ArcadeInputAdapter.cs)

```csharp
// Add these fields:
private GameObject[] _cachedEnemies;
private float _enemyCacheTimer;
private const float ENEMY_CACHE_INTERVAL = 0.1f; // Update enemy list 10 times per second

// Replace FindNearestEnemyDirection with:
private Vector2 FindNearestEnemyDirection()
{
    GameObject player = GameObject.FindGameObjectWithTag("Player");
    if (player == null) return Vector2.up;

    // Only refresh the enemy list every 0.1 seconds (not every frame)
    _enemyCacheTimer -= Time.deltaTime;
    if (_enemyCacheTimer <= 0f || _cachedEnemies == null)
    {
        _cachedEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        _enemyCacheTimer = ENEMY_CACHE_INTERVAL;
    }

    if (_cachedEnemies.Length == 0) return Vector2.up;

    float closestDistance = float.MaxValue;
    Vector3 closestDirection = Vector3.up;

    foreach (GameObject enemy in _cachedEnemies)
    {
        if (enemy == null) continue;
        Vector3 toEnemy = enemy.transform.position - player.transform.position;
        toEnemy.z = 0;
        float distance = toEnemy.sqrMagnitude;
        if (distance < closestDistance)
        {
            closestDistance = distance;
            closestDirection = toEnemy;
        }
    }

    return closestDirection.normalized;
}
```

---

### Step 9.2: Cache Coroutine Yields

**Problem**: `new WaitForSeconds(x)` allocates memory every time. In WebGL, GC runs every frame, so allocations cause frame drops.

**Where**: Modify any script that uses `yield return new WaitForSeconds(...)` — especially `WaveManager.cs` and `EnemyBase.cs`

**Example in WaveManager.cs:**
```csharp
// Add cached yields as fields:
private WaitForSeconds _waitBetweenWaves;
private WaitForSeconds _waitBetweenSpawns;
private WaitForSeconds _waitFlashRed;

void Awake()
{
    // ... existing singleton code ...
    _waitBetweenWaves = new WaitForSeconds(_timeBetweenWaves);
    _waitBetweenSpawns = new WaitForSeconds(_spawnDelay);
}

// Then use them in coroutines:
private IEnumerator StartNextWave()
{
    yield return _waitBetweenWaves; // reuses cached object, no allocation
    // ...
    foreach (GameObject enemyPrefab in enemiesToSpawn)
    {
        SpawnEnemy(enemyPrefab);
        yield return _waitBetweenSpawns; // reuses cached object
    }
}
```

**Example in EnemyBase.cs:**
```csharp
// Add cached yield:
private static readonly WaitForSeconds FlashWait = new WaitForSeconds(0.1f);

private System.Collections.IEnumerator FlashRed()
{
    if (_cachedRenderer != null)
    {
        _cachedRenderer.material.color = Color.red;
        yield return FlashWait; // static cached — shared across all enemies
        if (_cachedRenderer != null)
            _cachedRenderer.material.color = _originalColor;
    }
}
```

---

### Step 9.3: Use CompareTag Instead of .tag

**Problem**: `gameObject.tag == "Enemy"` allocates a new string every call. `CompareTag` does not allocate.

**Where**: Check all scripts. Our code already uses `CompareTag` in most places (good!), but double-check.

---

### Step 9.4: Remove Debug.Log in Production

**Where**: Any script with `Debug.Log` calls

**How**: Wrap debug logs so they only run in the Editor, not in the WebGL build:

```csharp
// Option 1: Conditional compilation
#if UNITY_EDITOR
Debug.Log("This only prints in the Unity Editor, not in WebGL builds");
#endif

// Option 2: Use [System.Diagnostics.Conditional("UNITY_EDITOR")]
// on a helper method (more elegant for many log calls)
[System.Diagnostics.Conditional("UNITY_EDITOR")]
private static void DebugLog(string message)
{
    Debug.Log(message);
}
```

---

## PHASE 10: CLEANUP & SUBMISSION CHECKLIST

### Step 10.1: Delete Unused Files

- Delete `Assets/Scenes/NewEmptyCSharpScript.cs` (empty placeholder)
- Delete `Portal.cs` if portals are not part of the final game (teleportation is unimplemented)
- Remove any unused prefabs, materials, or textures

### Step 10.2: Verify All Tags Are Set

Make sure these tags exist in **Edit → Project Settings → Tags and Layers**:
- `Player` — on the player GameObject
- `Enemy` — on ALL enemy prefabs (Swarmer, Dasher, Orbiter)
- `Wall` — on arena boundary walls (set by ArenaSetup.cs)

### Step 10.3: Final Submission Checklist

Go through each item before uploading to Luxodd:

| # | Requirement | Status |
|---|-------------|--------|
| 1 | WebGL build with `index.html` in root of zip | ☐ |
| 2 | Resolution is 1080x1920 portrait (9:16) | ☐ |
| 3 | Build size ≤ 100 MB | ☐ |
| 4 | All gameplay works with joystick + buttons only (no mouse) | ☐ |
| 5 | All menus navigable with joystick (up/down + confirm button) | ☐ |
| 6 | Health check active (5-second interval) | ☐ |
| 7 | Leaderboard integrated via `SendLeaderboardRequestCommand` | ☐ |
| 8 | `BackToSystem()` called when game session ends | ☐ |
| 9 | No pause mechanic exists | ☐ |
| 10 | Score is visible during gameplay | ☐ |
| 11 | Session timer ≤ 10 minutes | ☐ |
| 12 | Game Over menu auto-exits after 30s of inactivity | ☐ |
| 13 | Game auto-starts or exits after 60s with no input | ☐ |
| 14 | Continue/Restart flow works via Luxodd API | ☐ |
| 15 | No `Debug.Log` calls in production build | ☐ |
| 16 | No `PlayerPrefs` file I/O (use Luxodd User State if needed) | ☐ |
| 17 | No `Thread.Sleep` calls (use coroutines) | ☐ |
| 18 | `Application.targetFrameRate = -1` for WebGL | ☐ |
| 19 | Sound effects present (shoot, hit, death, power-up, wave) | ☐ |
| 20 | No critical bugs or crashes | ☐ |

---

## IMPLEMENTATION ORDER (Step by Step)

Follow this order for the smoothest development process:

1. **Phase 1** (Input) → Build and test the input adapters first — everything depends on this
2. **Phase 3** (Remove Pause) → Quick change, do it early to avoid building on removed features
3. **Phase 2** (Portrait Layout) → Resize arena, camera, and UI for 9:16
4. **Phase 4** (Session Timer) → Add the 10-minute countdown
5. **Phase 5** (Menu Timeouts) → Add inactivity auto-exit
6. **Phase 6** (UI Overhaul) → Reposition all UI for portrait, add joystick navigation
7. **Phase 7** (Luxodd Plugin) → Install plugin, create session manager, connect everything
8. **Phase 9** (Performance) → Optimize before building
9. **Phase 8** (WebGL Build) → Build, test, upload
10. **Phase 10** (Cleanup & Submit) → Final checks, submit to Luxodd

Each phase can be tested independently. Use the keyboard adapter during development — you don't need the arcade hardware until final testing.

---

## FILE SUMMARY: What Gets Created / Modified

### New Files to Create:
| File | Purpose |
|------|---------|
| `Assets/Scripts/Input/IInputAdapter.cs` | Interface defining all game input actions |
| `Assets/Scripts/Input/KeyboardInputAdapter.cs` | Keyboard input for development/testing |
| `Assets/Scripts/Input/ArcadeInputAdapter.cs` | Arcade joystick+button input for Luxodd cabinet |
| `Assets/Scripts/Input/InputManager.cs` | Singleton that provides the active input adapter |
| `Assets/Scripts/Managers/LuxoddSessionManager.cs` | Handles all Luxodd server communication |

### Existing Files to Modify:
| File | Changes |
|------|---------|
| `Assets/Scripts/Player/PlayerController.cs` | Remove mouse/keyboard input, use IInputAdapter, auto-aim |
| `Assets/Scripts/Managers/GameManager.cs` | Remove pause, add session timer, add inactivity timeout, connect to Luxodd |
| `Assets/Scripts/UI/GameUI.cs` | Remove pause UI, add timer display, add button hints, portrait layout |
| `Assets/Scripts/Arena/ArenaSetup.cs` | Change from square to portrait dimensions (18x32) |
| `Assets/Scripts/Managers/WaveManager.cs` | Update spawn positions for portrait arena, connect to Luxodd level tracking |
| `Assets/Scripts/Managers/ScoreManager.cs` | Remove PlayerPrefs, use Luxodd leaderboard |
| `Assets/Scripts/Enemies/EnemyBase.cs` | Cache WaitForSeconds for WebGL performance |

### Files to Delete:
| File | Reason |
|------|--------|
| `Assets/Scenes/NewEmptyCSharpScript.cs` | Empty placeholder, unused |
| `Assets/Scripts/Arena/Portal.cs` (optional) | Teleportation not implemented |

---

*Each step above tells you exactly what file to edit, what code to change, and why. Follow the phases in order and test after each one.*
