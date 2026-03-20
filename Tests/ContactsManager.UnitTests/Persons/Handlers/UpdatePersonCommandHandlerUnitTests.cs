using AutoFixture;
using ContactsManager.Application.Features.Persons.Commands.UpdatePerson;
using ContactsManager.Domain.Common.Results;
using ContactsManager.Domain.Countries;
using ContactsManager.Domain.Persons;
using ContactsManager.Domain.Persons.Enums;
using ContactsManager.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace ContactsManager.UnitTests.Persons.Handlers;

public class UpdatePersonCommandHandlerUnitTests
{
    private readonly AppDbContext _dbContext;
    private readonly UpdatePersonCommandHandler _handler;
    private readonly IFixture _fixture;

    public UpdatePersonCommandHandlerUnitTests()
    {
        _fixture = new Fixture();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_fixture.Create<Guid>().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _handler = new UpdatePersonCommandHandler(
            _dbContext,
            new NullLogger<UpdatePersonCommandHandler>()
        );
    }

    [Fact]
    public async Task Handle_WhenPersonNotFound_ReturnsNotFound()
    {
        var command = new UpdatePersonCommand(
            _fixture.Create<Guid>(),
            "Name",
            Gender.Male,
            _fixture.Create<DateTime>().Date.AddYears(-30),
            "name@test.com",
            null,
            false,
            _fixture.Create<Guid>()
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.TopError.Type.Should().Be(ErrorKind.NotFound);
        result.TopError.Code.Should().Be("Application_UpdatePerson_PersonNotFound");
    }

    [Fact]
    public async Task Handle_WhenEmailIsDuplicate_ReturnsConflict()
    {
        var country = Country.Create(_fixture.Create<Guid>(), "Egypt").Value;
        _dbContext.Countries.Add(country);

        var personA = Person
            .Create(
                _fixture.Create<Guid>(),
                "A",
                Gender.Male,
                _fixture.Create<DateTime>().Date.AddYears(-30),
                "a@test.com",
                null,
                false,
                country.Id
            )
            .Value;

        var personB = Person
            .Create(
                _fixture.Create<Guid>(),
                "B",
                Gender.Female,
                _fixture.Create<DateTime>().Date.AddYears(-29),
                "b@test.com",
                null,
                false,
                country.Id
            )
            .Value;

        _dbContext.Persons.AddRange(personA, personB);
        await _dbContext.SaveChangesAsync();

        var command = new UpdatePersonCommand(
            personA.Id,
            "Updated",
            Gender.Male,
            _fixture.Create<DateTime>().Date.AddYears(-30),
            "b@test.com",
            null,
            false,
            country.Id
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.TopError.Type.Should().Be(ErrorKind.Conflict);
    }

    [Fact]
    public async Task Handle_WhenCountryNotFound_ReturnsNotFound()
    {
        var country = Country.Create(_fixture.Create<Guid>(), "Egypt").Value;
        _dbContext.Countries.Add(country);

        var person = Person
            .Create(
                _fixture.Create<Guid>(),
                "A",
                Gender.Male,
                _fixture.Create<DateTime>().Date.AddYears(-30),
                "a@test.com",
                null,
                false,
                country.Id
            )
            .Value;

        _dbContext.Persons.Add(person);
        await _dbContext.SaveChangesAsync();

        var command = new UpdatePersonCommand(
            person.Id,
            "Updated",
            Gender.Male,
            _fixture.Create<DateTime>().Date.AddYears(-30),
            "a@test.com",
            null,
            false,
            _fixture.Create<Guid>()
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.TopError.Type.Should().Be(ErrorKind.NotFound);
        result.TopError.Code.Should().Be("Application_UpdatePerson_CountryNotFound");
    }

    [Fact]
    public async Task Handle_WhenDomainValidationFails_ReturnsValidationError()
    {
        var country = Country.Create(_fixture.Create<Guid>(), "Egypt").Value;
        _dbContext.Countries.Add(country);

        var person = Person
            .Create(
                _fixture.Create<Guid>(),
                "A",
                Gender.Male,
                _fixture.Create<DateTime>().Date.AddYears(-30),
                "a@test.com",
                null,
                false,
                country.Id
            )
            .Value;

        _dbContext.Persons.Add(person);
        await _dbContext.SaveChangesAsync();

        var command = new UpdatePersonCommand(
            person.Id,
            " ",
            Gender.Male,
            _fixture.Create<DateTime>().Date.AddYears(-30),
            "a@test.com",
            null,
            false,
            country.Id
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.TopError.Type.Should().Be(ErrorKind.Validation);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_UpdatesPerson()
    {
        var oldCountry = Country.Create(_fixture.Create<Guid>(), "Egypt").Value;
        var newCountry = Country.Create(_fixture.Create<Guid>(), "USA").Value;
        _dbContext.Countries.AddRange(oldCountry, newCountry);

        var person = Person
            .Create(
                _fixture.Create<Guid>(),
                "Old",
                Gender.Male,
                _fixture.Create<DateTime>().Date.AddYears(-30),
                "old@test.com",
                "OldAddress",
                false,
                oldCountry.Id
            )
            .Value;

        _dbContext.Persons.Add(person);
        await _dbContext.SaveChangesAsync();

        var command = new UpdatePersonCommand(
            person.Id,
            "New Name",
            Gender.Female,
            _fixture.Create<DateTime>().Date.AddYears(-25),
            "new@test.com",
            "NewAddress",
            true,
            newCountry.Id
        );

        var result = await _handler.Handle(command, CancellationToken.None);
        var updated = await _dbContext.Persons.FindAsync(person.Id);

        result.IsSuccess.Should().BeTrue();
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("New Name");
        updated.Email.Should().Be("new@test.com");
        updated.CountryId.Should().Be(newCountry.Id);
    }
}
