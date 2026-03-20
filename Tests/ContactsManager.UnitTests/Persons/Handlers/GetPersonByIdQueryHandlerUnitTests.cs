using AutoFixture;
using ContactsManager.Application.Features.Persons.Queries.GetPersonById;
using ContactsManager.Domain.Common.Results;
using ContactsManager.Domain.Countries;
using ContactsManager.Domain.Persons;
using ContactsManager.Domain.Persons.Enums;
using ContactsManager.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace ContactsManager.UnitTests.Persons.Handlers;

public class GetPersonByIdQueryHandlerUnitTests
{
    private readonly AppDbContext _dbContext;
    private readonly GetPersonByIdQueryHandler _handler;
    private readonly IFixture _fixture;

    public GetPersonByIdQueryHandlerUnitTests()
    {
        _fixture = new Fixture();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_fixture.Create<Guid>().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _handler = new GetPersonByIdQueryHandler(
            _dbContext,
            new NullLogger<GetPersonByIdQueryHandler>()
        );
    }

    [Fact]
    public async Task Handle_WhenPersonNotFound_ReturnsNotFound()
    {
        var result = await _handler.Handle(
            new GetPersonByIdQuery(_fixture.Create<Guid>()),
            CancellationToken.None
        );

        result.IsError.Should().BeTrue();
        result.TopError.Type.Should().Be(ErrorKind.NotFound);
        result.TopError.Code.Should().Be("Application_GetPersonById_PersonNotFound");
    }

    [Fact]
    public async Task Handle_WhenPersonExists_ReturnsPersonDto()
    {
        var country = Country.Create(_fixture.Create<Guid>(), "Egypt").Value;
        _dbContext.Countries.Add(country);

        var person = Person
            .Create(
                _fixture.Create<Guid>(),
                "Existing",
                Gender.Female,
                _fixture.Create<DateTime>().Date.AddYears(-28),
                "existing@test.com",
                "Address",
                true,
                country.Id
            )
            .Value;

        _dbContext.Persons.Add(person);
        await _dbContext.SaveChangesAsync();

        var result = await _handler.Handle(
            new GetPersonByIdQuery(person.Id),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(person.Id);
        result.Value.Name.Should().Be("Existing");
        result.Value.Email.Should().Be("existing@test.com");
    }
}
