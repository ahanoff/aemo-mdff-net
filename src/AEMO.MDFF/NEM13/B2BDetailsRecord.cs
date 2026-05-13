using AEMO.MDFF.Abstractions;

namespace AEMO.MDFF.NEM13;

public sealed class B2BDetailsRecord : IMdffRecord
{
    public string RecordIndicator => "550";
    public string PreviousTransCode { get; set; } = string.Empty;
    public string? PreviousRetServiceOrder { get; set; }
    public string CurrentTransCode { get; set; } = string.Empty;
    public string? CurrentRetServiceOrder { get; set; }
}
