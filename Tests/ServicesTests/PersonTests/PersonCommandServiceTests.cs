using AutoFixture;
using Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RepositoriesContract;
using Serilog;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;
using Xunit.Abstractions;

namespace ServicesTests.PersonsTests;

public class PersonCommandServiceTests
{
    private readonly IPersonCommandService _sut;
    private readonly Mock<ICountryQueryService> _countriesServiceMock;
    private readonly Mock<IPersonsRepository> _personsRepositoryMock;
    private readonly IFixture _fixture;
    private readonly ITestOutputHelper _testOutputHelper;

    public PersonCommandServiceTests(ITestOutputHelper testOutputHelper)
    {
        _fixture = new Fixture();

        _personsRepositoryMock = new Mock<IPersonsRepository>();
        _countriesServiceMock = new Mock<ICountryQueryService>();
        var loggerMock = new Mock<ILogger<PersonCommandService>>();
        var diagnosticContextMock = new Mock<IDiagnosticContext>();

        _sut = new PersonCommandService(
            _personsRepositoryMock.Object,
            _countriesServiceMock.Object,
            loggerMock.Object,
            diagnosticContextMock.Object
        );

        _testOutputHelper = testOutputHelper;
    }

    #region Helpers

    private List<Person> SeedPersons()
    {
        var egypt = _fixture
            .Build<Country>()
            .With(c => c.Name, "Egypt")
            .Without(c => c.Persons)
            .Create();

        var usa = _fixture
            .Build<Country>()
            .With(c => c.Name, "USA")
            .Without(c => c.Persons)
            .Create();

        var persons = new List<Person>
        {
            _fixture
                .Build<Person>()
                .With(p => p.Email, "adham@gmail.com")
                .With(p => p.Name, "Adham")
                .With(p => p.Country, egypt)
                .With(p => p.CountryId, egypt.Id)
                .Create(),
            _fixture
                .Build<Person>()
                .With(p => p.Email, "ziad@gmail.com")
                .With(p => p.Name, "Ziad")
                .With(p => p.Country, egypt)
                .With(p => p.CountryId, egypt.Id)
                .Create(),
            _fixture
                .Build<Person>()
                .With(p => p.Email, "ramdan@gmail.com")
                .With(p => p.Name, "Ramdan")
                .With(p => p.Country, egypt)
                .With(p => p.CountryId, egypt.Id)
                .Create(),
            _fixture
                .Build<Person>()
                .With(p => p.Email, "ahmed@gmail.com")
                .With(p => p.Name, "Ahmed")
                .With(p => p.Country, usa)
                .With(p => p.CountryId, usa.Id)
                .Create(),
        };

        return persons;
    }

    #endregion

    #region  AddPerson

    [Fact]
    public async Task AddAsync_NullRequest_ThrowsArgumentNullException()
    {
        Func<Task> act = async () => await _sut.AddAsync(null);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task AddAsync_NullName_ThrowsArgumentException()
    {
        var request = _fixture.Build<PersonAddRequest>().With(p => p.Name, null as string).Create();

        Func<Task> act = async () => await _sut.AddAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task AddAsync_InvalidCountry_ThrowsArgumentException()
    {
        var request = _fixture
            .Build<PersonAddRequest>()
            .With(p => p.CountryId, Guid.Empty)
            .With(p => p.Email, "adham@gmail.com")
            .Create();

        _countriesServiceMock
            .Setup(x => x.GetByIdAsync(request.CountryId))
            .ReturnsAsync((CountryResponse?)null);

        Func<Task> act = async () => await _sut.AddAsync(request);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Invalid CountryId");
    }

    [Fact]
    public async Task AddAsync_ValidRequest_ShouldAddPerson()
    {
        var country = _fixture.Create<CountryResponse>();

        var request = _fixture
            .Build<PersonAddRequest>()
            .With(p => p.Email, "adham@gmail.com")
            .With(p => p.CountryId, country.Id)
            .Create();

        _countriesServiceMock.Setup(x => x.GetByIdAsync(request.CountryId)).ReturnsAsync(country);

        Person? capturedPerson = null;

        _personsRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Person>()))
            .Callback<Person>(p => capturedPerson = p)
            .ReturnsAsync((Person p) => p);

        var response = await _sut.AddAsync(request);

        response.Should().NotBeNull();
        response.Id.Should().NotBe(Guid.Empty);

        capturedPerson.Should().NotBeNull();
        capturedPerson!.Name.Should().Be(request.Name);
        capturedPerson.Email.Should().Be(request.Email);
        capturedPerson.CountryId.Should().Be(request.CountryId);
    }

    #endregion

    #region  UpdatePerson

    [Fact]
    public async Task UpdateAsync_NullRequest_ThrowsArgumentNullException()
    {
        Func<Task> act = async () => await _sut.UpdateAsync(null);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateAsync_InvalidId_ThrowsArgumentException()
    {
        var request = _fixture.Build<PersonUpdateRequest>().With(p => p.Id, Guid.Empty).Create();

        Func<Task> act = async () => await _sut.UpdateAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UpdateAsync_PersonNotFound_ThrowsArgumentException()
    {
        var request = _fixture.Create<PersonUpdateRequest>();

        _personsRepositoryMock.Setup(x => x.GetById(request.Id)).ReturnsAsync((Person?)null);

        Func<Task> act = async () => await _sut.UpdateAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UpdateAsync_InvalidCountry_ThrowsArgumentException()
    {
        var persons = SeedPersons();
        var existing = persons.First();

        _personsRepositoryMock.Setup(x => x.GetById(existing.Id)).ReturnsAsync(existing);

        _countriesServiceMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((CountryResponse?)null);

        var updateRequest = _fixture
            .Build<PersonUpdateRequest>()
            .With(p => p.Id, existing.Id)
            .With(p => p.Email, "updated@email.com")
            .Create();

        Func<Task> act = async () => await _sut.UpdateAsync(updateRequest);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesPersonSuccessfully()
    {
        var persons = SeedPersons();
        var existingPerson = persons.First();
        var country = _fixture.Create<CountryResponse>();

        _personsRepositoryMock
            .Setup(x => x.GetById(existingPerson.Id))
            .ReturnsAsync(existingPerson);

        _countriesServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(country);

        var updateRequest = _fixture
            .Build<PersonUpdateRequest>()
            .With(p => p.Id, existingPerson.Id)
            .With(p => p.CountryId, existingPerson?.CountryId)
            .With(p => p.Email, "updated@email.com")
            .With(p => p.ReceiveNewsLetters, false)
            .Create();

        var personAfterUpdate = updateRequest.ToPerson();
        var expected = personAfterUpdate.ToPersonResponse();
        _personsRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Person>()))
            .ReturnsAsync(personAfterUpdate);

        var personResponseAfterUpdate = await _sut.UpdateAsync(updateRequest);

        personResponseAfterUpdate.Should().BeEquivalentTo(expected);
        personResponseAfterUpdate.Email.Should().Be("updated@email.com");
        personResponseAfterUpdate.ReceiveNewsLetters.Should().BeFalse();

        _personsRepositoryMock.Verify(x => x.GetById(existingPerson!.Id), Times.Once);
        _personsRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Person>()), Times.Once);
        _countriesServiceMock.Verify(x => x.GetByIdAsync(updateRequest.CountryId), Times.Once);
    }

    #endregion

    #region DeletePerson


    [Fact]
    public async Task DeleteAsync_InvalidId_ThrowsArgumentException()
    {
        var act = async () => await _sut.DeleteAsync(Guid.Empty);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task DeleteAsync_PersonNotFound_ReturnFalse()
    {
        var id = Guid.NewGuid();

        _personsRepositoryMock.Setup(x => x.GetById(id)).ReturnsAsync((Person?)null);

        var result = await _sut.DeleteAsync(id);

        result.Should().BeFalse();

        _personsRepositoryMock.Verify(x => x.GetById(id), Times.Once);
        _personsRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_RepositoryReturnsZero_ReturnsFalse()
    {
        // Arrange
        var persons = SeedPersons();
        var toDelete = persons.First();

        _personsRepositoryMock.Setup(x => x.GetById(toDelete.Id)).ReturnsAsync(toDelete);

        _personsRepositoryMock.Setup(x => x.DeleteAsync(toDelete.Id)).ReturnsAsync(0);

        // Act
        var result = await _sut.DeleteAsync(toDelete.Id);

        // Assert
        result.Should().BeFalse();

        _personsRepositoryMock.Verify(x => x.GetById(toDelete.Id), Times.Once);
        _personsRepositoryMock.Verify(x => x.DeleteAsync(toDelete.Id), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ValidPersonId_ReturnTrue()
    {
        var persons = SeedPersons();
        var toDelete = persons.First();

        _personsRepositoryMock.Setup(x => x.GetById(toDelete.Id)).ReturnsAsync(toDelete);

        _personsRepositoryMock.Setup(x => x.DeleteAsync(toDelete.Id)).ReturnsAsync(1);

        var result = await _sut.DeleteAsync(toDelete.Id);

        result.Should().BeTrue();

        _personsRepositoryMock.Verify(x => x.GetById(toDelete.Id), Times.Once);
        _personsRepositoryMock.Verify(x => x.DeleteAsync(toDelete.Id), Times.Once);
    }

    #endregion
}
