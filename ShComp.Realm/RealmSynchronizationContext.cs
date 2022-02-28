using System.Collections.Concurrent;

namespace ShComp.Realms;

public sealed class RealmSynchronizationContext : SynchronizationContext, IDisposable
{
    private readonly CancellationTokenSource _cts;
    private readonly BlockingCollection<(SendOrPostCallback, object?)> _q;

    public RealmSynchronizationContext()
    {
        _cts = new CancellationTokenSource();
        _q = new BlockingCollection<(SendOrPostCallback, object?)>();
        Task.Run(() => Loop(_cts.Token));
    }

    private void Loop(CancellationToken cancellationToken)
    {
        SetSynchronizationContext(this);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var (d, state) = _q.Take(cancellationToken);
                try { d(state); } catch { }
            }
        }
        catch { }

        SetSynchronizationContext(null);
    }

    public override void Send(SendOrPostCallback d, object? state)
    {
        _q.Add((d, state));
    }

    public override void Post(SendOrPostCallback d, object? state)
    {
        _q.Add((d, state));
    }

    public void Dispose()
    {
        _cts.Cancel();
        _q.Dispose();
    }
}
