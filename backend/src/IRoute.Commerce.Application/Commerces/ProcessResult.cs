namespace IRoute.Commerce.Application.Commerces;

public class ProcessResult
{
    public DateOnly ProcessDate { get; }
    public int Quarantined { get; }

    public ProcessResult(DateOnly processDate, int quarantined)
    {
        ProcessDate = processDate;
        Quarantined = quarantined;
    }
}
