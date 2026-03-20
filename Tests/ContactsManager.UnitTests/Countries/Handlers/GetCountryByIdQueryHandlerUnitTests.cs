using AutoFixture;
using ContactsManager.Application.Features.Countries.Queries.GetCountryById;
using ContactsManager.Domain.Common.Results;
using ContactsManager.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace ContactsManager.UnitTests.Countries.Handlers;

public class GetCountryByIdQueryHandlerUnitTests
{
    private readonly AppDbContext _dbContext;
    private readonly GetCountryByIdQueryHandler _handler;
    private readonly IFixture _fixture;

    public GetCountryByIdQueryHandlerUnitTests()
    {
        _fixture = new Fixture();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_fixture.Create<Guid>().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _handler = new GetCountryByIdQueryHandler(
            new NullLogger<GetCountryByIdQueryHandler>(),
            _dbContext
        );
    }

    [Fact]
    public async Task Handle_WhenCountryNotFound_ReturnsNotFound()
    {
        var result = await _handler.Handle(
            new GetCountryByIdQuery(_fixture.Create<Guid>()),
            CancellationToken.None
        );

        result.IsError.Should().BeTrue();
        result.TopError.Type.Should().Be(ErrorKind.NotFound);
        result.TopError.Code.Should().Be("Application_GetCountryById_CountryNotFound");
    }

    [Fact]
    public async Task Handle_WhenCountryExists_ReturnsCountryDto()
    {
        var country = ContactsManager
            .Domain.Countries.Country.Create(_fixture.Create<Guid>(), "Egypt")
            .Value;
        _dbContext.Countries.Add(country);
        await _dbContext.SaveChangesAsync();

        var result = await _handler.Handle(
            new GetCountryByIdQuery(country.Id),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(country.Id);
        result.Value.Name.Should().Be("Egypt");
    }
}
