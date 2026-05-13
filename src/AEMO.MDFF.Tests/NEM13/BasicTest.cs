using System.Text;
using AEMO.MDFF.Abstractions;
using AEMO.MDFF.NEM13;

namespace AEMO.MDFF.Tests.NEM13;

public class BasicTest
{
    [Fact]
    public async Task ParsesNem13Records()
    {
        var nem13Reader = new Nem13Reader();
        await using var fs = CreateStream("""
                                          100,NEM13,200506081149,UNITEDDP,NEMMCO
                                          250,1234567890,1141,01,11,11,METSER66,E,000021.2,20031001103230,A,,,000534.5,20040201100030,E64,77,,343.5,kWh,20040509,20040202125010,20040203000130
                                          550,N,,A,S01009
                                          900
                                          """);

        var records = new List<IMdffRecord>();
        await foreach (var record in nem13Reader.ReadAsync(fs, CancellationToken.None))
        {
            records.Add(record);
        }

        var header = Assert.IsType<HeaderRecord>(records[0]);
        Assert.Equal("NEM13", header.VersionHeader);

        var accumulationData = Assert.IsType<AccumulationMeterDataRecord>(records[1]);
        Assert.Equal("1234567890", accumulationData.NMI);
        Assert.Equal("000021.2", accumulationData.PreviousRegisterRead);
        Assert.Equal(new DateTime(2003, 10, 01, 10, 32, 30), accumulationData.PreviousRegisterReadDateTime);
        Assert.Equal("000534.5", accumulationData.CurrentRegisterRead);
        Assert.Equal(new DateTime(2004, 02, 01, 10, 00, 30), accumulationData.CurrentRegisterReadDateTime);
        Assert.Equal(343.5m, accumulationData.Quantity);
        Assert.Equal(new DateOnly(2004, 05, 09), accumulationData.NextScheduledReadDate);
        Assert.Equal(new DateTime(2004, 02, 02, 12, 50, 10), accumulationData.UpdateDateTime);
        Assert.Equal(new DateTime(2004, 02, 03, 00, 01, 30), accumulationData.MSATSLoadDateTime);

        var b2bDetails = Assert.IsType<B2BDetailsRecord>(records[2]);
        Assert.Equal("N", b2bDetails.PreviousTransCode);
        Assert.Null(b2bDetails.PreviousRetServiceOrder);
        Assert.Equal("A", b2bDetails.CurrentTransCode);
        Assert.Equal("S01009", b2bDetails.CurrentRetServiceOrder);

        Assert.IsType<EndRecord>(records[3]);
    }

    [Fact]
    public async Task ThrowsWhenDataRecordAppearsBeforeHeader()
    {
        var nem13Reader = new Nem13Reader();
        await using var fs = CreateStream("""
                                          250,1234567890,1141,01,11,11,METSER66,E,000021.2,20031001103230,A,,,000534.5,20040201100030,E64,77,,343.5,kWh,20040509,20040202125010,20040203000130
                                          900
                                          """);

        var ex = await Assert.ThrowsAsync<InvalidDataException>(() => DrainAsync(nem13Reader.ReadAsync(fs, CancellationToken.None)));
        Assert.Equal("Data record found before header", ex.Message);
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
