using AEMO.MDFF.Abstractions;

namespace AEMO.MDFF.NEM12;

public sealed class IntervalDataRecord : IMdffRecord
{
    public string RecordIndicator => "300";
    public DateOnly IntervalDate { get; set; }
    public IReadOnlyCollection<decimal> IntervalValues { get; set; }
    public string QualityMethod { get; set; }
    public string ReasonCode { get; set; }
    public string ReasonDescription { get; set; }
    public DateTime UpdateDateTime { get; set; }
    public DateTime MSATSLoadDateTime { get; set; }
}