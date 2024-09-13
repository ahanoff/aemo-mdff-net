using System.Globalization;
using System.Runtime.CompilerServices;
using AEMO.MDFF.Abstractions;
using Sylvan.Data.Csv;

namespace AEMO.MDFF.NEM12;

public class Nem12Reader() : IMdffReader
{
    private readonly Dictionary<string, int> _nmiIntervalLengths = new();

    public async IAsyncEnumerable<IMdffRecord> ReadAsync(Stream stream, [EnumeratorCancellation] CancellationToken ct)
    {
        using var sr = new StreamReader(stream);
        await using var csv = await CsvDataReader.CreateAsync(sr,
            new CsvDataReaderOptions()
            {
                HasHeaders = false,
            }, ct);
        
        bool headerFound = false;
        bool endFound = false;
        string currentNMI = null;
        
        while (await csv.ReadAsync(ct) && !endFound)
        {
            var recordIndicator = csv.GetString(0);
            switch (recordIndicator)
            {
                case "100":
                    if (headerFound)
                        throw new InvalidDataException("Multiple header records found");
                    var hr = ParseHeaderRecord(csv);
                    headerFound = true;
                    yield return hr;
                    break;
                case "200":
                    if (!headerFound)
                        throw new InvalidDataException("Data record found before header");
                    var ddr = ParseNMIDataDetailsRecord(csv);
                    currentNMI = ddr.NMI;
                    _nmiIntervalLengths[currentNMI] = ddr.IntervalLength;
                    yield return ddr;
                    break;
                case "300":
                    if (!headerFound)
                        throw new InvalidDataException("Data record found before header");
                    var idr = ParseIntervalDataRecord(csv, currentNMI);
                    yield return idr;
                    break;
                case "400":
                    if (!headerFound)
                        throw new InvalidDataException("Data record found before header");
                    break;
                case "500":
                    if (!headerFound)
                        throw new InvalidDataException("Data record found before header");
                    break;
                case "900":
                    var er = new EndRecord();
                    endFound = true;
                    yield return er;
                    break;
                default:
                    throw new InvalidDataException($"Unsupported record indicator: {recordIndicator}");
            }
        }
        if (!headerFound)
            throw new InvalidDataException("No header record found");
        if (!endFound)
            throw new InvalidDataException("No end record found");
    }
    
    private HeaderRecord ParseHeaderRecord(CsvDataReader csv)
    {
        var dateTimeString = csv.GetString(2);
        var dateTime = DateTime.ParseExact(dateTimeString, "yyyyMMddHHmm", CultureInfo.InvariantCulture);

        return new HeaderRecord()
        {
            VersionHeader = csv.GetString(1),
            DateTime = dateTime,
            FromParticipant = csv.GetString(3),
            ToParticipant = csv.GetString(4)
        };
    }
    private NMIDataDetailsRecord ParseNMIDataDetailsRecord(CsvDataReader csv)
    {
        var dateString = csv.GetString(9);
        var date = DateOnly.ParseExact(dateString, "yyyyMMdd", CultureInfo.InvariantCulture);

        return new NMIDataDetailsRecord
        {
            NMI = csv.GetString(1),
            NMIConfiguration = csv.GetString(2),
            RegisterId = csv.GetString(3),
            NMISuffix = csv.GetString(4),
            MDMDataStreamIdentifier = csv.GetString(5),
            MeterSerialNumber = csv.GetString(6),
            UOM = csv.GetString(7),
            IntervalLength = csv.GetInt32(8),
            NextScheduledReadDate = date
        };
    }
    
    private IntervalDataRecord ParseIntervalDataRecord(CsvDataReader csv, string currentNMI)
    {
        var dateString = csv.GetString(1);
        var date = DateTime.ParseExact(dateString, "yyyyMMdd", CultureInfo.InvariantCulture);
        int intervalLength = _nmiIntervalLengths[currentNMI];
        int expectedIntervals = 1440 / intervalLength; // 1440 minutes in a day

        return new IntervalDataRecord
        {
            IntervalDate = date
            // TODO: parse interval values
        };
    }
    
}