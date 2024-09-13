using AEMO.MDFF.Abstractions;

namespace AEMO.MDFF;

public class EndRecord : IMdffRecord
{
    public string RecordIndicator => "900";
}