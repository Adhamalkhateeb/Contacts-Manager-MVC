using AutoFixture;
using ContactsManager.Application.Features.Persons.Commands.RemovePerson;
using ContactsManager.Domain.Common.Results;
using ContactsManager.Domain.Countries;
using ContactsManager.Domain.Persons;
using ContactsManager.Domain.Persons.Enums;
using ContactsManager.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace ContactsManager.UnitTests.Persons.Handlers;

public class RemovePersonCommandHandlerUnitTests
{
    private readonly AppDbContext _dbContext;
    private readonly RemovePersonCommandHandler _handler;
    private readonly IFixture _fixture;

    public RemovePersonCommandHandlerUnitTests()
    {
        _fixture = new Fixture();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_fixture.Create<Guid>().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _handler = new RemovePersonCommandHandler(
            _dbContext,
            new NullLogger<RemovePersonCommandHandler>()
        );
    }

    [Fact]
    public async Task Handle_WhenPersonNotFound_ReturnsNotFound()
    {
        var result = await _handler.Handle(
            new RemovePersonCommand(_fixture.Create<Guid>()),
            CancellationToken.None
        );

        result.IsError.Should().BeTrue();
        result.TopError.Type.Should().Be(ErrorKind.NotFound);
        result.TopError.Code.Should().Be("Application_RemovePerson_PersonNotFound");
    }

    [Fact]
    public async Task Handle_WhenPersonExists_RemovesPersonAndReturnsDeleted()
    {
        var country = Country.Create(_fixture.Create<Guid>(), "Egypt").Value;
        _dbContext.Countries.Add(country);

        var person = Person
            .Create(
                _fixture.Create<Guid>(),
                "ToDelete",
                Gender.Male,
                _fixture.Create<DateTime>().Date.AddYears(-30),
                "delete@test.com",
                null,
                false,
                country.Id
            )
            .Value;

        _dbContext.Persons.Add(person);
        await _dbContext.SaveChangesAsync();

        var result = await _handler.Handle(
            new RemovePersonCommand(person.Id),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        (await _dbContext.Persons.FindAsync(person.Id)).Should().BeNull();
    }
}
