# 🎰 Multiplayer Setup Guide - Part 2: Game Scene Integration

**Project:** Elite Casino Game: Roulette  
**Date:** March 8, 2026

---

## Overview

This guide covers integrating the multiplayer system into your existing roulette game scene.

---

## Part 1: Modify Roulette Scene

### Step 1: Open Existing Scene

1. Open `Assets/Roulette Game Template/RouletteGame/Roulette/Scenes/EuropeanRoulette_mobile.unity`
2. We'll modify this scene for multiplayer

### Step 2: Add Network Components

**Create new GameObjects:**

```
EuropeanRoulette_mobile (existing scene)
├── [Existing wheel, table, etc.]
│
├── NetworkGameState (NEW GameObject)
│   └── NetworkGameState.cs
│
├── NetworkedWheel (NEW GameObject)
│   └── NetworkedWheel.cs
│   └── PhotonView (added automatically)
│
└── NetworkPlayerSpawn (NEW Empty GameObject)
    └── Transform (position for spawning players)
```

### Step 3: Configure NetworkGameState

Select the `NetworkGameState` GameObject and configure:

| Setting | Value |
|---------|-------|
| Betting Phase Duration | 15 |
| Spinning Phase Duration | 7 |
| Result Phase Duration | 3 |
| Payout Phase Duration | 2 |
| European Wheel | Drag from scene |
| American Wheel | Leave empty |

### Step 4: Configure NetworkedWheel

Select the `NetworkedWheel` GameObject:

| Setting | Value |
|---------|-------|
| European Wheel | Drag from scene |
| American Wheel | Leave empty |
| Ball Manager | Drag from scene |
| Spin Delay Before Result | 5 |

---

## Part 2: Create Multiplayer UI

### Step 1: Add Game UI Canvas

If your existing Canvas doesn't have these, create them:

```
Canvas (existing or new)
├── PhasePanel
│   ├── PhaseText (TMP_Text): "PLACE YOUR BETS"
│   ├── TimerText (TMP_Text): "15"
│   └── TimerSlider (Slider)
│
├── ResultPanel (Inactive)
│   ├── Background (Image)
│   ├── ResultNumberText (TMP_Text): "17"
│   └── ResultColorIndicator (Image)
│
├── PlayerListPanel
│   ├── Title (TMP_Text): "Players"
│   └── PlayerListContainer (Vertical Layout Group)
│
├── BettingButtons
│   ├── SpinButton (Button): "SPIN" (Master only)
│   ├── ClearButton (Button): "CLEAR"
│   ├── UndoButton (Button): "UNDO"
│   ├── RebetButton (Button): "REBET"
│   ├── DoubleButton (Button): "DOUBLE"
│   └── LeaveButton (Button): "LEAVE"
│
└── BalancePanel
    ├── BalanceText (TMP_Text): "$3,000"
    ├── TotalBetText (TMP_Text): "Bet: $0"
    └── LastWinText (TMP_Text): ""
```

### Step 2: Configure GameUIManager

Add `GameUIManager.cs` to a GameObject and assign:

| Field | Value |
|-------|-------|
| Phase Text | Drag `PhaseText` |
| Timer Text | Drag `TimerText` |
| Timer Slider | Drag `TimerSlider` |
| Result Panel | Drag `ResultPanel` |
| Result Number Text | Drag `ResultNumberText` |
| Result Color Indicator | Drag `ResultColorIndicator` |
| Player List Container | Drag `PlayerListContainer` |
| Player Entry Prefab | Assign `PlayerEntry` prefab |
| Spin Button | Drag `SpinButton` |
| Clear Button | Drag `ClearButton` |
| Undo Button | Drag `UndoButton` |
| Rebet Button | Drag `RebetButton` |
| Double Button | Drag `DoubleButton` |
| Leave Button | Drag `LeaveButton` |
| Balance Text | Drag `BalanceText` |
| Total Bet Text | Drag `TotalBetText` |
| Last Win Text | Drag `LastWinText` |

---

## Part 3: Create Player Prefab

### Step 1: Create Player Object

1. Create empty GameObject
2. Name it `NetworkPlayer`

### Step 2: Setup Structure

```
NetworkPlayer
├── Model (optional: avatar mesh)
├── NameLabel (World Space TMP)
│   └── Canvas (World Space)
│       └── Text (TMP_Text)
├── ChipAnchor (Transform - where chips appear)
└── Components:
    ├── NetworkPlayer.cs
    ├── PhotonView
    └── PhotonTransformView (optional)
```

### Step 3: Configure PhotonView

1. Add `PhotonView` component
2. Set `Ownership` to `Request`
3. If using `PhotonTransformView`:
   - Enable `Synchronize Position`
   - Enable `Synchronize Rotation`

### Step 4: Create Prefab

1. Drag to `Assets/Prefabs/Network/`
2. Assign this prefab to `NetworkManager` → `Player Prefab`

---

## Part 4: Modify Existing Scripts

### Modify BetSpace.cs

Add to `Start()` method:

```csharp
void Start()
{
    // ... existing code ...
    
    mesh = GetComponent<MeshRenderer>();
    if (mesh) mesh.enabled = false;
    
    stack = Cloth.InstanceStack();
    stack.SetInitialPosition(transform.position);
    stack.transform.SetParent(transform);
    stack.transform.localPosition = Vector3.zero;
    ResultManager.RegisterBetSpace(this);
    
    // NEW: Register for networked betting
    if (PhotonNetwork.InRoom)
    {
        int index = BetSpaceRegistry.GetAllBetSpaces().Count;
        BetSpaceRegistry.RegisterBetSpace(this, index);
    }
}
```

### Modify OnMouseUp() for Multiplayer

```csharp
private void OnMouseUp()
{
    float selectedValue = ChipManager.GetSelectedValue();
    
    // NEW: Check if in multiplayer mode
    if (PhotonNetwork.InRoom)
    {
        // Get bet space index
        int betSpaceIndex = BetSpaceRegistry.GetIndexOfBetSpace(this);
        
        // Place bet through NetworkPlayer
        if (NetworkPlayer.LocalPlayer != null)
        {
            NetworkPlayer.LocalPlayer.PlaceBetOnSpace(betSpaceIndex, selectedValue);
        }
    }
    else
    {
        // Single player - original logic
        ApplyBet(selectedValue);
    }
    
    ToolTipManager.SelectTarget(stack);
}
```

---

## Part 5: Scene Transitions

### Update NetworkManager

In `NetworkManager.cs`, update scene names:

```csharp
[SerializeField] private string lobbyScene = "Lobby";
[SerializeField] private string rouletteScene = "EuropeanRoulette_mobile";
```

### Add to All Roulette Scenes

Repeat this setup for:
- `AmericanRoulette_mobile.unity`
- `EuropeanRoulette_desktop.unity`
- `AmericanRoulette_desktop.unity`

---

## Checklist

- [ ] NetworkGameState added to scene
- [ ] NetworkedWheel added to scene
- [ ] GameUIManager configured
- [ ] Player prefab created
- [ ] BetSpace.cs modified
- [ ] All scenes updated

---

*Continue to Part 3: Testing & Troubleshooting*
