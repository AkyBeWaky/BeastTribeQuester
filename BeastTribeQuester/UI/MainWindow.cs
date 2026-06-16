using BeastTribeQuester.Model;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Numerics;

namespace BeastTribeQuester;

public sealed class MainWindow : Window
{
    private readonly Plugin                    _plugin;
    private readonly MasterRunner              _runner;
    private readonly List<BeastTribeDefinition> _allTribes;

    // Colour constants
    private static readonly Vector4 ColGreen  = new(0.40f, 0.85f, 0.40f, 1f);
    private static readonly Vector4 ColRed    = new(0.90f, 0.35f, 0.35f, 1f);
    private static readonly Vector4 ColYellow = new(0.95f, 0.85f, 0.25f, 1f);
    private static readonly Vector4 ColGray   = new(0.55f, 0.55f, 0.55f, 1f);

    public MainWindow(Plugin plugin, MasterRunner runner, List<BeastTribeDefinition> allTribes)
        : base("Beast Tribe Quester###BeastTribeQuesterMain",
               ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        _plugin    = plugin;
        _runner    = runner;
        _allTribes = allTribes;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(380, 260),
            MaximumSize = new Vector2(700, 800),
        };
    }

    public override void Draw()
    {
        // ── Status bar ───────────────────────────────────────────────────────
        var statusColour = _runner.IsRunning  ? ColGreen
                         : _runner.IsFinished ? ColYellow
                                              : ColGray;
        ImGui.TextColored(statusColour, $"Status: {_runner.Status}");

        ImGui.Separator();

        // ── Start / Stop buttons ─────────────────────────────────────────────
        if (_runner.IsRunning)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, ColRed with { W = 0.8f });
            if (ImGui.Button("  Stop  ", new Vector2(120, 0)))
                _runner.Stop();
            ImGui.PopStyleColor();
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.Button, ColGreen with { W = 0.7f });
            if (ImGui.Button("  Start  ", new Vector2(120, 0)))
                _runner.Start();
            ImGui.PopStyleColor();
        }

        ImGui.SameLine();
        if (ImGui.Button("Config"))
            _plugin.PluginInterface.UiBuilder.OpenConfigUi();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // ── Tribe list ───────────────────────────────────────────────────────
        ImGui.TextColored(ColYellow, "Available Tribes");
        ImGui.Spacing();

        if (ImGui.BeginTable("tribeTable", 3,
            ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("Tribe",     ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Expansion", ImGuiTableColumnFlags.WidthFixed, 110);
            ImGui.TableSetupColumn("Quests",    ImGuiTableColumnFlags.WidthFixed,  60);
            ImGui.TableHeadersRow();

            foreach (var tribe in _allTribes)
            {
                var enabled = _plugin.Config.EnabledTribes.Count == 0
                           || _plugin.Config.EnabledTribes.Contains(tribe.TribeKey);

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                if (!enabled)
                    ImGui.PushStyleColor(ImGuiCol.Text, ColGray);

                ImGui.Text(tribe.DisplayName);

                if (!enabled)
                    ImGui.PopStyleColor();

                ImGui.TableSetColumnIndex(1);
                ImGui.TextColored(ExpansionColour(tribe.Expansion), tribe.Expansion.ToString());

                ImGui.TableSetColumnIndex(2);
                ImGui.Text($"{tribe.DailyQuests.Count}");
            }

            ImGui.EndTable();
        }

        ImGui.Spacing();
        ImGui.TextColored(ColGray, $"Total tribes loaded: {_allTribes.Count}");
    }

    private static Vector4 ExpansionColour(EExpansion exp) => exp switch
    {
        EExpansion.Dawntrail     => new Vector4(0.60f, 0.90f, 1.00f, 1f),
        EExpansion.Endwalker     => new Vector4(0.70f, 0.80f, 1.00f, 1f),
        EExpansion.Shadowbringers => new Vector4(0.75f, 0.55f, 0.90f, 1f),
        EExpansion.Stormblood    => new Vector4(0.90f, 0.50f, 0.50f, 1f),
        EExpansion.Heavensward   => new Vector4(0.70f, 0.85f, 1.00f, 1f),
        EExpansion.ARR           => new Vector4(0.95f, 0.80f, 0.45f, 1f),
        _                        => Vector4.One
    };
}
