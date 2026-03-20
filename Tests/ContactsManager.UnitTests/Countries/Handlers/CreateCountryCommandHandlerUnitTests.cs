using AutoFixture;
using ContactsManager.Application.Features.Countries.Commands.CreateCountry;
using ContactsManager.Domain.Common.Results;
using ContactsManager.Domain.Countries;
using ContactsManager.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace ContactsManager.UnitTests.Countries.Handlers;

public class CreateCountryCommandHandlerUnitTests
{
    private readonly AppDbContext _dbContext;
    private readonly CreateCountryCommandHandler _handler;
    private readonly IFixture _fixture;

    public CreateCountryCommandHandlerUnitTests()
    {
        _fixture = new Fixture();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_fixture.Create<Guid>().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _handler = new CreateCountryCommandHandler(
            _dbContext,
            new NullLogger<CreateCountryCommandHandler>()
        );
    }

    [Fact]
    public async Task Handle_WhenCountryAlreadyExists_ReturnsConflict()
    {
        var existing = Country.Create(_fixture.Create<Guid>(), "egypt").Value;
        _dbContext.Countries.Add(existing);
        await _dbContext.SaveChangesAsync();

        var command = new CreateCountryCommand(" Egypt ");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.TopError.Type.Should().Be(ErrorKind.Conflict);
        result.TopError.Code.Should().Be("Application_CreateCountry_CountryExists");
    }

    [Fact]
    public async Task Handle_WhenCountryNameIsInvalid_ReturnsValidationError()
    {
        var command = new CreateCountryCommand("   ");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.TopError.Type.Should().Be(ErrorKind.Validation);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_CreatesCountry()
    {
        var command = new CreateCountryCommand(" Egypt ");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("egypt");
        (await _dbContext.Countries.CountAsync()).Should().Be(1);
    }
}
