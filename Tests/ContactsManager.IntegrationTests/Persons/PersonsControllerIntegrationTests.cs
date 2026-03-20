using System.Net;
using System.Text.RegularExpressions;
using AutoFixture;
using ContactsManager.Domain.Countries;
using ContactsManager.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ContactsManager.IntegrationTests.Persons;

public class PersonsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly AppDbContext _context;
    private readonly IFixture _fixture;

    public PersonsControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _fixture = new Fixture();

        _client = factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false }
        );

        var scope = factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task Index_WhenNoData_ThenReturnsSuccess()
    {
        var response = await _client.GetAsync("/Persons/Index");
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task Create_Post_WhenPersonIsValid_ThenRedirectsToIndex()
    {
        var country = Country.Create(_fixture.Create<Guid>(), "Egypt").Value;
        _context.Countries.Add(country);
        await _context.SaveChangesAsync();

        var rawName = _fixture.Create<string>();
        var trimmedName = string.IsNullOrWhiteSpace(rawName) ? "New Person" : rawName.Trim();
        var name = trimmedName[..Math.Min(10, trimmedName.Length)];
        var emailLocalPart = new string(
            name.Where(char.IsLetterOrDigit).ToArray()
        ).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(emailLocalPart))
            emailLocalPart = "newperson";
        var email = $"{emailLocalPart}{_fixture.Create<int>()}@test.com";

        var requestData = new Dictionary<string, string>
        {
            { "Name", name },
            { "Email", email },
            { "Gender", "Male" },
            { "CountryId", country.Id.ToString() },
        };

        var createPageResponse = await _client.GetAsync("/Persons/Create");
        createPageResponse.IsSuccessStatusCode.Should().BeTrue();

        var createPageHtml = await createPageResponse.Content.ReadAsStringAsync();
        var tokenMatch = Regex.Match(
            createPageHtml,
            "name=\"__RequestVerificationToken\"\\s+type=\"hidden\"\\s+value=\"([^\"]+)\""
        );

        tokenMatch.Success.Should().BeTrue("antiforgery token should be rendered on create page");
        requestData["__RequestVerificationToken"] = tokenMatch.Groups[1].Value;

        var content = new FormUrlEncodedContent(requestData);

        var response = await _client.PostAsync("/Persons/Create", content);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Contain("/Persons/Index");

        var createdPerson = _context.Persons.FirstOrDefault(p => p.Email == email);
        createdPerson.Should().NotBeNull();
    }

    [Fact]
    public async Task Delete_Get_WhenIdIsUnknown_ThenReturnsNotFoundOrErrorRedirect()
    {
        var response = await _client.GetAsync($"/Persons/Delete/{_fixture.Create<Guid>()}");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Redirect);
    }
}
