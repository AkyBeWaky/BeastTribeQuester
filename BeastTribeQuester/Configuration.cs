using Dalamud.Configuration;
using System.Text.Json.Serialization;

namespace BeastTribeQuester;

[Serializable]
public sealed class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    // ── General ──────────────────────────────────────────────────────────────
    /// <summary>
    /// Tribes to include in the daily automation run, keyed by TribeKey.
    /// If empty, all known tribes are attempted.
    /// </summary>
    public HashSet<string> EnabledTribes { get; set; } = [];

    /// <summary>Run the tribes in Dawntrail → ARR order (default) or reverse.</summary>
    public bool RunNewestFirst { get; set; } = true;

    // ── Timing ───────────────────────────────────────────────────────────────
    /// <summary>Extra delay added after each step (ms). Increase on high-latency connections.</summary>
    public int GlobalStepDelayMs { get; set; } = 500;

    /// <summary>How long to wait after teleporting before moving (ms).</summary>
    public int PostTeleportDelayMs { get; set; } = 2500;

    // ── Safety ───────────────────────────────────────────────────────────────
    /// <summary>Pause the run if combat is detected.</summary>
    public bool StopOnCombat { get; set; } = true;

    /// <summary>Skip a tribe if its quests are not yet unlocked, instead of erroring.</summary>
    public bool SkipLockedTribes { get; set; } = true;

    // ── Notifications ────────────────────────────────────────────────────────
    public bool NotifyOnComplete { get; set; } = true;
    public bool PrintStepsToChat { get; set; } = false;

    [JsonIgnore]
    private static IDalamudPluginInterface? _pi;

    public static Configuration Load(IDalamudPluginInterface pi)
    {
        _pi = pi;
        return pi.GetPluginConfig() as Configuration ?? new Configuration();
    }

    public void Save() => _pi?.SavePluginConfig(this);
}
