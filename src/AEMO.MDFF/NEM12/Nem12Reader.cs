using System.Globalization;
using System.Runtime.CompilerServices;
using AEMO.MDFF.Abstractions;
using Sylvan.Data.Csv;

namespace AEMO.MDFF.NEM12;

public class Nem12Reader() : IMdffReader
{
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
        int? currentIntervalLength = null;
        
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
                    currentIntervalLength = ddr.IntervalLength;
                    yield return ddr;
                    break;
                case "300":
                    if (!headerFound)
                        throw new InvalidDataException("Data record found before header");
                    if (currentIntervalLength is null)
                        throw new InvalidDataException("Interval data record found before NMI data details record");
                    var idr = ParseIntervalDataRecord(csv, currentIntervalLength.Value);
                    yield return idr;
                    break;
                case "400":
                    if (!headerFound)
                        throw new InvalidDataException("Data record found before header");
                    var ier = ParseIntervalEventRecord(csv);
                    yield return ier;
                    break;
                case "500":
                    if (!headerFound)
                        throw new InvalidDataException("Data record found before header");
                    var b2b = ParseB2BDetailsRecord(csv);
                    yield return b2b;
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
        return new NMIDataDetailsRecord
        {
            NMI = csv.GetString(1),
            NMIConfiguration = csv.GetString(2),
            RegisterId = GetOptionalString(csv, 3),
            NMISuffix = csv.GetString(4),
            MDMDataStreamIdentifier = GetOptionalString(csv, 5),
            MeterSerialNumber = GetOptionalString(csv, 6),
            UOM = csv.GetString(7),
            IntervalLength = csv.GetInt32(8),
            NextScheduledReadDate = ParseOptionalDate(csv, 9)
        };
    }
    
    private IntervalDataRecord ParseIntervalDataRecord(CsvDataReader csv, int intervalLength)
    {
        var intervalDate = DateOnly.ParseExact(csv.GetString(1), "yyyyMMdd", CultureInfo.InvariantCulture);
        int expectedIntervals = 1440 / intervalLength; // 1440 minutes in a day
        int qualityMethodIndex = 2 + expectedIntervals;
        int reasonCodeIndex = qualityMethodIndex + 1;
        int reasonDescriptionIndex = qualityMethodIndex + 2;
        int updateDateTimeIndex = qualityMethodIndex + 3;
        int msatsLoadDateTimeIndex = qualityMethodIndex + 4;

        var intervalValues = new decimal[expectedIntervals];
        for (int i = 2; i < expectedIntervals + 2; i++)
        {
            intervalValues[i - 2] = csv.GetDecimal(i);
        }

        return new IntervalDataRecord
        {
            IntervalDate = intervalDate,
            IntervalValues = intervalValues,
            QualityMethod = csv.GetString(qualityMethodIndex),
            ReasonCode = GetOptionalString(csv, reasonCodeIndex),
            ReasonDescription = GetOptionalString(csv, reasonDescriptionIndex),
            UpdateDateTime = ParseOptionalDateTime(csv, updateDateTimeIndex),
            MSATSLoadDateTime = ParseOptionalDateTime(csv, msatsLoadDateTimeIndex),
        };
    }

    private IntervalEventRecord ParseIntervalEventRecord(CsvDataReader csv)
    {
        return new IntervalEventRecord
        {
            StartInterval = csv.GetInt32(1),
            EndInterval = csv.GetInt32(2),
            QualityMethod = csv.GetString(3),
            ReasonCode = GetOptionalString(csv, 4),
            ReasonDescription = GetOptionalString(csv, 5),
        };
    }

    private B2BDetailsRecord ParseB2BDetailsRecord(CsvDataReader csv)
    {
        return new B2BDetailsRecord
        {
            TransCode = csv.GetString(1),
            RetServiceOrder = GetOptionalString(csv, 2),
            ReadDateTime = ParseOptionalDateTime(csv, 3),
            IndexRead = GetOptionalString(csv, 4),
        };
    }
    private static DateOnly? ParseOptionalDate(CsvDataReader csv, int index)
    {
        var value = GetOptionalString(csv, index);
        return value is null
            ? null
            : DateOnly.ParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture);
    }

    private static DateTime? ParseOptionalDateTime(CsvDataReader csv, int index)
    {
        var value = GetOptionalString(csv, index);
        return value is null
            ? null
            : DateTime.ParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
    }

    private static string? GetOptionalString(CsvDataReader csv, int index)
    {
        var value = csv.GetString(index);
        return string.IsNullOrEmpty(value) ? null : value;
    }
}
