# 🎰 UI Toolkit Setup Guide

**Project:** Elite Casino Game: Roulette  
**System:** In-Game Multiplayer Invite/Join UI  
**Last Updated:** March 24, 2026

---

## 📋 Overview

This guide covers setting up the multiplayer UI using Unity's **UI Toolkit** (UXML/USS) for an in-game invite/join system.

---

## 🎯 System Design

**Key Features:**
- In-game invite system (no separate lobby scene)
- Players can join ongoing sessions via room code
- Table settings configurable (stakes, min/max bet, starting balance)
- Real-time player list display
- Phase timer and game state indicators

---

## 📁 File Structure

```
UI Toolkit/
├── Theme.uss                    # Shared styles
├── Lobby/
│   ├── LobbyPanel.uxml         # Main invite/join panel
│   ├── LobbyPanel.uss          # Lobby styles
│   ├── PlayerEntry.uxml        # Player list item
│   └── TableEntry.uxml         # Available tables list item
└── GameHUD/
    ├── GameHUD.uxml            # In-game HUD
    ├── GameHUD.uss             # HUD styles
    └── PlayerHUDEntry.uxml     # Player HUD item

Scripts/Multiplayer/UI Toolkit/
├── LobbyPanelController.cs     # Invite/join panel logic
└── GameHUDController.cs        # In-game HUD logic
```

---

## 🔧 Unity Setup Steps

### Step 1: Import UXML/USS Files

1. Copy all `.uxml` and `.uss` files to:
   - `Assets/Roulette Game Template/RouletteGame/Roulette/UI Toolkit/`

2. Unity will auto-import them

### Step 2: Create UI Document

1. In Hierarchy, right-click → UI → UI Document
2. Name it `MultiplayerUI`
3. Set Panel Settings:
   - Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920x1080

4. Add USS reference:
   - Drag `Theme.uss` to Panel Settings → Theme Style Sheet

### Step 3: Setup LobbyPanel

1. Select `MultiplayerUI` UI Document
2. In UI Toolkit component:
   - Source Asset: `LobbyPanel.uxml`
   - Add `LobbyPanel.uss` to Panel Settings

### Step 4: Add Controllers

1. Add `LobbyPanelController.cs` to UI Document GameObject
2. Assign references:
   - UI Document: Your UIDocument component
   - Player Entry Asset: `PlayerEntry.uxml`
   - Table Entry Asset: `TableEntry.uxml`

---

## 🎮 In-Game HUD Setup

### Step 1: Create Separate UI Document

1. Create new UI Document: `GameHUD`
2. Source Asset: `GameHUD.uxml`
3. Add `GameHUD.uss` to style sheets

### Step 2: Add Controller

1. Add `GameHUDController.cs`
2. Assign:
   - UI Document: Your UIDocument
   - Player HUD Entry Asset: `PlayerHUDEntry.uxml`

---

## 🔗 Integration with Roulette Scene

### Add to EuropeanRoulette_mobile Scene

```
EuropeanRoulette_mobile
├── [Existing elements]
├── MultiplayerUI (UIDocument)
│   └── LobbyPanelController.cs
└── GameHUD (UIDocument)
    └── GameHUDController.cs
```

---

## 📱 UI Panels Reference

### LobbyPanel (Invite/Join)

**Top Section:**
- Connection status indicator
- Ping display

**Left Side:**
- Current table info
- Stakes dropdown (Low/Medium/High)
- Min/Max bet inputs
- Starting balance input
- Room code display + copy button
- Invite & share buttons

**Right Side:**
- Join by code input
- Quick join button
- Available tables list

**Bottom:**
- Create private table
- Leave table
- Start game (Master only)

### GameHUD (In-Game)

**Top Bar:**
- Phase indicator (BETTING/SPINNING/RESULT)
- Timer bar with countdown
- Player count

**Right Panel:**
- Players list (compact)
- Invite button

**Bottom Bar:**
- Balance display
- Bet amount
- Action buttons (Clear/Undo/Rebet/2x)
- Room code display

**Popups:**
- Result popup (win/lose)
- Invite panel (share code)

---

## 🎨 Theme Colors

| Color | Hex | Usage |
|-------|-----|-------|
| Gold Primary | #D4AF37 | Titles, buttons, accents |
| Gold Light | #E8C34B | Hover states |
| Background Dark | #14141E | Panel backgrounds |
| Card Background | #28283280 | Card containers |
| Text Primary | #FFFFFF | Main text |
| Text Secondary | #FFFFFFB2 | Subdued text |
| Success | #64FF64 | Positive indicators |
| Danger | #FF6464 | Negative indicators |

---

## ⚡ Script API

### LobbyPanelController

```csharp
// Show/hide panel
lobbyPanelController.ShowPanel();
lobbyPanelController.HidePanel();
lobbyPanelController.TogglePanel();

// Update room info
UpdateRoomInfo();
UpdatePlayerList();
```

### GameHUDController

```csharp
// Update display
gameHUDController.UpdateBalance(3000, 50);
gameHUDController.UpdatePhase(GamePhase.Betting, 15);
gameHUDController.UpdateTimer(12.5f);

// Show results
gameHUDController.ShowResult(17, 350, true);

// Show invite
gameHUDController.ShowInvitePanel();
```

---

## ✅ Checklist

- [ ] UXML/USS files imported
- [ ] UI Documents created
- [ ] Controllers attached
- [ ] References assigned
- [ ] Theme.uss applied
- [ ] Tested in scene

---

*UI Toolkit implementation complete*
