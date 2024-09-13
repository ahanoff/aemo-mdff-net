using AEMO.MDFF.Abstractions;

namespace AEMO.MDFF.NEM13;

public class Nem13Reader : IMdffReader
{
    public IAsyncEnumerable<IMdffRecord> ReadAsync(Stream stream, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}