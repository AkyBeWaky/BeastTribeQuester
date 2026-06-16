# BeastTribeQuester

A Dalamud plugin for Final Fantasy XIV that automates **Beast Tribe / Allied Society daily quests**
from Dawntrail all the way down to ARR.

---

## Features

- One-click "Start" runs all enabled tribes in order (Dawntrail → ARR by default)
- Teleports to each tribe's hub via **Lifestream**, navigates via **vnavmesh**, advances dialogue via **TextAdvance**
- Per-tribe enable/disable toggle in the config window
- Combat-detection safety stop
- Configurable step and teleport delays for high-latency connections
- All quest paths stored as plain JSON — easy to tweak or contribute new tribes

---

## Required Dependencies

Install these from the Puni.sh / NightmareXIV custom repositories before using BeastTribeQuester:

| Plugin | Purpose |
|--------|---------|
| [vnavmesh](https://github.com/awgil/ffxiv_navmesh) | In-zone pathfinding and movement |
| [TextAdvance](https://github.com/NightmareXIV/TextAdvance) | Auto-advance dialogue, accept/turn-in quests |
| [Lifestream](https://github.com/NightmareXIV/Lifestream) | Aetheryte and Aethernet teleportation |

---

## Installation

1. Add the custom repository URL to XIVLauncher → Dalamud Settings → Custom Plugin Repositories.
2. Install **BeastTribeQuester** from the plugin list.
3. In-game: `/btq` to open the main window, `/btqconfig` for settings.

---

## Usage

1. Open the window with `/btq`.
2. (Optional) Open `/btqconfig` and toggle which tribes to include.
3. Click **Start**. The plugin will:
   - Teleport to each tribe's hub
   - Accept each of the 3 daily quests
   - Navigate to objectives / NPCs
   - Turn in quests
   - Move on to the next tribe
4. Click **Stop** at any time, or press ESC / manually move to interrupt.

---

## Tribe Coverage

| Expansion | Tribe | Notes |
|-----------|-------|-------|
| Dawntrail | Pelupelu | Tuliyollal hub |
| Endwalker | Omicron | Ultima Thule / Base Omicron |
| Endwalker | Loporrits | Mare Lamentorum / Bestways Burrow |
| Shadowbringers | Pixies | Il Mheg / Lydha Lran |
| Shadowbringers | Qitari | Rak'tika Greatwood / Slitherbough |
| Shadowbringers | Dwarves | Kholusia / Stilltide |
| Stormblood | Ananta | The Fringes / Ring of Ash |
| Stormblood | Namazu | Yanxia / Dhoro Iloh |
| Heavensward | Vanu Vanu | Sea of Clouds / Ok' Zundu |
| Heavensward | Moogles | Churning Mists / Zenith |
| Heavensward | Gnath | Dravanian Forelands / Lull |
| ARR | Amalj'aa | Southern Thanalan / Ring of Ash |
| ARR | Sylph | East Shroud / Little Solace |
| ARR | Kobold | Outer La Noscea / 789th Order Dig |
| ARR | Sahagin | Western La Noscea / Novv's Nursery |
| ARR | Ixal ⚒️ | North Shroud / Ehcatl — **crafting daily** |

> **Ixal note:** The Ixal tribe requires crafting. The plugin will accept and turn in quests,
> but you must have crafting materials pre-stocked in your inventory. The engine does NOT
> perform the craft itself — integrate with SomethingNeedDoing scripts for full automation.

---

## Calibrating Coordinates

The bundled JSON files use approximate coordinates. If the plugin fails to reach an NPC:

1. Open `/pdr` or a DataID inspector in-game.
2. Stand near the NPC and record its **DataID** and your character's **position** (shown in `/xldev`).
3. Edit the relevant file in `QuestPaths/<Expansion>/<Tribe>.json` and update `DataId` and `Position`.

See `QuestPaths/README.md` for the full JSON schema reference.

---

## Adding New Tribes

1. Create a new `.json` file in `QuestPaths/<Expansion>/`.
2. Follow the schema in `QuestPaths/README.md`.
3. Rebuild — the JSON is embedded at compile time.

---

## Commands

| Command | Description |
|---------|-------------|
| `/btq` | Open the main window |
| `/btqconfig` | Open the configuration window |

---

## License

MIT — contributions welcome.
