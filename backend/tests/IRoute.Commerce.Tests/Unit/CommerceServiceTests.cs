using IRoute.Commerce.Application.Abstractions;
using IRoute.Commerce.Application.Commerces;
using NSubstitute;
using Xunit;
using CommerceEntity = IRoute.Commerce.Domain.Entities.Commerce;

namespace IRoute.Commerce.Tests.Unit;

public class CommerceServiceTests
{
    private readonly ICommerceRepository _commerces = Substitute.For<ICommerceRepository>();
    private readonly ICommerceFileReader _fileReader = Substitute.For<ICommerceFileReader>();

    private CommerceService CreateService() => new(_commerces, _fileReader);

    [Theory]
    [InlineData("commerce.csv")]
    [InlineData("commerce_31022026.csv")]
    [InlineData("comercio_07102026.csv")]
    [InlineData("commerce_07102026.txt")]
    public async Task UploadAsync_ShouldFail_WhenFileNameIsInvalid(string fileName)
    {
        var result = await CreateService().UploadAsync(fileName, 10, Stream.Null);

        Assert.False(result.Success);
        await _commerces.DidNotReceive().AddRangeAsync(Arg.Any<IReadOnlyList<CommerceEntity>>());
    }

    [Fact]
    public async Task UploadAsync_ShouldFail_WhenFileIsEmpty()
    {
        var result = await CreateService().UploadAsync("commerce_07102026.csv", 0, Stream.Null);

        Assert.False(result.Success);
        Assert.Equal("El archivo se encuentra vacío.", result.Error);
    }

    [Fact]
    public async Task UploadAsync_ShouldFail_WhenFileHasOnlyHeader()
    {
        _fileReader.ReadAsync(Arg.Any<Stream>()).Returns(new CommerceFileReadResult());

        var result = await CreateService().UploadAsync("commerce_07102026.csv", 10, Stream.Null);

        Assert.False(result.Success);
        Assert.Equal("El archivo se encuentra vacío.", result.Error);
    }

    [Fact]
    public async Task UploadAsync_ShouldReturnFormatErrors_WhenFileHasInvalidLines()
    {
        var file = new CommerceFileReadResult();
        file.Errors.Add("Línea 2: pc_processdate inválida 'x'.");
        _fileReader.ReadAsync(Arg.Any<Stream>()).Returns(file);

        var result = await CreateService().UploadAsync("commerce_07102026.csv", 10, Stream.Null);

        Assert.False(result.Success);
        Assert.Single(result.Errors);
        await _commerces.DidNotReceive().AddRangeAsync(Arg.Any<IReadOnlyList<CommerceEntity>>());
    }

    [Fact]
    public async Task UploadAsync_ShouldPersistCommerces_WhenFileIsValid()
    {
        var file = new CommerceFileReadResult();
        file.Commerces.Add(new CommerceEntity { ProcessDate = new DateOnly(2026, 10, 8), NumDoc = "1" });
        file.Commerces.Add(new CommerceEntity { ProcessDate = new DateOnly(2026, 10, 7), NumDoc = "2" });
        file.Commerces.Add(new CommerceEntity { ProcessDate = new DateOnly(2026, 10, 8), NumDoc = "3" });
        _fileReader.ReadAsync(Arg.Any<Stream>()).Returns(file);
        _commerces.AddRangeAsync(Arg.Any<IReadOnlyList<CommerceEntity>>()).Returns(3);

        var result = await CreateService().UploadAsync("commerce_07102026.csv", 10, Stream.Null);

        Assert.True(result.Success);
        Assert.Equal(3, result.Inserted);
        Assert.Equal([new DateOnly(2026, 10, 7), new DateOnly(2026, 10, 8)], result.ProcessDates);
        await _commerces.Received(1).AddRangeAsync(Arg.Is<IReadOnlyList<CommerceEntity>>(l => l.Count == 3));
    }

    [Fact]
    public async Task ProcessAsync_ShouldReturnQuarantinedCount()
    {
        var date = new DateOnly(2026, 10, 7);
        _commerces.ProcessAsync(date).Returns(5);

        var result = await CreateService().ProcessAsync(date);

        Assert.Equal(date, result.ProcessDate);
        Assert.Equal(5, result.Quarantined);
    }
}
