namespace AEMO.MDFF.Abstractions;

public interface IMdffReader
{
    public IAsyncEnumerable<IMdffRecord> ReadAsync(Stream stream, CancellationToken ct);
}
