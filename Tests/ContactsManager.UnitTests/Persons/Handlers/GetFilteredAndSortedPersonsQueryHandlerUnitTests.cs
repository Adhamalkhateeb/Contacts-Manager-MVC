using AutoFixture;
using ContactsManager.Application.Features.Persons.DTOs;
using ContactsManager.Application.Features.Persons.Enums;
using ContactsManager.Application.Features.Persons.Queries.GetFilteredAndSortedPersons;
using ContactsManager.Domain.Countries;
using ContactsManager.Domain.Persons;
using ContactsManager.Domain.Persons.Enums;
using ContactsManager.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace ContactsManager.UnitTests.Persons.Handlers;

public class GetFilteredAndSortedPersonsQueryHandlerUnitTests
{
    private readonly GetFilteredAndSortedPersonsQueryHandler _handler;
    private readonly AppDbContext _dbContext;
    private readonly IFixture _fixture;

    public GetFilteredAndSortedPersonsQueryHandlerUnitTests()
    {
        _fixture = new Fixture();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_fixture.Create<Guid>().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _handler = new GetFilteredAndSortedPersonsQueryHandler(
            _dbContext,
            new NullLogger<GetFilteredAndSortedPersonsQueryHandler>()
        );
    }

    private async Task SeedPersonsAsync()
    {
        var egypt = Country.Create(_fixture.Create<Guid>(), "Egypt").Value;
        var usa = Country.Create(_fixture.Create<Guid>(), "USA").Value;

        _dbContext.Countries.AddRange(egypt, usa);

        var persons = new[]
        {
            Person
                .Create(
                    _fixture.Create<Guid>(),
                    "Charlie",
                    Gender.Male,
                    new DateTime(1988, 1, 2),
                    "charlie@test.com",
                    "Cairo",
                    false,
                    egypt.Id
                )
                .Value,
            Person
                .Create(
                    _fixture.Create<Guid>(),
                    "Alice",
                    Gender.Female,
                    new DateTime(1998, 7, 10),
                    "alice@test.com",
                    "Alex",
                    true,
                    egypt.Id
                )
                .Value,
            Person
                .Create(
                    _fixture.Create<Guid>(),
                    "Bob",
                    Gender.Male,
                    new DateTime(1992, 5, 20),
                    "bob@test.com",
                    "NYC",
                    false,
                    usa.Id
                )
                .Value,
        };

        _dbContext.Persons.AddRange(persons);
        await _dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WithSearchAndSort_ReturnsFilteredData()
    {
        var country = Country.Create(_fixture.Create<Guid>(), "Egypt").Value;
        _dbContext.Countries.Add(country);

        var p1 = Person
            .Create(
                _fixture.Create<Guid>(),
                "Charlie",
                Gender.Male,
                null,
                "charlie@test.com",
                null,
                false,
                country.Id
            )
            .Value;
        var p2 = Person
            .Create(
                _fixture.Create<Guid>(),
                "Alice",
                Gender.Female,
                null,
                "alice@test.com",
                null,
                false,
                country.Id
            )
            .Value;
        var p3 = Person
            .Create(
                _fixture.Create<Guid>(),
                "Bob",
                Gender.Male,
                null,
                "bob@test.com",
                null,
                false,
                country.Id
            )
            .Value;

        _dbContext.Persons.AddRange(p1, p2, p3);
        await _dbContext.SaveChangesAsync();

        var query = new GetFilteredAndSortedPersonsQuery(
            nameof(PersonDto.Name),
            "alice",
            nameof(PersonDto.Name),
            SortOrder.ASC
        );

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Name.Should().Be("Alice");
    }

    [Fact]
    public async Task Handle_WhenSearchValueIsWhitespace_ReturnsAllPersons()
    {
        await SeedPersonsAsync();

        var query = new GetFilteredAndSortedPersonsQuery(
            nameof(PersonDto.Name),
            "   ",
            nameof(PersonDto.Name),
            SortOrder.ASC
        );

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WhenSearchByUnknown_ReturnsAllPersons()
    {
        await SeedPersonsAsync();

        var query = new GetFilteredAndSortedPersonsQuery(
            "UnknownField",
            "Alice",
            nameof(PersonDto.Name),
            SortOrder.ASC
        );

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WhenGenderFilterInvalid_ReturnsEmpty()
    {
        await SeedPersonsAsync();

        var query = new GetFilteredAndSortedPersonsQuery(
            nameof(PersonDto.Gender),
            "invalid-gender",
            nameof(PersonDto.Name),
            SortOrder.ASC
        );

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenGenderFilterCaseInsensitive_ReturnsMatchingRows()
    {
        await SeedPersonsAsync();

        var query = new GetFilteredAndSortedPersonsQuery(
            nameof(PersonDto.Gender),
            "female",
            nameof(PersonDto.Name),
            SortOrder.ASC
        );

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Name.Should().Be("Alice");
    }

    [Fact]
    public async Task Handle_WhenDateFilterInvalid_ReturnsEmpty()
    {
        await SeedPersonsAsync();

        var query = new GetFilteredAndSortedPersonsQuery(
            nameof(PersonDto.DateOfBirth),
            "not-a-date",
            nameof(PersonDto.Name),
            SortOrder.ASC
        );

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenDateFilterValid_ReturnsExactDateMatch()
    {
        await SeedPersonsAsync();

        var query = new GetFilteredAndSortedPersonsQuery(
            nameof(PersonDto.DateOfBirth),
            "1998-07-10",
            nameof(PersonDto.Name),
            SortOrder.ASC
        );

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Name.Should().Be("Alice");
    }

    [Fact]
    public async Task Handle_WhenSortByNameDescending_ReturnsExpectedOrder()
    {
        await SeedPersonsAsync();

        var query = new GetFilteredAndSortedPersonsQuery(
            nameof(PersonDto.Name),
            null,
            nameof(PersonDto.Name),
            SortOrder.DESC
        );

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Select(p => p.Name).Should().Equal("Charlie", "Bob", "Alice");
    }

    [Fact]
    public async Task Handle_WhenSortByCountryAscending_ReturnsCountryOrder()
    {
        await SeedPersonsAsync();

        var query = new GetFilteredAndSortedPersonsQuery(
            nameof(PersonDto.Name),
            null,
            nameof(PersonDto.CountryId),
            SortOrder.ASC
        );

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Select(p => p.Country).Should().ContainInOrder("Egypt", "Egypt", "USA");
    }

    [Fact]
    public async Task Handle_WhenOrderByUnknown_KeepsAllRows()
    {
        await SeedPersonsAsync();

        var query = new GetFilteredAndSortedPersonsQuery(
            nameof(PersonDto.Name),
            null,
            "UnknownOrder",
            SortOrder.ASC
        );

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
    }
}
