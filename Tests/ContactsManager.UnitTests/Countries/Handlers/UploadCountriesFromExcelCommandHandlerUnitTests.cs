using System.Text;
using AutoFixture;
using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Application.Features.Countries.Commands.UploadCountryFromExcel;
using ContactsManager.Domain.Common.Results;
using ContactsManager.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ContactsManager.UnitTests.Countries.Handlers;

public class UploadCountriesFromExcelCommandHandlerUnitTests
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<ICountryImportService> _importServiceMock;
    private readonly UploadCountriesFromExcelCommandHandler _handler;
    private readonly IFixture _fixture;

    public UploadCountriesFromExcelCommandHandlerUnitTests()
    {
        _fixture = new Fixture();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_fixture.Create<Guid>().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _importServiceMock = new Mock<ICountryImportService>();

        _handler = new UploadCountriesFromExcelCommandHandler(
            _dbContext,
            new NullLogger<UploadCountriesFromExcelCommandHandler>(),
            _importServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenImportServiceFails_ReturnsTopError()
    {
        _importServiceMock
            .Setup(x =>
                x.GetCountryNamesFromExcelAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(Error.Validation("Import_InvalidFile", "Invalid excel"));

        var (stream, fileName, length) = CreateFakeExcelStream();
        var command = new UploadCountriesFromExcelCommand(stream, fileName, length);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.TopError.Code.Should().Be("Import_InvalidFile");
    }

    [Fact]
    public async Task Handle_WhenNoRowsParsed_ReturnsZero()
    {
        _importServiceMock
            .Setup(x =>
                x.GetCountryNamesFromExcelAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(Array.Empty<string>());

        var (stream, fileName, length) = CreateFakeExcelStream();
        var command = new UploadCountriesFromExcelCommand(stream, fileName, length);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ParsedCount.Should().Be(0);
        result.Value.InsertedCount.Should().Be(0);
        result.Value.DuplicateCount.Should().Be(0);
        result.Value.InvalidCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenRowsContainExistingAndDuplicates_InsertsOnlyNewUnique()
    {
        _dbContext.Countries.Add(
            ContactsManager.Domain.Countries.Country.Create(_fixture.Create<Guid>(), "Egypt").Value
        );
        await _dbContext.SaveChangesAsync();

        _importServiceMock
            .Setup(x =>
                x.GetCountryNamesFromExcelAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new[] { "Egypt", "USA", "usa", "France" });

        var (stream, fileName, length) = CreateFakeExcelStream();
        var command = new UploadCountriesFromExcelCommand(stream, fileName, length);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ParsedCount.Should().Be(4);
        result.Value.InsertedCount.Should().Be(2);
        result.Value.DuplicateCount.Should().Be(2);
        result.Value.InvalidCount.Should().Be(0);
        (await _dbContext.Countries.CountAsync()).Should().Be(3);
    }

    [Fact]
    public async Task Handle_WhenRowsContainInvalidNames_SkipsInvalidRows()
    {
        _importServiceMock
            .Setup(x =>
                x.GetCountryNamesFromExcelAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new[] { " ", "USA", "  ", "Egypt" });

        var (stream, fileName, length) = CreateFakeExcelStream();
        var command = new UploadCountriesFromExcelCommand(stream, fileName, length);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ParsedCount.Should().Be(4);
        result.Value.InsertedCount.Should().Be(2);
        result.Value.DuplicateCount.Should().Be(0);
        result.Value.InvalidCount.Should().Be(2);
        (await _dbContext.Countries.CountAsync()).Should().Be(2);
    }

    private static (Stream stream, string fileName, long length) CreateFakeExcelStream()
    {
        var fixture = new Fixture();
        var content = Encoding.UTF8.GetBytes(fixture.Create<string>());
        var stream = new MemoryStream(content);
        return (stream, "countries.xlsx", content.Length);
    }
}
