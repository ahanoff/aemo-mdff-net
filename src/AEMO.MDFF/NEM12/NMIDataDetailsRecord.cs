using AEMO.MDFF.Abstractions;

namespace AEMO.MDFF.NEM12;

public sealed class NMIDataDetailsRecord : IMdffRecord
{
    public string RecordIndicator => "200";
    public string NMI { get; set; }
    public string NMIConfiguration { get; set; }
    public string RegisterId { get; set; }
    public string NMISuffix { get; set; }
    public string MDMDataStreamIdentifier { get; set; }
    public string MeterSerialNumber { get; set; }
    public string UOM { get; set; }
    public int IntervalLength { get; set; }
    public DateTime NextScheduledReadDate { get; set; }
}