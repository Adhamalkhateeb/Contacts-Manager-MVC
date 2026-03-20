using AutoFixture;
using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Application.Features.Persons.DTOs;
using ContactsManager.Application.Features.Persons.Queries.GetPersonsAsExcel;
using ContactsManager.Domain.Common.Results;
using ContactsManager.Domain.Countries;
using ContactsManager.Domain.Persons;
using ContactsManager.Domain.Persons.Enums;
using ContactsManager.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ContactsManager.UnitTests.Persons.Handlers;

public class GetPersonsAsExcelQueryHandlerUnitTests
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IPersonExportService> _exportServiceMock;
    private readonly GetPersonsExcelQueryHandler _handler;
    private readonly IFixture _fixture;

    public GetPersonsAsExcelQueryHandlerUnitTests()
    {
        _fixture = new Fixture();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_fixture.Create<Guid>().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _exportServiceMock = new Mock<IPersonExportService>();

        _handler = new GetPersonsExcelQueryHandler(
            _dbContext,
            new NullLogger<GetPersonsExcelQueryHandler>(),
            _exportServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenExportSucceeds_ReturnsExcelBytes()
    {
        var country = Country.Create(_fixture.Create<Guid>(), "Egypt").Value;
        _dbContext.Countries.Add(country);

        var person = Person
            .Create(
                _fixture.Create<Guid>(),
                "Jane",
                Gender.Female,
                _fixture.Create<DateTime>().Date.AddYears(-25),
                "jane@test.com",
                "Address",
                false,
                country.Id
            )
            .Value;

        _dbContext.Persons.Add(person);
        await _dbContext.SaveChangesAsync();

        var bytes = _fixture.CreateMany<byte>(3).ToArray();
        _exportServiceMock
            .Setup(x =>
                x.GenerateExcelAsync(
                    It.IsAny<IReadOnlyCollection<PersonDto>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(bytes);

        var result = await _handler.Handle(new GetPersonsExcelQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Equal(bytes);
    }

    [Fact]
    public async Task Handle_WhenExportReturnsEmptyBytes_ReturnsFailure()
    {
        _exportServiceMock
            .Setup(x =>
                x.GenerateExcelAsync(
                    It.IsAny<IReadOnlyCollection<PersonDto>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Array.Empty<byte>());

        var result = await _handler.Handle(new GetPersonsExcelQuery(), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.TopError.Type.Should().Be(ErrorKind.Failure);
    }
}
