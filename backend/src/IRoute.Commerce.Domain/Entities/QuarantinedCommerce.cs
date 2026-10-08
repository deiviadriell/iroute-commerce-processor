namespace IRoute.Commerce.Domain.Entities;

public class QuarantinedCommerce : Commerce
{
    public long Id { get; set; }
    public long CommerceId { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public DateTime QuarantinedAt { get; set; }
}
