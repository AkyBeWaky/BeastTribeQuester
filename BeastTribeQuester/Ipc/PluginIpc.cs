using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using System.Numerics;

namespace BeastTribeQuester.Ipc;

// ─────────────────────────────────────────────────────────
//  vnavmesh
// ─────────────────────────────────────────────────────────

/// <summary>
/// Thin wrapper around the vnavmesh IPC surface.
/// All method names match the actual IPC identifiers exposed by vnavmesh.
/// </summary>
public sealed class VnavmeshIpc : IDisposable
{
    private readonly ICallGateSubscriber<bool>            _isReady;
    private readonly ICallGateSubscriber<bool>            _pathIsRunning;
    private readonly ICallGateSubscriber<float, float, float, bool, object?> _moveTo;
    private readonly ICallGateSubscriber<object?>         _pathStop;

    public VnavmeshIpc(IDalamudPluginInterface pi)
    {
        _isReady       = pi.GetIpcSubscriber<bool>                            ("vnavmesh.Nav.IsReady");
        _pathIsRunning = pi.GetIpcSubscriber<bool>                            ("vnavmesh.Path.IsRunning");
        _moveTo        = pi.GetIpcSubscriber<float, float, float, bool, object?>("vnavmesh.SimpleMove.PathfindAndMoveTo");
        _pathStop      = pi.GetIpcSubscriber<object?>                          ("vnavmesh.Path.Stop");
    }

    public bool IsReady       => InvokeOrDefault(_isReady,       false);
    public bool IsPathRunning => InvokeOrDefault(_pathIsRunning, false);

    /// <summary>
    /// Kick off pathfinding to <paramref name="position"/>.
    /// Returns immediately; poll <see cref="IsPathRunning"/> to know when arrived.
    /// </summary>
    public void MoveToPosition(Vector3 position, bool fly = false)
        => _moveTo.InvokeAction(position.X, position.Y, position.Z, fly);

    public void StopPath() => _pathStop.InvokeAction();

    private static T InvokeOrDefault<T>(ICallGateSubscriber<T> sub, T fallback)
    {
        try   { return sub.InvokeFunc(); }
        catch { return fallback;          }
    }

    public void Dispose() { }
}

// ─────────────────────────────────────────────────────────
//  TextAdvance
// ─────────────────────────────────────────────────────────

/// <summary>
/// Wrapper for TextAdvance IPC – enables / disables auto-dialogue skipping.
/// </summary>
public sealed class TextAdvanceIpc : IDisposable
{
    private readonly ICallGateSubscriber<string, object?> _enqueue;

    public TextAdvanceIpc(IDalamudPluginInterface pi)
    {
        // TextAdvance exposes an IPC that accepts a chat command string.
        // We use /ta enable / disable to toggle its automated dialogue handling.
        _enqueue = pi.GetIpcSubscriber<string, object?>("TextAdvance.Enqueue");
    }

    /// <summary>Tell TextAdvance to process a command (e.g. "enable" / "disable").</summary>
    public void Enqueue(string command)
    {
        try { _enqueue.InvokeAction(command); }
        catch { /* TextAdvance not installed – silently ignore */ }
    }

    public void Dispose() { }
}

// ─────────────────────────────────────────────────────────
//  Lifestream
// ─────────────────────────────────────────────────────────

/// <summary>
/// Wrapper for Lifestream IPC – handles Aetheryte teleportation.
/// </summary>
public sealed class LifestreamIpc : IDisposable
{
    private readonly ICallGateSubscriber<uint, byte, bool> _teleport;
    private readonly ICallGateSubscriber<bool>             _isBusy;

    public LifestreamIpc(IDalamudPluginInterface pi)
    {
        _teleport = pi.GetIpcSubscriber<uint, byte, bool>("Lifestream.Teleport");
        _isBusy   = pi.GetIpcSubscriber<bool>            ("Lifestream.IsBusy");
    }

    /// <returns>True if the teleport request was accepted.</returns>
    public bool Teleport(uint aetheryteId, byte subIndex = 0)
    {
        try   { return _teleport.InvokeFunc(aetheryteId, subIndex); }
        catch { return false; }
    }

    public bool IsBusy
    {
        get
        {
            try   { return _isBusy.InvokeFunc(); }
            catch { return false;                 }
        }
    }

    public void Dispose() { }
}
