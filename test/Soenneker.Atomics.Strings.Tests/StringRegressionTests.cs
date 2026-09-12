using System;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Atomics.Strings.Tests;

public sealed class StringRegressionTests
{
    [Test]
    public async Task Clear_races_never_make_publication_return_null()
    {
        var value = new AtomicString();
        int stop = 0, nulls = 0;
        Task clear = Task.Run(() =>
        {
            while (Volatile.Read(ref stop) == 0)
                value.Clear();
        });
        try
        {
            for (int i = 0; i < 100000; i++)
            {
                if (value.GetOrAdd(static () => "published") is null)
                    nulls++;
            }
        }
        finally
        {
            Volatile.Write(ref stop, 1);
            await clear.WaitAsync(TimeSpan.FromSeconds(5));
        }
        await Assert.That(nulls).IsEqualTo(0);
    }

    [Test]
    public async Task State_factory_is_used_only_when_absent()
    {
        var value = new AtomicString();
        await Assert.That(value.GetOrAdd("first", static state => state)).IsEqualTo("first");
        await Assert.That(value.GetOrAdd("second", static state => state)).IsEqualTo("first");
        value.Clear();
        await Assert.That(value.GetOrAdd("second", static state => state)).IsEqualTo("second");
        await Assert.That(() => value.GetOrAdd(0, (Func<int, string>)null!)).Throws<ArgumentNullException>();
        value.Clear();
        await Assert.That(() => value.GetOrAdd(0, static _ => (string)null!)).Throws<InvalidOperationException>();
    }
}
