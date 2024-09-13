using AEMO.MDFF.Abstractions;

namespace AEMO.MDFF;

public class HeaderRecord : IMdffRecord
{
    public string RecordIndicator => "100";
    public string VersionHeader { get; set; }
    public DateTimeOffset DateTime { get; set; }
    public string FromParticipant { get; set; }
    public string ToParticipant { get; set; }
}