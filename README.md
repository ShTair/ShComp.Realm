# ShComp.Realm

## はじめに

Realmデータベースへの処理は、Realmインスタンスを生成したスレッドと同じスレッドで実行する必要がある。
これは、Realmへの処理と処理の間に、安易に非同期処理を挟んでawaitできないことを意味する。
（Realmインスタンスを生成したスレッドがスレッドプール等のものだった場合、awaitすると後続の処理が別のスレッドで実行される可能性があるから。）

```csharp
using (var realm = Realm.GetInstance())
{
    realm.All<xxxx>().xxxxxxxxxx;
    await Task.Delay(1000);
    realm.All<xxxx>().xxxxxxxxxx; // <-ダメ
}
```

これだと使いにくい。同期コンテキストを駆使して、処理は常に同じスレッドで実行する＆awaitしても同じスレッドに戻ってくるようにした。  
RealmContextを生成すると、Taskプールのスレッドを1個捕まえて、あとのRealmへの処理を全てそのスレッドで行う。

```csharp
using (var context = new RealmContext(_realmConfiguration))
{
    await context.InvokeAsync(realm =>
    {
        realm.All<xxxx>().xxxxxxxxxx;
        await Task.Delay(1000);
        realm.All<xxxx>().xxxxxxxxxx; // <-OK
    });
}
```

## 疑問点

- Realmのインスタンスを、常に持っておくのは良いのか？
    - 同じスレッドを保持し続けることを考えるより、使うたびに生成→破棄した方が良いのでは。
- 同期コンテキストの使い方って合ってる…？
