using AEMO.MDFF.NEM12;
using Xunit.Abstractions;

namespace AEMO.MDFF.Tests.NEM12;

public class BasicTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public BasicTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task Test1()
    {
        var nem12Reader = new Nem12Reader();
        await using var fs = new FileStream("nem12-sample.csv", FileMode.Open, FileAccess.Read);
        await foreach (var r in nem12Reader.ReadAsync(fs, CancellationToken.None))
        {
            switch (r)
            {
                case HeaderRecord { RecordIndicator: var vh }:
                    Assert.Equal(100.ToString(), vh);
                    break;
                case NMIDataDetailsRecord { NextScheduledReadDate: var nsrd }:
                    _testOutputHelper.WriteLine(nsrd.ToLongDateString());
                    break;
            }
        }
    }
}
