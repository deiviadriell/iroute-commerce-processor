using IRoute.Commerce.Application.Commerces;

namespace IRoute.Commerce.Application.Abstractions;

public interface ICommerceFileReader
{
    Task<CommerceFileReadResult> ReadAsync(Stream content);
}
