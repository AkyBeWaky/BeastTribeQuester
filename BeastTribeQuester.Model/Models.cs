using System.Numerics;
using System.Text.Json.Serialization;

namespace BeastTribeQuester.Model;

// ─────────────────────────────────────────────────────────
//  Enumerations
// ─────────────────────────────────────────────────────────

public enum EInteractionType
{
    None,
    /// <summary>Walk/fly to a world-space position.</summary>
    MoveTo,
    /// <summary>Interact with an NPC or object (opens dialogue).</summary>
    TalkTo,
    /// <summary>Accept a quest from an NPC.</summary>
    AcceptQuest,
    /// <summary>Turn in a completed quest to an NPC.</summary>
    TurnInQuest,
    /// <summary>Gather a node (mining/botany).</summary>
    Gather,
    /// <summary>Teleport via an Aetheryte or Aethernet shard.</summary>
    Teleport,
    /// <summary>Wait for a flag/condition to be true before proceeding.</summary>
    WaitForCondition,
    /// <summary>Equip a specific job before the next step.</summary>
    SwitchJob,
    /// <summary>Use an item from inventory (e.g. quest key items).</summary>
    UseItem,
}

public enum EExpansion
{
    ARR = 0,
    Heavensward = 1,
    Stormblood = 2,
    Shadowbringers = 3,
    Endwalker = 4,
    Dawntrail = 5,
}

// ─────────────────────────────────────────────────────────
//  Core step / sequence types
// ─────────────────────────────────────────────────────────

/// <summary>
/// A single automation step within a quest.
/// </summary>
public sealed class QuestStep
{
    /// <summary>What the automation engine should do at this step.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EInteractionType InteractionType { get; set; } = EInteractionType.None;

    /// <summary>
    /// Target world-space position. Used for MoveTo, TalkTo, AcceptQuest,
    /// TurnInQuest, Gather steps.
    /// </summary>
    public Vector3? Position { get; set; }

    /// <summary>Territory/zone ID (ClientStructs TerritoryType row id).</summary>
    public uint? TerritoryId { get; set; }

    /// <summary>
    /// NPC data-id for TalkTo / AcceptQuest / TurnInQuest steps.
    /// (The value you see in /pdr or ACT's "DataID" column.)
    /// </summary>
    public uint? DataId { get; set; }

    /// <summary>Aetheryte row id for Teleport steps.</summary>
    public uint? AetheryteId { get; set; }

    /// <summary>Job/class id for SwitchJob steps (Lumina ClassJob RowId).</summary>
    public uint? ClassJobId { get; set; }

    /// <summary>Item id for UseItem steps.</summary>
    public uint? ItemId { get; set; }

    /// <summary>
    /// Gathering node data-id.  When set the engine will invoke
    /// vnavmesh to approach and then trigger the gathering mini-game.
    /// </summary>
    public uint? GatheringNodeDataId { get; set; }

    /// <summary>
    /// Human-readable comment shown in the automation UI – not used by the engine.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// How long (ms) to wait after completing this step before starting the next.
    /// Defaults to 500 ms if null.
    /// </summary>
    public int? DelayAfterMs { get; set; }

    /// <summary>
    /// If true the engine will mount before navigating to the position
    /// (ignored if already mounted or indoors).
    /// </summary>
    public bool Mount { get; set; } = false;

    /// <summary>
    /// If true the engine will sprint to the target position
    /// (calls the Sprint action automatically).
    /// </summary>
    public bool Sprint { get; set; } = false;
}

/// <summary>
/// A single quest that belongs to a tribe's daily rotation.
/// </summary>
public sealed class BeastTribeQuest
{
    /// <summary>FFXIV quest id (Lumina Quest sheet RowId).</summary>
    public uint QuestId { get; set; }

    /// <summary>Human-readable quest name (for logging / UI display).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Ordered list of automation steps.</summary>
    public List<QuestStep> Steps { get; set; } = [];
}

/// <summary>
/// All daily quests for one Beast Tribe (one entry per expansion tribe).
/// </summary>
public sealed class BeastTribeDefinition
{
    /// <summary>Internal key used in JSON file names, e.g. "Amalj'aa".</summary>
    public string TribeKey { get; set; } = string.Empty;

    /// <summary>Display name shown in the plugin UI.</summary>
    public string DisplayName { get; set; } = string.Empty;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EExpansion Expansion { get; set; }

    /// <summary>
    /// Aetheryte to teleport to when starting the daily run
    /// (nearest to the tribe's quest hub).
    /// </summary>
    public uint StartAetheryteId { get; set; }

    /// <summary>
    /// Territory the tribe's NPC hub is located in.
    /// Used to check whether we're already in the right zone.
    /// </summary>
    public uint HubTerritoryId { get; set; }

    /// <summary>
    /// The tribe's daily allowance quest ids that the plugin tracks
    /// (set by the game per day; 3 quests for most tribes).
    /// </summary>
    public List<BeastTribeQuest> DailyQuests { get; set; } = [];
}
