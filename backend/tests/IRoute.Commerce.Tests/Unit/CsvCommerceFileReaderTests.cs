using System.Text;
using IRoute.Commerce.Application.Commerces;
using IRoute.Commerce.Infrastructure.Files;
using Xunit;

namespace IRoute.Commerce.Tests.Unit;

public class CsvCommerceFileReaderTests
{
    private const string Header = "pc_processdate,pc_codcomred,pc_nomcomred,pc_tipdoc,pc_numdoc,pc_ciudad,pc_direccion,pc_telefono,pc_email";

    private static Task<CommerceFileReadResult> ReadAsync(string csv) =>
        new CsvCommerceFileReader().ReadAsync(new MemoryStream(Encoding.UTF8.GetBytes(csv)));

    [Fact]
    public async Task ReadAsync_ShouldKeepInvalidBusinessValues_ForLaterValidation()
    {
        var csv = $"{Header}\n2026-10-07,C001,\"Tienda, S.A.\",RUC,1790012345001,Quito,Av. Amazonas,022222222,a@b.com\n07/10/2026,C002,,CED,12AB,,,,\n";

        var result = await ReadAsync(csv);

        Assert.True(result.IsValid);
        Assert.Equal(2, result.Commerces.Count);
        Assert.Equal("Tienda, S.A.", result.Commerces[0].NomComRed);
        Assert.Equal(new DateOnly(2026, 10, 7), result.Commerces[1].ProcessDate);
        Assert.Null(result.Commerces[1].NomComRed);
        Assert.Equal("12AB", result.Commerces[1].NumDoc);
    }

    [Fact]
    public async Task ReadAsync_ShouldDetectSemicolonDelimiter()
    {
        var result = await ReadAsync(Header.Replace(',', ';') + "\n2026-10-07;C001;Tienda;RUC;123;Quito;Dir;099;a@b.com");

        Assert.Single(result.Commerces);
        Assert.Equal("Tienda", result.Commerces[0].NomComRed);
    }

    [Fact]
    public async Task ReadAsync_ShouldReportMissingColumns()
    {
        var result = await ReadAsync("pc_processdate,pc_nomcomred\n2026-10-07,Tienda");

        Assert.False(result.IsValid);
        Assert.Contains("pc_numdoc", result.Errors[0]);
    }

    [Fact]
    public async Task ReadAsync_ShouldReportInvalidDateWithLineNumber()
    {
        var result = await ReadAsync($"{Header}\n2026-10-07,C1,A,RUC,1,,,,\n\nfecha,C2,B,RUC,2,,,,");

        Assert.False(result.IsValid);
        Assert.StartsWith("Línea 4", result.Errors[0]);
    }

    [Fact]
    public async Task ReadAsync_ShouldReturnNoRows_WhenFileHasOnlyHeader()
    {
        var result = await ReadAsync(Header);

        Assert.True(result.IsValid);
        Assert.Empty(result.Commerces);
    }
}
