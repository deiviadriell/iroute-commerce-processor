using System.Globalization;
using System.Text;
using IRoute.Commerce.Application.Abstractions;
using IRoute.Commerce.Application.Commerces;
using CommerceEntity = IRoute.Commerce.Domain.Entities.Commerce;

namespace IRoute.Commerce.Infrastructure.Files;

public class CsvCommerceFileReader : ICommerceFileReader
{
    private const int MaxReportedErrors = 50;
    private static readonly char[] Delimiters = [',', ';', '|', '\t'];
    private static readonly string[] DateFormats = ["yyyy-MM-dd", "dd/MM/yyyy", "dd-MM-yyyy", "ddMMyyyy", "yyyyMMdd", "yyyy/MM/dd"];

    private static readonly Dictionary<string, int> Columns = new()
    {
        ["pc_processdate"] = 10,
        ["pc_codcomred"] = 30,
        ["pc_nomcomred"] = 150,
        ["pc_tipdoc"] = 10,
        ["pc_numdoc"] = 30,
        ["pc_ciudad"] = 80,
        ["pc_direccion"] = 250,
        ["pc_telefono"] = 30,
        ["pc_email"] = 120
    };

    public async Task<CommerceFileReadResult> ReadAsync(Stream content)
    {
        var result = new CommerceFileReadResult();
        using var reader = new StreamReader(content, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var lineNumber = 0;

        async Task<string?> NextLineAsync()
        {
            while (await reader.ReadLineAsync() is { } text)
            {
                lineNumber++;
                if (!string.IsNullOrWhiteSpace(text))
                    return text;
            }
            return null;
        }

        var header = await NextLineAsync();
        if (header is null)
            return result;

        var delimiter = Delimiters.OrderByDescending(d => header.Count(c => c == d)).First();
        var positions = SplitLine(header, delimiter)
            .Select((name, index) => (Name: name.Trim().ToLowerInvariant(), Index: index))
            .GroupBy(c => c.Name)
            .ToDictionary(g => g.Key, g => g.First().Index);

        var missing = Columns.Keys.Where(c => !positions.ContainsKey(c)).ToList();
        if (missing.Count > 0)
        {
            result.Errors.Add($"Faltan columnas en la cabecera: {string.Join(", ", missing)}.");
            return result;
        }

        while (await NextLineAsync() is { } line)
        {
            var values = SplitLine(line, delimiter);
            var lineErrors = new List<string>();

            string? Get(string column)
            {
                var index = positions[column];
                var value = index < values.Count ? values[index].Trim() : null;
                if (string.IsNullOrEmpty(value))
                    return null;

                if (value.Length > Columns[column])
                    lineErrors.Add($"{column} supera {Columns[column]} caracteres");

                return value;
            }

            var rawDate = Get("pc_processdate");
            if (!DateOnly.TryParseExact(rawDate, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var processDate))
                lineErrors.Add($"pc_processdate inválida '{rawDate}'");

            var commerce = new CommerceEntity
            {
                ProcessDate = processDate,
                CodComRed = Get("pc_codcomred"),
                NomComRed = Get("pc_nomcomred"),
                TipDoc = Get("pc_tipdoc"),
                NumDoc = Get("pc_numdoc"),
                Ciudad = Get("pc_ciudad"),
                Direccion = Get("pc_direccion"),
                Telefono = Get("pc_telefono"),
                Email = Get("pc_email")
            };

            if (lineErrors.Count == 0)
                result.Commerces.Add(commerce);
            else if (result.Errors.Count < MaxReportedErrors)
                result.Errors.Add($"Línea {lineNumber}: {string.Join("; ", lineErrors)}.");
        }

        return result;
    }

    private static List<string> SplitLine(string line, char delimiter)
    {
        var values = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (inQuotes)
            {
                if (c == '"' && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else if (c == '"')
                    inQuotes = false;
                else
                    current.Append(c);
            }
            else if (c == '"')
                inQuotes = true;
            else if (c == delimiter)
            {
                values.Add(current.ToString());
                current.Clear();
            }
            else
                current.Append(c);
        }

        values.Add(current.ToString());
        return values;
    }
}
