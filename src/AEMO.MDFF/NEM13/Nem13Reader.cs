using System.Globalization;
using System.Runtime.CompilerServices;
using AEMO.MDFF.Abstractions;
using Sylvan.Data.Csv;

namespace AEMO.MDFF.NEM13;

public class Nem13Reader : IMdffReader
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
                case "250":
                    if (!headerFound)
                        throw new InvalidDataException("Data record found before header");
                    var amdr = ParseAccumulationMeterDataRecord(csv);
                    yield return amdr;
                    break;
                case "550":
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
        var versionHeader = csv.GetString(1);
        if (!string.Equals(versionHeader, "NEM13", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Unsupported NEM13 version header: {versionHeader}");

        return new HeaderRecord()
        {
            VersionHeader = versionHeader,
            DateTime = dateTime,
            FromParticipant = csv.GetString(3),
            ToParticipant = csv.GetString(4)
        };
    }

    private AccumulationMeterDataRecord ParseAccumulationMeterDataRecord(CsvDataReader csv)
    {
        return new AccumulationMeterDataRecord
        {
            NMI = csv.GetString(1),
            NMIConfiguration = csv.GetString(2),
            RegisterId = csv.GetString(3),
            NMISuffix = csv.GetString(4),
            MDMDataStreamIdentifier = GetOptionalString(csv, 5),
            MeterSerialNumber = csv.GetString(6),
            DirectionIndicator = csv.GetString(7),
            PreviousRegisterRead = csv.GetString(8),
            PreviousRegisterReadDateTime = ParseDateTime(csv, 9),
            PreviousQualityMethod = csv.GetString(10),
            PreviousReasonCode = GetOptionalString(csv, 11),
            PreviousReasonDescription = GetOptionalString(csv, 12),
            CurrentRegisterRead = csv.GetString(13),
            CurrentRegisterReadDateTime = ParseDateTime(csv, 14),
            CurrentQualityMethod = csv.GetString(15),
            CurrentReasonCode = GetOptionalString(csv, 16),
            CurrentReasonDescription = GetOptionalString(csv, 17),
            Quantity = csv.GetDecimal(18),
            UOM = csv.GetString(19),
            NextScheduledReadDate = ParseOptionalDate(csv, 20),
            UpdateDateTime = ParseDateTime(csv, 21),
            MSATSLoadDateTime = ParseOptionalDateTime(csv, 22),
        };
    }

    private B2BDetailsRecord ParseB2BDetailsRecord(CsvDataReader csv)
    {
        return new B2BDetailsRecord
        {
            PreviousTransCode = csv.GetString(1),
            PreviousRetServiceOrder = GetOptionalString(csv, 2),
            CurrentTransCode = csv.GetString(3),
            CurrentRetServiceOrder = GetOptionalString(csv, 4),
        };
    }

    private static DateTime ParseDateTime(CsvDataReader csv, int index)
    {
        return DateTime.ParseExact(csv.GetString(index), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
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
