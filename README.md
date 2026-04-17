# ROGUE ASSASSIN

**ONE BLADE. NO MERCY. NO TRACE.**

A top-down 2D stealth game built in Unity where you play as a rogue assassin navigating through enemy territory. Avoid vision cones, take down enemies silently, and face the final boss.

---

## Gameplay

- Sneak past enemies by staying out of their red vision cones
- Get detected = instant death
- Kill enemies with your knife to earn gun ammo
- Use the gun to take out enemies from a distance
- Clear all enemies to advance to the next level

### Weapon Chain

```
Knife Kill  -->  +1 Gun Ammo
Gun Kill    -->  +1 Shotgun Ammo (Boss Fight only)
Shotgun     -->  Takes 2 Boss Lives (Boss Fight only)
```

---

## Controls

| Action | Key |
|--------|-----|
| Move | Arrow Keys |
| Knife Attack | Spacebar |
| Shoot Gun | S |
| Shotgun (Boss Fight) | A |
| Restart Level | R |

---

## Levels

### Level 1 - 4 Enemies
Learn the basics. Sneak, knife, shoot.

### Level 2 - 6 Enemies
Tighter corridors, more patrols. Plan your route carefully.

### Level 3 - Boss Fight
4 enemies + **BOSS_OMEGA** who has 3 lives, extended vision range, and roams the entire map. Use the shotgun weapon chain to take it down.

---

## Features

- Stealth gameplay with enemy vision cones that react to walls and obstacles
- Context-steering AI for smooth enemy pathfinding
- 3 lives per level with in-place respawn (dead enemies stay dead)
- Lives refill between levels
- Boss with world-space health display, invincibility frames, and respawn mechanic
- Shotgun with pellet spread that pierces through multiple enemies
- Tactical HUD with kill counter, ammo display, and heart-based lives
- Boss fight briefing screen with weapon chain instructions
- Game over screen with randomized sassy messages
- Styled main menu with controls overview

---

## Tech Stack

- **Engine:** Unity 2022.3 (2D Core)
- **Language:** C#
- **Rendering:** Sprite-based with procedural vision cone mesh

---

## Project Structure

```
Assets/
├── Scripts/
│   ├── Player/        PlayerController, PlayerCombat, PlayerStats
│   ├── Enemy/         EnemyController, VisionCone, BossController, EnemyLabel
│   ├── UI/            UIManager, WinScreen, GameOverScreen, MainMenuUI, MinimapController
│   ├── Managers/      GameManager, PauseManager, AudioManager, CameraFollow
│   ├── Effects/       BulletTrail, KnifeSlashEffect, DeathEffect, ScreenShake
│   └── Editor/        ProjectSetup (auto-builds scenes, prefabs, UI)
├── Prefabs/           Player, Enemy, Boss
├── Scenes/            MainMenu, Level1, Level2, Level3
├── Materials/         Vision cone, player, enemy, wall, obstacle materials
├── Art/               Generated sprites (circles, hearts)
└── Shaders/           Vision cone shader
```

---

## Setup

1. Clone the repo
2. Open in Unity Hub (Unity 2022.3+)
3. If scenes/prefabs are missing, go to menu: **Assassin > 1. Full Project Setup**
4. Open the **MainMenu** scene
5. Press Play

---

## Screenshots

*Main Menu*

The game features a dark tactical theme with neon green (player/friendly) and pink/red (enemy/threat) color coding.

---

## License

This project was made for a Game Jam.
