using AEMO.MDFF.Abstractions;

namespace AEMO.MDFF.NEM13;

public sealed class AccumulationMeterDataRecord : IMdffRecord
{
    public string RecordIndicator => "250";
    public string NMI { get; set; } = string.Empty;
    public string NMIConfiguration { get; set; } = string.Empty;
    public string RegisterId { get; set; } = string.Empty;
    public string NMISuffix { get; set; } = string.Empty;
    public string? MDMDataStreamIdentifier { get; set; }
    public string MeterSerialNumber { get; set; } = string.Empty;
    public string DirectionIndicator { get; set; } = string.Empty;
    public string PreviousRegisterRead { get; set; } = string.Empty;
    public DateTime PreviousRegisterReadDateTime { get; set; }
    public string PreviousQualityMethod { get; set; } = string.Empty;
    public string? PreviousReasonCode { get; set; }
    public string? PreviousReasonDescription { get; set; }
    public string CurrentRegisterRead { get; set; } = string.Empty;
    public DateTime CurrentRegisterReadDateTime { get; set; }
    public string CurrentQualityMethod { get; set; } = string.Empty;
    public string? CurrentReasonCode { get; set; }
    public string? CurrentReasonDescription { get; set; }
    public decimal Quantity { get; set; }
    public string UOM { get; set; } = string.Empty;
    public DateOnly? NextScheduledReadDate { get; set; }
    public DateTime UpdateDateTime { get; set; }
    public DateTime? MSATSLoadDateTime { get; set; }
}
