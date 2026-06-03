# 14er Critter Quest - Setup Guide

## Quick Start (3 steps)

1. Open **Unity Hub** > **Add** > **Add project from disk** > select `14er-critter-quest`
2. Unity version: **2022.3 LTS** or newer
3. Once imported, go to menu: **14er Critter Quest > Create Bootstrap Scene (Play Immediately)**
4. Press **Play**

That's it. The entire level — mountain terrain, critters, HUD, weather — generates at runtime. No manual scene setup needed.

## Optional: Ground Layer Setup

For proper collision detection, run: **14er Critter Quest > Setup Ground Layer**

This creates a "Ground" layer in your project settings. Without it the game uses default collision (still playable, just less precise).

## Controls

| Key | Action |
|-----|--------|
| WASD / Arrows | Move |
| Space | Jump (costs stamina) |
| Tab | Toggle camera mode |
| Mouse + Click | Aim and snap photo (in camera mode) |
| Scroll Wheel | Zoom in/out (in camera mode) |
| G | Open Field Guide |

## What Happens When You Hit Play

**GameBootstrap.cs** creates everything programmatically:

- **Player**: Pixel-art hiker with movement, wall-slide, stamina, squash/stretch, dust FX
- **Mountain**: Procedurally generated platforms forming a climbable mountain with forest > alpine > tundra > summit biome transitions, trees, rocks, and a trail sign at the base
- **Critters**: 12 animals with AI (idle, wander, flee, silly actions, posing), speech bubbles showing what they're doing, spawned on platforms throughout the mountain
- **Photo System**: Tab to enter camera mode with viewfinder UI (corner brackets, crosshair, vignette), click to snap, scored by distance/rarity/silly-action, flash + screen shake on snap
- **HUD**: Stamina bar, altitude readout (feet), field guide counter, photo rating popups, camera mode indicator, controls hint
- **Weather**: Snow particles above the snowline, wind gusts that increase with altitude, drifting clouds
- **Audio**: Procedurally generated placeholder sounds (jump, land, shutter, discovery chime, summit fanfare)
- **Backgrounds**: Multi-layer parallax mountain silhouettes

## Architecture (28 scripts)

```
Scripts/
├── Player/
│   ├── PlayerController.cs      # Movement, jump, wall-slide, coyote time, input buffer, stamina
│   └── PlayerFX.cs              # Squash/stretch, dust particles, landing impact
├── Camera/
│   └── CameraFollow.cs          # Smooth follow with look-ahead and bounds
├── Critters/
│   ├── CritterData.cs           # ScriptableObject definition
│   ├── Critter.cs               # AI: idle, wander, flee, silly action, pose (personality-driven)
│   ├── CritterSpawner.cs        # Altitude-based weighted spawning
│   └── CritterSpeechBubble.cs   # Floating text for silly actions
├── Photo/
│   ├── PhotoCamera.cs           # Viewfinder, snap, distance/rarity/silly scoring
│   ├── FieldGuide.cs            # Collection tracking + PlayerPrefs persistence
│   ├── CameraFlash.cs           # Flash overlay + camera shake on snap
│   └── ViewfinderUI.cs          # Corner brackets, crosshair, vignette, zoom display
├── Game/
│   ├── GameBootstrap.cs         # Creates entire playable scene at runtime
│   ├── GameManager.cs           # Score, peak progression, save/load
│   ├── SummitTrigger.cs         # Summit celebration with absurd messages
│   ├── CritterDatabase.cs       # All 12 critter templates with names/titles/entries
│   ├── PeakDatabase.cs          # 8 starting peaks with elevation/flavor text
│   ├── SpriteFactory.cs         # Procedural pixel-art sprites (hiker, marmot, goat, pika, llama)
│   └── AudioManager.cs          # Procedural placeholder sound effects
├── Level/
│   ├── ProceduralMountain.cs    # Generates climbable mountain terrain at runtime
│   ├── ParallaxBackground.cs    # Multi-layer parallax scrolling
│   ├── AltitudeZone.cs          # Biome transitions (forest→alpine→tundra→summit)
│   └── WeatherSystem.cs         # Snow, wind gusts, drifting clouds
├── UI/
│   ├── HUDManager.cs            # Stamina, altitude, photo feedback, discovery popups
│   ├── FieldGuideUI.cs          # Collection browser (pauses game)
│   ├── PeakSelectUI.cs          # Mountain picker for multi-peak progression
│   └── MainMenuUI.cs            # Title screen with rotating taglines
└── Editor/
    ├── SceneSetup.cs            # One-click bootstrap scene creation + ground layer setup
    └── CritterCreator.cs        # Bulk ScriptableObject generation
```

## The Critters

| Name | Species | Rarity | Personality | Signature Move |
|------|---------|--------|-------------|----------------|
| Gerald | Marmot | Common | Bold | Judges your outfit |
| Beatrice | Pika | Common | Shy | Adds to enormous hay pile |
| Gremlin | Chickaree | Common | Chaotic | Throws pinecones at you |
| Reginald | Mountain Goat | Uncommon | Bold | Strokes own beard |
| Chef Boyar-Dee | Ptarmigan | Uncommon | Curious | Critiques your trail mix |
| Professor Whiskers | Yellow-Bellied Marmot | Uncommon | Curious | Grades your climbing technique |
| Big Tony | Bighorn Sheep | Rare | Bold | Offers "trail insurance" |
| Karen | Elk | Rare | Chaotic | Demands to see the manager |
| Brenda & Carl | Clark's Nutcrackers | Rare | Chaotic | Bicker about nest decor |
| Steve | Llama | Legendary | Chaotic | Vibes existentially in a sweater |
| Duchess Von Floof | White Ptarmigan | Legendary | Shy | Refuses to acknowledge you |
| The Council | Three Pikas in a Trenchcoat | Legendary | Curious | Introduces self as "one normal pika" |

## Next Steps

- [ ] Replace procedural sprites with real pixel art
- [ ] Add more peaks (all 58 Colorado 14ers)
- [ ] Steam SDK integration (Steamworks.NET) + achievements
- [ ] Music and real sound effects
- [ ] Weather system affecting critter spawns (rare critters in specific conditions)
- [ ] Tilemap-based levels for hand-crafted mountain routes
- [ ] Save system upgrade (JSON file instead of PlayerPrefs)
- [ ] Main menu + peak select scenes
