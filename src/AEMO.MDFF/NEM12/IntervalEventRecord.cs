using AEMO.MDFF.Abstractions;

namespace AEMO.MDFF.NEM12;

public sealed class IntervalEventRecord : IMdffRecord
{
    public string RecordIndicator => "400";
    public int StartInterval { get; set; }
    public int EndInterval { get; set; }
    public string QualityMethod { get; set; } = string.Empty;
    public string? ReasonCode { get; set; }
    public string? ReasonDescription { get; set; }
}
