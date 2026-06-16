using BeastTribeQuester.Model;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Numerics;

namespace BeastTribeQuester;

public sealed class ConfigWindow : Window
{
    private readonly Plugin _plugin;

    public ConfigWindow(Plugin plugin)
        : base("Beast Tribe Quester – Config###BeastTribeQuesterConfig")
    {
        _plugin = plugin;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(400, 400),
            MaximumSize = new Vector2(600, 700),
        };
    }

    public override void Draw()
    {
        var cfg = _plugin.Config;
        bool changed = false;

        // ── General ──────────────────────────────────────────────────────────
        if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
        {
            changed |= ImGui.Checkbox("Run newest expansion first (Dawntrail → ARR)", ref cfg.RunNewestFirst);
            changed |= ImGui.Checkbox("Skip tribes that are not yet unlocked",        ref cfg.SkipLockedTribes);
            changed |= ImGui.Checkbox("Stop automation if combat is detected",        ref cfg.StopOnCombat);
        }

        ImGui.Spacing();

        // ── Timing ───────────────────────────────────────────────────────────
        if (ImGui.CollapsingHeader("Timing", ImGuiTreeNodeFlags.DefaultOpen))
        {
            changed |= ImGui.SliderInt("Global step delay (ms)",   ref cfg.GlobalStepDelayMs,  0,  2000);
            changed |= ImGui.SliderInt("Post-teleport delay (ms)", ref cfg.PostTeleportDelayMs, 500, 6000);
        }

        ImGui.Spacing();

        // ── Notifications ────────────────────────────────────────────────────
        if (ImGui.CollapsingHeader("Notifications", ImGuiTreeNodeFlags.DefaultOpen))
        {
            changed |= ImGui.Checkbox("Notify when all tribes are complete", ref cfg.NotifyOnComplete);
            changed |= ImGui.Checkbox("Print each step to chat log",         ref cfg.PrintStepsToChat);
        }

        ImGui.Spacing();

        // ── Tribe filter ─────────────────────────────────────────────────────
        if (ImGui.CollapsingHeader("Tribe Filter", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.TextWrapped("Check tribes to include in the run. Uncheck all to run every tribe.");
            ImGui.Spacing();

            var allTribes = QuestDataLoader.LoadAll();
            foreach (var tribe in allTribes)
            {
                bool enabled = cfg.EnabledTribes.Count == 0
                            || cfg.EnabledTribes.Contains(tribe.TribeKey);
                bool check = enabled;
                if (ImGui.Checkbox($"{tribe.DisplayName} ({tribe.Expansion})", ref check))
                {
                    if (check)
                        cfg.EnabledTribes.Add(tribe.TribeKey);
                    else
                        cfg.EnabledTribes.Remove(tribe.TribeKey);
                    changed = true;
                }
            }
        }

        ImGui.Spacing();
        ImGui.Separator();

        if (ImGui.Button("Save & Close"))
        {
            cfg.Save();
            IsOpen = false;
        }

        ImGui.SameLine();
        if (ImGui.Button("Reset to Defaults"))
        {
            _plugin.Config.EnabledTribes.Clear();
            _plugin.Config.RunNewestFirst    = true;
            _plugin.Config.SkipLockedTribes  = true;
            _plugin.Config.StopOnCombat      = true;
            _plugin.Config.GlobalStepDelayMs = 500;
            _plugin.Config.PostTeleportDelayMs = 2500;
            _plugin.Config.NotifyOnComplete  = true;
            _plugin.Config.PrintStepsToChat  = false;
            _plugin.Config.Save();
        }

        if (changed)
            cfg.Save();
    }
}
