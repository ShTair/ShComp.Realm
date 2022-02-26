using Realms;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ShComp.Realms.Test;

public class RealmTest
{
    private readonly RealmConfiguration _realmConfiguration;

    public RealmTest()
    {
        var path = Path.Combine(Environment.CurrentDirectory, "db.realm");
        Console.WriteLine(path);
        _realmConfiguration = new RealmConfiguration(path);
    }

    [Fact]
    public async Task Test()
    {
        using var context = new RealmContext(_realmConfiguration);
        await context.InvokeAsync(realm =>
        {
            realm.Write(() =>
            {
                realm.Add(new Data { Id = (int)DateTime.Now.Ticks, Creation = DateTimeOffset.Now });
            });
        });

        var datas = await context.InvokeAsync(realm =>
        {
            return realm.All<Data>().ToArray();
        });

        await context.InvokeAsync(realm =>
        {
            Assert.NotEqual(0, datas[0].Id);
        });
    }

    public class Data : RealmObject
    {
        [PrimaryKey]
        public int Id { get; set; }

        public DateTimeOffset Creation { get; set; }
    }
}
