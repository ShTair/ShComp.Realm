using Realms;

namespace ShComp.Realms;

/// <summary>
/// Realmの処理を同じスレッドで行うための方法を提供します。
/// </summary>
public sealed class RealmContext : IDisposable
{
    private readonly RealmSynchronizationContext _syncContext;
    private Realm? _realm;

    public RealmContext(RealmConfiguration configuration)
    {
        _syncContext = new RealmSynchronizationContext();
        _syncContext.Post(_ => _realm = Realm.GetInstance(configuration), null);
    }

    public void Dispose()
    {
        _syncContext.Dispose();
        _realm?.Dispose();
    }

    /// <summary>
    /// 指定したRealmの非同期処理を、Realmを生成したスレッドと同じスレッドで実行します。<br />
    /// 同期コンテキストにより、処理でawaitを使用できます。
    /// </summary>
    /// <param name="func">Realmの非同期処理</param>
    public Task InvokeAsync(Func<Realm, Task> func)
    {
        var tcs = new TaskCompletionSource();

        _syncContext.Post(async _ =>
        {
            try { await func(_realm!); tcs.TrySetResult(); }
            catch (Exception e) { tcs.TrySetException(e); }
        }, null);

        return tcs.Task;
    }

    /// <summary>
    /// 指定したRealmの非同期処理を、Realmを生成したスレッドと同じスレッドで実行します。<br />
    /// 同期コンテキストにより、処理でawaitを使用できます。
    /// </summary>
    /// <param name="func">Realmの非同期処理</param>
    public Task<T> InvokeAsync<T>(Func<Realm, Task<T>> func)
    {
        var tcs = new TaskCompletionSource<T>();

        _syncContext.Post(async _ =>
        {
            try { tcs.TrySetResult(await func(_realm!)); }
            catch (Exception e) { tcs.TrySetException(e); }
        }, null);

        return tcs.Task;
    }

    /// <summary>
    /// 指定したRealmの処理を、Realmを生成したスレッドと同じスレッドで実行します。
    /// </summary>
    /// <param name="func">Realmの処理</param>
    public Task InvokeAsync(Action<Realm> func)
    {
        var tcs = new TaskCompletionSource();

        _syncContext.Post(_ =>
        {
            try { func(_realm!); tcs.TrySetResult(); }
            catch (Exception e) { tcs.TrySetException(e); }
        }, null);

        return tcs.Task;
    }

    /// <summary>
    /// 指定したRealmの処理を、Realmを生成したスレッドと同じスレッドで実行します。
    /// </summary>
    /// <param name="func">Realmの処理</param>
    public Task<T> InvokeAsync<T>(Func<Realm, T> func)
    {
        var tcs = new TaskCompletionSource<T>();

        _syncContext.Post(_ =>
        {
            try { tcs.TrySetResult(func(_realm!)); }
            catch (Exception e) { tcs.TrySetException(e); }
        }, null);

        return tcs.Task;
    }
}
