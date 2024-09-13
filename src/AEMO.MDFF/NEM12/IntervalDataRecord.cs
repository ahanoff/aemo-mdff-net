using AEMO.MDFF.Abstractions;

namespace AEMO.MDFF.NEM12;

public class IntervalDataRecord : IMdffRecord
{
    public string RecordIndicator => "300";
    public DateTimeOffset IntervalDate { get; set; }
    public IReadOnlyCollection<decimal> IntervalValues { get; set; }
    public string QualityMethod { get; set; }
    public string ReasonCode { get; set; }
    public string ReasonDescription { get; set; }
    public string UpdateDateTime { get; set; }
    public string MSATSLoadDateTime { get; set; }
}