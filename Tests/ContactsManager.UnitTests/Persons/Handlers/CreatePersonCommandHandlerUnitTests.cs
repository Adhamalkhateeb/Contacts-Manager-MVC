using AutoFixture;
using ContactsManager.Application.Features.Persons.Commands.CreatePerson;
using ContactsManager.Domain.Common.Results;
using ContactsManager.Domain.Countries;
using ContactsManager.Domain.Persons;
using ContactsManager.Domain.Persons.Enums;
using ContactsManager.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace ContactsManager.UnitTests.Persons.Handlers;

public class CreatePersonCommandHandlerUnitTests
{
    private readonly AppDbContext _dbContext;
    private readonly CreatePersonCommandHandler _handler;
    private readonly IFixture _fixture;

    public CreatePersonCommandHandlerUnitTests()
    {
        _fixture = new Fixture();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_fixture.Create<Guid>().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _handler = new CreatePersonCommandHandler(
            _dbContext,
            new NullLogger<CreatePersonCommandHandler>()
        );
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ReturnsConflict()
    {
        var country = Country.Create(_fixture.Create<Guid>(), "Egypt").Value;
        _dbContext.Countries.Add(country);

        var existing = Person
            .Create(
                _fixture.Create<Guid>(),
                "Existing",
                Gender.Male,
                _fixture.Create<DateTime>().Date.AddYears(-30),
                "existing@test.com",
                null,
                false,
                country.Id
            )
            .Value;

        _dbContext.Persons.Add(existing);
        await _dbContext.SaveChangesAsync();

        var command = new CreatePersonCommand(
            "New Name",
            Gender.Female,
            _fixture.Create<DateTime>().Date.AddYears(-29),
            " Existing@Test.com ",
            "Address",
            true,
            country.Id
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.TopError.Type.Should().Be(ErrorKind.Conflict);
    }

    [Fact]
    public async Task Handle_WhenCountryDoesNotExist_ReturnsNotFound()
    {
        var command = new CreatePersonCommand(
            "New Name",
            Gender.Female,
            _fixture.Create<DateTime>().Date.AddYears(-29),
            "new@test.com",
            "Address",
            true,
            _fixture.Create<Guid>()
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.TopError.Type.Should().Be(ErrorKind.NotFound);
        result.TopError.Code.Should().Be("Application_CreatePerson_Country_NotFound");
    }

    [Fact]
    public async Task Handle_WhenDomainValidationFails_ReturnsValidationError()
    {
        var country = Country.Create(_fixture.Create<Guid>(), "Egypt").Value;
        _dbContext.Countries.Add(country);
        await _dbContext.SaveChangesAsync();

        var command = new CreatePersonCommand(
            " ",
            Gender.Male,
            _fixture.Create<DateTime>().Date.AddYears(-30),
            "new@test.com",
            null,
            false,
            country.Id
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.TopError.Type.Should().Be(ErrorKind.Validation);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_CreatesPersonSuccessfully()
    {
        var country = Country.Create(_fixture.Create<Guid>(), "Egypt").Value;
        _dbContext.Countries.Add(country);
        await _dbContext.SaveChangesAsync();

        var command = new CreatePersonCommand(
            "  New Person  ",
            Gender.Female,
            _fixture.Create<DateTime>().Date.AddYears(-20),
            " NEW@TEST.COM ",
            " Address ",
            true,
            country.Id
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        (await _dbContext.Persons.CountAsync()).Should().Be(1);
    }
}
