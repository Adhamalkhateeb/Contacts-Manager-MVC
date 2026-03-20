using System;
using AutoFixture;
using ContactsManager.Application.Features.Persons.Queries;
using ContactsManager.Application.Features.Persons.Queries.GetPersons;
using ContactsManager.Domain.Persons;
using ContactsManager.Domain.Persons.Enums;
using ContactsManager.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ContactsManager.UnitTests.Persons.Handlers;

public class GetPersonsQueryHandlerUnitTests
{
    private readonly AppDbContext _dbContext;
    private readonly GetPersonsQueryHandler _handler;
    private readonly IFixture _fixture;

    public GetPersonsQueryHandlerUnitTests()
    {
        _fixture = new Fixture();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_fixture.Create<Guid>().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _handler = new GetPersonsQueryHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_WhenNoPersons_ReturnsEmptyList()
    {
        var result = await _handler.Handle(new GetPersonsQuery(), CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenPersonsExist_ReturnsPersonList()
    {
        // Arrange
        var person1 = Person.Create(
            _fixture.Create<Guid>(),
            "Test User",
            Gender.Male,
            _fixture.Create<DateTime>().Date.AddYears(-30),
            "test@gmail.com",
            null,
            false,
            _fixture.Create<Guid>()
        );

        var person2 = Person.Create(
            _fixture.Create<Guid>(),
            "Test User",
            Gender.Male,
            _fixture.Create<DateTime>().Date.AddYears(-31),
            "test1@gmail.com",
            null,
            false,
            _fixture.Create<Guid>()
        );

        _dbContext.Persons.AddRange(person1.Value, person2.Value);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(new GetPersonsQuery(), CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(2);
    }
}
