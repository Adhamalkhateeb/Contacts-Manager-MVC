using AutoFixture;
using ContactsManager.Application.Features.Countries.Queries.GetCountries;
using ContactsManager.Domain.Common.Results;
using ContactsManager.Domain.Countries;
using ContactsManager.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ContactsManager.UnitTests.Countries.Handlers;

public class GetCountriesQueryHandlerUnitTests
{
    private readonly AppDbContext _dbContext;
    private readonly GetCountriesQueryHandler _handler;
    private readonly IFixture _fixture;

    public GetCountriesQueryHandlerUnitTests()
    {
        _fixture = new Fixture();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_fixture.Create<Guid>().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _handler = new GetCountriesQueryHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_WhenNoCountries_ReturnsNotFound()
    {
        var result = await _handler.Handle(new GetCountriesQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.TopError.Type.Should().Be(ErrorKind.NotFound);
        result
            .Errors.Should()
            .ContainSingle()
            .Which.Code.Should()
            .Be("Application_Countries_NotFound");
    }

    [Fact]
    public async Task Handle_WhenCountriesExist_ReturnsAllCountries()
    {
        var egypt = Country.Create(_fixture.Create<Guid>(), "Egypt").Value;
        var usa = Country.Create(_fixture.Create<Guid>(), "USA").Value;
        _dbContext.Countries.AddRange(egypt, usa);
        await _dbContext.SaveChangesAsync();

        var result = await _handler.Handle(new GetCountriesQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Select(c => c.Name).Should().Contain(["Egypt", "USA"]);
    }
}
