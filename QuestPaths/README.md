# BeastTribeQuester – Quest Path Data

This folder contains JSON quest path definitions for all Beast Tribe / Allied Society daily quests,
organized by expansion.

## ⚠️ Important: Coordinate Calibration Required

The coordinates (X, Y, Z positions) and DataIDs in these JSON files are **approximate reference
values**. Before running the plugin on a new tribe, you should verify them in-game using the
included coordinate-capture workflow:

1. Stand next to the quest NPC and use `/pdr` (PositionalDebugger) or any DataID inspector to
   confirm the NPC's DataID.
2. Note your character's exact coordinates (shown in `/xldev` → "About" or via PositionalDebugger).
3. Update the corresponding JSON file with the correct values.

The engine uses a **2.5-yalm arrival radius**, so minor coordinate imprecision is fine for NPCs
in open areas. Tight indoor hubs may need more precise values.

---

## Tribe Coverage

| Expansion     | Tribe          | File                                |
|---------------|----------------|-------------------------------------|
| Dawntrail     | Pelupelu       | Dawntrail/Pelupelu.json             |
| Endwalker     | Omicron        | Endwalker/Omicron.json              |
| Endwalker     | Loporrits      | Endwalker/Loporrits.json            |
| Shadowbringers | Pixies        | Shadowbringers/Pixies.json          |
| Shadowbringers | Qitari        | Shadowbringers/Qitari.json          |
| Shadowbringers | Dwarves       | Shadowbringers/Dwarves.json         |
| Stormblood    | Ananta         | Stormblood/Ananta.json              |
| Stormblood    | Namazu         | Stormblood/Namazu.json              |
| Heavensward   | Vanu Vanu      | Heavensward/VanuVanu.json           |
| Heavensward   | Moogles        | Heavensward/Moogles.json            |
| Heavensward   | Gnath          | Heavensward/Gnath.json              |
| ARR           | Amalj'aa       | ARR/Amaljaa.json                    |
| ARR           | Sylph          | ARR/Sylph.json                      |
| ARR           | Kobold         | ARR/Kobold.json                     |
| ARR           | Sahagin        | ARR/Sahagin.json                    |
| ARR           | Ixal (crafting)| ARR/Ixal.json                       |

> **Note:** HW also has Vath (Dravanian Hinterlands, Mogmill) – add `Heavensward/Vath.json` using
> the same schema. EW also has Arkasodara (Thavnair) – add `Endwalker/Arkasodara.json`.
> DT also has Pelupelu and Xbr'aal – add additional DT files as you unlock them.

---

## JSON Schema Reference

```jsonc
{
  "TribeKey":        "string  – unique internal key",
  "DisplayName":     "string  – shown in the UI",
  "Expansion":       "ARR | Heavensward | Stormblood | Shadowbringers | Endwalker | Dawntrail",
  "StartAetheryteId": 0,     // Aetheryte to teleport to at the start of the run
  "HubTerritoryId":   0,     // TerritoryType RowId of the quest hub zone
  "DailyQuests": [
    {
      "QuestId": 0,           // Lumina Quest sheet RowId
      "Name": "string",
      "Steps": [
        {
          "InteractionType": "Teleport | MoveTo | AcceptQuest | TalkTo | TurnInQuest | Gather | SwitchJob | WaitForCondition | UseItem",
          "Position":        { "X": 0.0, "Y": 0.0, "Z": 0.0 },  // world-space
          "TerritoryId":     0,     // required for Teleport completion check
          "DataId":          0,     // NPC data id (from /pdr or equivalent)
          "AetheryteId":     0,     // for Teleport steps
          "ClassJobId":      0,     // for SwitchJob steps
          "GatheringNodeDataId": 0, // for Gather steps
          "DelayAfterMs":    500,   // extra wait after this step
          "Mount":           false, // mount before navigating
          "Sprint":          false, // sprint to target
          "Comment":         "string – human notes, ignored by engine"
        }
      ]
    }
  ]
}
```
