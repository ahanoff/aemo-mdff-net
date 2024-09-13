using AEMO.MDFF.Abstractions;

namespace AEMO.MDFF;

public sealed class EndRecord : IMdffRecord
{
    public string RecordIndicator => "900";
}