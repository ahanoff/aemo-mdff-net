using System.Text;
using AEMO.MDFF.Abstractions;
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
                    _testOutputHelper.WriteLine(nsrd?.ToLongDateString());
                    break;
                case IntervalDataRecord intervalDataRecord:
                    _testOutputHelper.WriteLine(intervalDataRecord.IntervalValues.ToString());
                    _testOutputHelper.WriteLine(intervalDataRecord.UpdateDateTime?.ToLongTimeString());
                    break;
            }
        }
    }

    [Fact]
    public async Task ThrowsWhenIntervalDataRecordAppearsBeforeNmiDataDetailsRecord()
    {
        var nem12Reader = new Nem12Reader();
        await using var fs = CreateStream("""
                                          100,NEM12,200506081149,UNITEDDP,NEMMCO
                                          300,20050301
                                          900
                                          """);

        var ex = await Assert.ThrowsAsync<InvalidDataException>(() => DrainAsync(nem12Reader.ReadAsync(fs, CancellationToken.None)));
        Assert.Equal("Interval data record found before NMI data details record", ex.Message);
    }

    [Fact]
    public async Task ParsesOptionalNmiFieldsAsNull()
    {
        var nem12Reader = new Nem12Reader();
        await using var fs = CreateStream($"""
                                          100,NEM12,200506081149,UNITEDDP,NEMMCO
                                          200,NEM1201009,E1E2,,E1,,01009,kWh,30,
                                          900
                                          """);

        var records = new List<IMdffRecord>();
        await foreach (var record in nem12Reader.ReadAsync(fs, CancellationToken.None))
        {
            records.Add(record);
        }

        var nmiDataDetails = Assert.IsType<NMIDataDetailsRecord>(records[1]);
        Assert.Null(nmiDataDetails.RegisterId);
        Assert.Null(nmiDataDetails.MDMDataStreamIdentifier);
        Assert.Null(nmiDataDetails.NextScheduledReadDate);
    }

    private static MemoryStream CreateStream(string content)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(content));
    }

    private static async Task DrainAsync(IAsyncEnumerable<IMdffRecord> records)
    {
        await foreach (var _ in records)
        {
        }
    }
}
