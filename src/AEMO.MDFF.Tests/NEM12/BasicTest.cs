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

    [Fact]
    public async Task ParsesIntervalMetadataAndRelatedRecords()
    {
        var nem12Reader = new Nem12Reader();
        var values = string.Join(",", Enumerable.Repeat("0", 48));
        await using var fs = CreateStream($"""
                                          100,NEM12,200506081149,UNITEDDP,NEMMCO
                                          200,NEM1201009,E1E2,1,E1,N1,01009,kWh,30,20050610
                                          300,20050301,{values},A,79,Power outage,20050310121004,20050310182204
                                          400,1,48,A,79,Power outage
                                          500,O,S01009,20050310121004,001123.5
                                          900
                                          """);

        var records = new List<IMdffRecord>();
        await foreach (var record in nem12Reader.ReadAsync(fs, CancellationToken.None))
        {
            records.Add(record);
        }

        var intervalData = Assert.IsType<IntervalDataRecord>(records[2]);
        Assert.Equal("A", intervalData.QualityMethod);
        Assert.Equal("79", intervalData.ReasonCode);
        Assert.Equal("Power outage", intervalData.ReasonDescription);
        Assert.Equal(new DateTime(2005, 03, 10, 12, 10, 04), intervalData.UpdateDateTime);
        Assert.Equal(new DateTime(2005, 03, 10, 18, 22, 04), intervalData.MSATSLoadDateTime);

        var intervalEvent = Assert.IsType<IntervalEventRecord>(records[3]);
        Assert.Equal(1, intervalEvent.StartInterval);
        Assert.Equal(48, intervalEvent.EndInterval);
        Assert.Equal("A", intervalEvent.QualityMethod);
        Assert.Equal("79", intervalEvent.ReasonCode);

        var b2bDetails = Assert.IsType<B2BDetailsRecord>(records[4]);
        Assert.Equal("O", b2bDetails.TransCode);
        Assert.Equal("S01009", b2bDetails.RetServiceOrder);
        Assert.Equal(new DateTime(2005, 03, 10, 12, 10, 04), b2bDetails.ReadDateTime);
        Assert.Equal("001123.5", b2bDetails.IndexRead);
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
