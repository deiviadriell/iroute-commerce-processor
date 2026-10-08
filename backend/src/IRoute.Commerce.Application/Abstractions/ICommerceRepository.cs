using IRoute.Commerce.Domain.Entities;

namespace IRoute.Commerce.Application.Abstractions;

public interface ICommerceRepository
{
    Task<int> AddRangeAsync(IReadOnlyList<Domain.Entities.Commerce> commerces);
    Task<int> ProcessAsync(DateOnly processDate);
    Task<IReadOnlyList<QuarantinedCommerce>> GetQuarantinedAsync(DateOnly? processDate);
    Task<IReadOnlyList<ProcessDateSummary>> GetProcessDatesAsync();
}
