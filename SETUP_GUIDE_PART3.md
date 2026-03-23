# 🎰 Multiplayer Setup Guide - Part 3: Testing & Troubleshooting

**Project:** Elite Casino Game: Roulette  
**Date:** March 8, 2026

---

## Overview

This guide covers testing your multiplayer implementation and troubleshooting common issues.

---

## Part 1: Testing Setup

### Step 1: Photon Server Settings

1. Go to `Assets/Photon/PhotonUnityNetworking/Resources/PhotonServerSettings.asset`
2. Verify your AppId is set
3. Check these settings:

| Setting | Recommended Value |
|---------|-------------------|
| AppId | Your Photon App ID |
| AppVersion | "1.0" |
| Region | Select closest to your players |
| Protocol | UDP |
| Send Rate | 30 |

### Step 2: Enable Debug Logging

In `NetworkManager.cs`, set:

```csharp
[SerializeField] private bool isDebugMode = true;
```

This enables:
- Auto-connect on start
- Detailed console logs

---

## Part 2: Single-Client Testing

### Test 1: Connection Flow

1. Open `Launcher` scene
2. Enter Play Mode
3. **Expected Results:**
   - Connection panel shows
   - Enter name and click Play
   - "Connecting..." panel appears
   - Lobby scene loads

### Test 2: Room Creation

1. In Lobby scene
2. Click "Create Room"
3. Enter room name
4. Select table stakes
5. Click "Create"
6. **Expected Results:**
   - Room is created
   - Game scene loads
   - Player spawns in scene

### Test 3: Betting Phase

1. In game scene
2. Select a chip value
3. Click on a betting space
4. **Expected Results:**
   - Balance decreases
   - Chip appears on betting space
   - Total bet updates

---

## Part 3: Multi-Client Testing

### Step 1: Build the Game

1. Go to **File → Build Settings**
2. Ensure all scenes are added
3. Click **Build** (not Build And Run)
4. Save build to a folder

### Step 2: Test with Editor + Build

1. Open `Launcher` scene in Editor
2. Enter Play Mode
3. Run the built executable
4. Test from both:

**Editor:**
- Enter name: "Player1"
- Create a room
- Wait in lobby

**Build:**
- Enter name: "Player2"  
- Join the same room
- Both should see each other

### Step 3: Test Game Sync

| Action | Expected Result |
|--------|-----------------|
| Player1 places bet | Player2 sees chip appear |
| Timer counts down | Same time on both clients |
| Wheel spins | Same result on both clients |
| Winner announced | Both see same result |

---

## Part 4: Troubleshooting

### Issue: "Cannot connect to Photon"

**Causes & Fixes:**

1. **Invalid AppId**
   - Check PhotonServerSettings.asset
   - Verify AppId from Photon Dashboard

2. **Region not set**
   - Set a specific region in settings
   - Try different regions

3. **Firewall blocking**
   - Allow Unity through firewall
   - Check port 5055-5057 (UDP)

### Issue: "JoinRandomFailed"

**Cause:** No rooms available

**Fix:** This is handled automatically - creates new room

### Issue: "Bets not syncing"

**Causes & Fixes:**

1. **NetworkPlayer not spawned**
   - Check prefab is assigned to NetworkManager
   - Check prefab has PhotonView

2. **BetSpace not registered**
   - Call `BetSpaceRegistry.Initialize()` on scene load
   - Check BetSpace.Start() registers properly

### Issue: "Wheel spins different on each client"

**Cause:** Not using NetworkedWheel

**Fix:**
1. Add NetworkedWheel component
2. Ensure Master Client generates result
3. Result broadcasts via RaiseEvent

### Issue: "Player balance not syncing"

**Cause:** Using local BalanceManager

**Fix:**
- Use NetworkPlayer.Balance instead
- Replace BalanceManager calls

---

## Part 5: Debug Tools

### Add Debug UI (Optional)

Create a debug panel:

```
DebugPanel (only in debug builds)
├── ConnectionStatusText: "Connected"
├── RoomNameText: "Room_1234"
├── PlayerCountText: "2/4"
├── PhaseText: "Betting"
├── ResultText: "Last: 17"
└── PingText: "45ms"
```

### Useful Debug Commands

Add to Update() for testing:

```csharp
// Quick start round (testing only)
if (Input.GetKeyDown(KeyCode.Space) && PhotonNetwork.IsMasterClient)
{
    NetworkGameState.Instance.StartNewRound();
}

// Add test balance
if (Input.GetKeyDown(KeyCode.B))
{
    NetworkPlayer.LocalPlayer?.AddBalance(1000);
}
```

---

## Part 6: Performance Tips

### Optimize Network Traffic

1. **Reduce Send Rate** (if needed)
   - Default: 30 sends/sec
   - For roulette, 10-15 may be enough

2. **Use Interest Groups**
   - Only sync what players need
   - Bets on table = all players
   - Other player details = lower priority

3. **Serialize Efficiently**
   - Use byte for small numbers
   - Use Photon's built-in serialization

---

## Checklist

- [ ] Photon AppId configured
- [ ] Debug mode enabled for testing
- [ ] Connection flow works
- [ ] Room creation works
- [ ] Betting syncs across clients
- [ ] Timer syncs across clients
- [ ] Wheel result same for all players

---

## Common Error Codes

| Error | Meaning | Fix |
|-------|---------|-----|
| 32750 | Invalid Region | Select valid region |
| 32751 | Invalid AppId | Check PhotonServerSettings |
| 32752 | Invalid AppVersion | Match versions |
| 32760 | Server Full | Try again later |
| 32765 | Custom Auth Failed | Check PlayFab settings |

---

## Next Steps

After testing works:

1. **P1 Features:**
   - Betting timer UI polish
   - Server-authoritative balance
   - Table stake selection

2. **P2 Features:**
   - XP/Progression system
   - Daily rewards
   - Double bet button

3. **Polish:**
   - Sound effects for network events
   - Win animations synced
   - Chat system (optional)

---

*Setup complete! Refer to Photon documentation for advanced features.*
