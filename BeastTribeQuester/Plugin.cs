using BeastTribeQuester.Ipc;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace BeastTribeQuester;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "BeastTribeQuester";

    private const string CommandMain   = "/btq";
    private const string CommandConfig = "/btqconfig";

    // ── Dalamud services ─────────────────────────────────────────────────────
    internal readonly IDalamudPluginInterface PluginInterface;
    internal readonly ICommandManager         Commands;
    internal readonly IFramework              Framework;
    internal readonly IClientState            ClientState;
    internal readonly ICondition              Condition;
    internal readonly IPluginLog              Log;
    internal readonly IChatGui                Chat;

    // ── Plugin-owned objects ─────────────────────────────────────────────────
    internal readonly Configuration Config;
    internal readonly VnavmeshIpc   Vnavmesh;
    internal readonly LifestreamIpc Lifestream;

    private readonly MasterRunner   _runner;
    private readonly WindowSystem   _windowSystem = new("BeastTribeQuester");
    private readonly MainWindow     _mainWindow;
    private readonly ConfigWindow   _configWindow;

    public Plugin(
        IDalamudPluginInterface pi,
        ICommandManager         commands,
        IFramework              framework,
        IClientState            clientState,
        ICondition              condition,
        IPluginLog              log,
        IChatGui                chat)
    {
        PluginInterface = pi;
        Commands        = commands;
        Framework       = framework;
        ClientState     = clientState;
        Condition       = condition;
        Log             = log;
        Chat            = chat;

        Config     = Configuration.Load(pi);
        Vnavmesh   = new VnavmeshIpc(pi);
        Lifestream = new LifestreamIpc(pi);

        var allTribes = QuestDataLoader.LoadAll();
        _runner = new MasterRunner(
            allTribes, Vnavmesh, Lifestream,
            ClientState, Condition, Log, Config);

        _mainWindow   = new MainWindow(this, _runner, allTribes);
        _configWindow = new ConfigWindow(this);

        _windowSystem.AddWindow(_mainWindow);
        _windowSystem.AddWindow(_configWindow);

        pi.UiBuilder.Draw          += _windowSystem.Draw;
        pi.UiBuilder.OpenConfigUi  += OpenConfig;
        pi.UiBuilder.OpenMainUi    += OpenMain;

        Framework.Update += OnFrameworkUpdate;

        commands.AddHandler(CommandMain,   new CommandInfo(OnMainCommand)   { HelpMessage = "Open BeastTribeQuester window." });
        commands.AddHandler(CommandConfig, new CommandInfo(OnConfigCommand) { HelpMessage = "Open BeastTribeQuester config." });

        Log.Info("[BeastTribeQuester] Plugin loaded. Use /btq to open.");
    }

    private void OnFrameworkUpdate(IFramework fw)
    {
        var delta = (float)fw.UpdateDelta.TotalSeconds;
        _runner.Tick(delta);
    }

    private void OnMainCommand  (string _, string __) => OpenMain();
    private void OnConfigCommand(string _, string __) => OpenConfig();
    private void OpenMain()   => _mainWindow.IsOpen   = true;
    private void OpenConfig() => _configWindow.IsOpen = true;

    public void Dispose()
    {
        Commands.RemoveHandler(CommandMain);
        Commands.RemoveHandler(CommandConfig);
        Framework.Update -= OnFrameworkUpdate;
        PluginInterface.UiBuilder.Draw         -= _windowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= OpenConfig;
        PluginInterface.UiBuilder.OpenMainUi   -= OpenMain;
        _runner.Stop();
        Vnavmesh.Dispose();
        Lifestream.Dispose();
    }
}
