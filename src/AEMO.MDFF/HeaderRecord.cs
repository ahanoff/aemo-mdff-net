using AEMO.MDFF.Abstractions;

namespace AEMO.MDFF;

public sealed class HeaderRecord : IMdffRecord
{
    public string RecordIndicator => "100";
    public string VersionHeader { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
    public string FromParticipant { get; set; } = string.Empty;
    public string ToParticipant { get; set; } = string.Empty;
}
