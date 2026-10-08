namespace IRoute.Commerce.Application.Commerces;

public class UploadResult
{
    public bool Success { get; private set; }
    public string? Error { get; private set; }
    public IReadOnlyList<string> Errors { get; private set; } = [];
    public string FileName { get; private set; } = string.Empty;
    public int Inserted { get; private set; }
    public IReadOnlyList<DateOnly> ProcessDates { get; private set; } = [];

    private UploadResult()
    {
    }

    public static UploadResult Ok(string fileName, int inserted, IReadOnlyList<DateOnly> processDates) =>
        new() { Success = true, FileName = fileName, Inserted = inserted, ProcessDates = processDates };

    public static UploadResult Fail(string error, IReadOnlyList<string>? errors = null) =>
        new() { Success = false, Error = error, Errors = errors ?? [] };
}
