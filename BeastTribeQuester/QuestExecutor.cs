using BeastTribeQuester.Ipc;
using BeastTribeQuester.Model;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
using System.Numerics;

namespace BeastTribeQuester;

/// <summary>
/// Drives the per-step automation of a single <see cref="BeastTribeQuest"/>.
/// Instantiate one per quest; call <see cref="Tick"/> every framework update
/// until <see cref="IsFinished"/> is true.
/// </summary>
public sealed class QuestExecutor
{
    // ── Dependencies ─────────────────────────────────────────────────────────
    private readonly VnavmeshIpc       _nav;
    private readonly LifestreamIpc     _lifestream;
    private readonly IClientState      _clientState;
    private readonly ICondition        _condition;
    private readonly IPluginLog        _log;
    private readonly Configuration     _config;

    // ── State ─────────────────────────────────────────────────────────────────
    private readonly BeastTribeQuest _quest;
    private int   _stepIndex;
    private bool  _stepStarted;
    private float _stepTimer;   // seconds since step was started

    private const float StepTimeoutSeconds = 60f;
    private const float ArriveDistanceSq   = 2.5f * 2.5f;   // 2.5y radius counts as "arrived"

    public bool IsFinished { get; private set; }
    public bool HasError   { get; private set; }
    public string? ErrorMessage { get; private set; }

    // ── Constructor ──────────────────────────────────────────────────────────
    public QuestExecutor(
        BeastTribeQuest quest,
        VnavmeshIpc     nav,
        LifestreamIpc   lifestream,
        IClientState    clientState,
        ICondition      condition,
        IPluginLog      log,
        Configuration   config)
    {
        _quest       = quest;
        _nav         = nav;
        _lifestream  = lifestream;
        _clientState = clientState;
        _condition   = condition;
        _log         = log;
        _config      = config;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Call once per framework tick.</summary>
    public void Tick(float deltaSeconds)
    {
        if (IsFinished || HasError)
            return;

        // Safety – abort in combat if configured
        if (_config.StopOnCombat && _condition[ConditionFlag.InCombat])
        {
            _log.Warning("[BeastTribeQuester] Stopped due to combat.");
            _nav.StopPath();
            SetError("Stopped: entered combat.");
            return;
        }

        if (_stepIndex >= _quest.Steps.Count)
        {
            _log.Info($"[BeastTribeQuester] Quest '{_quest.Name}' completed.");
            IsFinished = true;
            return;
        }

        var step = _quest.Steps[_stepIndex];
        _stepTimer += deltaSeconds;

        if (_stepTimer > StepTimeoutSeconds)
        {
            SetError($"Step {_stepIndex} ({step.InteractionType}) timed out after {StepTimeoutSeconds}s.");
            return;
        }

        if (!_stepStarted)
        {
            StartStep(step);
            _stepStarted = true;
        }

        if (IsStepComplete(step))
        {
            _log.Debug($"[BeastTribeQuester] Step {_stepIndex} ({step.InteractionType}) done.");
            _stepIndex++;
            _stepStarted = false;
            _stepTimer   = 0f;
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void StartStep(QuestStep step)
    {
        _log.Debug($"[BeastTribeQuester] Starting step {_stepIndex}: {step.InteractionType} – {step.Comment}");

        switch (step.InteractionType)
        {
            case EInteractionType.MoveTo:
                if (step.Position.HasValue)
                    _nav.MoveToPosition(step.Position.Value);
                break;

            case EInteractionType.Teleport:
                if (step.AetheryteId.HasValue)
                    _lifestream.Teleport(step.AetheryteId.Value);
                break;

            // TalkTo, AcceptQuest, TurnInQuest, UseItem, Gather:
            // TextAdvance will auto-advance all dialogue windows, so we
            // just navigate close to the NPC and wait.  The position is
            // optional – if it's missing we assume we're already there.
            case EInteractionType.TalkTo:
            case EInteractionType.AcceptQuest:
            case EInteractionType.TurnInQuest:
            case EInteractionType.Gather:
                if (step.Position.HasValue)
                    _nav.MoveToPosition(step.Position.Value);
                break;

            case EInteractionType.SwitchJob:
                // Job switching is done via game macro / chat command.
                // TextAdvance can queue /gearset change N for us.
                // Here we emit a chat command and let the game handle it.
                if (step.ClassJobId.HasValue)
                    _log.Info($"[BeastTribeQuester] NOTE: SwitchJob to ClassJobId {step.ClassJobId} – handle via /gearset manually or extend this handler.");
                break;

            case EInteractionType.WaitForCondition:
                // Nothing to start – polling happens in IsStepComplete.
                break;
        }
    }

    private bool IsStepComplete(QuestStep step)
    {
        switch (step.InteractionType)
        {
            case EInteractionType.MoveTo:
                return HasArrivedAt(step.Position);

            case EInteractionType.Teleport:
                // Wait until Lifestream is no longer busy AND we're in the right territory
                if (_lifestream.IsBusy)
                    return false;
                if (step.TerritoryId.HasValue)
                    return _clientState.TerritoryType == step.TerritoryId.Value;
                return _stepTimer >= (_config.PostTeleportDelayMs / 1000f);

            case EInteractionType.TalkTo:
            case EInteractionType.AcceptQuest:
                // Navigate close, then trust TextAdvance to click through.
                // Step is "done" once we've arrived and a short settle delay has passed.
                if (!HasArrivedAt(step.Position))
                    return false;
                return _stepTimer >= 1.5f; // give TextAdvance time to interact

            case EInteractionType.TurnInQuest:
                if (!HasArrivedAt(step.Position))
                    return false;
                return _stepTimer >= 2.0f;

            case EInteractionType.Gather:
                if (!HasArrivedAt(step.Position))
                    return false;
                // Wait for gathering animation to end
                return !_condition[ConditionFlag.Gathering] && _stepTimer >= 3f;

            case EInteractionType.WaitForCondition:
                // Generic delay; falls back to DelayAfterMs or 2 seconds
                var waitMs = step.DelayAfterMs ?? 2000;
                return _stepTimer >= waitMs / 1000f;

            case EInteractionType.SwitchJob:
                return _stepTimer >= 1.0f;

            case EInteractionType.UseItem:
                return _stepTimer >= 0.5f;

            default:
                return true;
        }
    }

    private bool HasArrivedAt(Vector3? target)
    {
        if (target == null)
            return true;   // no position constraint → already there
        if (_nav.IsPathRunning)
            return false;
        var localPlayer = _clientState.LocalPlayer;
        if (localPlayer == null)
            return false;
        var distSq = Vector3.DistanceSquared(localPlayer.Position, target.Value);
        return distSq <= ArriveDistanceSq;
    }

    private void SetError(string msg)
    {
        _nav.StopPath();
        ErrorMessage = msg;
        HasError     = true;
        _log.Error($"[BeastTribeQuester] {msg}");
    }
}

// ─────────────────────────────────────────────────────────
//  TribeRunner – runs all daily quests for one tribe
// ─────────────────────────────────────────────────────────

/// <summary>
/// Sequentially executes each <see cref="BeastTribeQuest"/> in a
/// <see cref="BeastTribeDefinition"/>.
/// </summary>
public sealed class TribeRunner
{
    private readonly BeastTribeDefinition _tribe;
    private readonly VnavmeshIpc          _nav;
    private readonly LifestreamIpc        _lifestream;
    private readonly IClientState         _clientState;
    private readonly ICondition           _condition;
    private readonly IPluginLog           _log;
    private readonly Configuration        _config;

    private int           _questIndex;
    private QuestExecutor? _currentExecutor;

    public bool IsFinished { get; private set; }
    public bool HasError   { get; private set; }
    public string? ErrorMessage { get; private set; }

    public TribeRunner(
        BeastTribeDefinition tribe,
        VnavmeshIpc          nav,
        LifestreamIpc        lifestream,
        IClientState         clientState,
        ICondition           condition,
        IPluginLog           log,
        Configuration        config)
    {
        _tribe       = tribe;
        _nav         = nav;
        _lifestream  = lifestream;
        _clientState = clientState;
        _condition   = condition;
        _log         = log;
        _config      = config;
    }

    public void Tick(float deltaSeconds)
    {
        if (IsFinished || HasError)
            return;

        // Teleport to hub first if needed
        if (_questIndex == 0 && _currentExecutor == null
            && _clientState.TerritoryType != _tribe.HubTerritoryId)
        {
            _log.Info($"[BeastTribeQuester] Teleporting to {_tribe.DisplayName} hub…");
            _lifestream.Teleport(_tribe.StartAetheryteId);
            // Give Lifestream time to process
            return;
        }

        if (_lifestream.IsBusy)
            return;

        if (_questIndex >= _tribe.DailyQuests.Count)
        {
            _log.Info($"[BeastTribeQuester] All quests for {_tribe.DisplayName} done.");
            IsFinished = true;
            return;
        }

        // Create executor for current quest if needed
        _currentExecutor ??= new QuestExecutor(
            _tribe.DailyQuests[_questIndex],
            _nav, _lifestream, _clientState, _condition, _log, _config);

        _currentExecutor.Tick(deltaSeconds);

        if (_currentExecutor.HasError)
        {
            ErrorMessage = _currentExecutor.ErrorMessage;
            HasError     = true;
            return;
        }

        if (_currentExecutor.IsFinished)
        {
            _questIndex++;
            _currentExecutor = null;
        }
    }
}

// ─────────────────────────────────────────────────────────
//  MasterRunner – iterates over all enabled tribes
// ─────────────────────────────────────────────────────────

public sealed class MasterRunner
{
    private readonly List<BeastTribeDefinition> _tribes;
    private readonly VnavmeshIpc                _nav;
    private readonly LifestreamIpc              _lifestream;
    private readonly IClientState               _clientState;
    private readonly ICondition                 _condition;
    private readonly IPluginLog                 _log;
    private readonly Configuration              _config;

    private int         _tribeIndex;
    private TribeRunner? _current;

    public bool   IsRunning { get; private set; }
    public bool   IsFinished { get; private set; }
    public string Status    { get; private set; } = "Idle";

    public MasterRunner(
        List<BeastTribeDefinition> tribes,
        VnavmeshIpc                nav,
        LifestreamIpc              lifestream,
        IClientState               clientState,
        ICondition                 condition,
        IPluginLog                 log,
        Configuration              config)
    {
        _tribes      = tribes;
        _nav         = nav;
        _lifestream  = lifestream;
        _clientState = clientState;
        _condition   = condition;
        _log         = log;
        _config      = config;
    }

    public void Start()
    {
        _tribeIndex = 0;
        _current    = null;
        IsRunning   = true;
        IsFinished  = false;
        Status      = "Starting…";
        _log.Info("[BeastTribeQuester] Master run started.");
    }

    public void Stop()
    {
        _nav.StopPath();
        IsRunning  = false;
        IsFinished = false;
        Status     = "Stopped by user.";
        _log.Info("[BeastTribeQuester] Stopped by user.");
    }

    public void Tick(float deltaSeconds)
    {
        if (!IsRunning || IsFinished)
            return;

        if (_tribeIndex >= _tribes.Count)
        {
            IsRunning  = false;
            IsFinished = true;
            Status     = "All tribes completed!";
            _log.Info("[BeastTribeQuester] All tribes finished.");
            return;
        }

        var tribe = _tribes[_tribeIndex];

        // Respect the enabled-tribes filter
        if (_config.EnabledTribes.Count > 0
            && !_config.EnabledTribes.Contains(tribe.TribeKey))
        {
            _tribeIndex++;
            _current = null;
            return;
        }

        _current ??= new TribeRunner(
            tribe, _nav, _lifestream, _clientState, _condition, _log, _config);

        Status = $"Running: {tribe.DisplayName}";
        _current.Tick(deltaSeconds);

        if (_current.HasError)
        {
            Status    = $"Error on {tribe.DisplayName}: {_current.ErrorMessage}";
            IsRunning = false;
            return;
        }

        if (_current.IsFinished)
        {
            _tribeIndex++;
            _current = null;
        }
    }
}
