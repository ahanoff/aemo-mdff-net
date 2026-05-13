using AEMO.MDFF.Abstractions;

namespace AEMO.MDFF.NEM12;

public sealed class B2BDetailsRecord : IMdffRecord
{
    public string RecordIndicator => "500";
    public string TransCode { get; set; } = string.Empty;
    public string? RetServiceOrder { get; set; }
    public DateTime? ReadDateTime { get; set; }
    public string? IndexRead { get; set; }
}
