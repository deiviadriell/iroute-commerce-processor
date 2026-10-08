using System.Globalization;
using System.Text.RegularExpressions;
using IRoute.Commerce.Application.Abstractions;
using IRoute.Commerce.Domain.Entities;

namespace IRoute.Commerce.Application.Commerces;

public partial class CommerceService
{
    private readonly ICommerceRepository _commerces;
    private readonly ICommerceFileReader _fileReader;

    public CommerceService(ICommerceRepository commerces, ICommerceFileReader fileReader)
    {
        _commerces = commerces;
        _fileReader = fileReader;
    }

    public async Task<UploadResult> UploadAsync(string fileName, long length, Stream content)
    {
        if (length == 0)
            return UploadResult.Fail("El archivo se encuentra vacío.");

        if (!IsValidFileName(fileName))
            return UploadResult.Fail("El archivo debe llamarse commerce_DDMMYYYY.csv con una fecha válida.");

        var file = await _fileReader.ReadAsync(content);

        if (!file.IsValid)
            return UploadResult.Fail("El archivo contiene errores de formato.", file.Errors);

        if (file.Commerces.Count == 0)
            return UploadResult.Fail("El archivo se encuentra vacío.");

        var inserted = await _commerces.AddRangeAsync(file.Commerces);

        var processDates = file.Commerces
            .Select(c => c.ProcessDate)
            .Distinct()
            .Order()
            .ToList();

        return UploadResult.Ok(Path.GetFileName(fileName), inserted, processDates);
    }

    public async Task<ProcessResult> ProcessAsync(DateOnly processDate)
    {
        var quarantined = await _commerces.ProcessAsync(processDate);
        return new ProcessResult(processDate, quarantined);
    }

    public Task<IReadOnlyList<QuarantinedCommerce>> GetQuarantinedAsync(DateOnly? processDate) =>
        _commerces.GetQuarantinedAsync(processDate);

    public Task<IReadOnlyList<ProcessDateSummary>> GetProcessDatesAsync() =>
        _commerces.GetProcessDatesAsync();

    private static bool IsValidFileName(string? fileName)
    {
        var match = FileNamePattern().Match(Path.GetFileName(fileName ?? string.Empty));
        return match.Success &&
               DateOnly.TryParseExact(match.Groups["date"].Value, "ddMMyyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
    }

    [GeneratedRegex(@"^commerce_(?<date>\d{8})\.csv$", RegexOptions.IgnoreCase)]
    private static partial Regex FileNamePattern();
}
