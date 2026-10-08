namespace IRoute.Commerce.Application.Commerces;

public class CommerceFileReadResult
{
    public List<Domain.Entities.Commerce> Commerces { get; } = [];
    public List<string> Errors { get; } = [];

    public bool IsValid => Errors.Count == 0;
}
