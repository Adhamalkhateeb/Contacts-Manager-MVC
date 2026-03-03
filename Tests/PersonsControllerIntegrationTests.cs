using System;
using System.Net;
using Entities;
using Fizzler.Systems.HtmlAgilityPack;
using FluentAssertions;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml.FormulaParsing.FormulaExpressions.CompileResults;
using Xunit.Sdk;

namespace ContactsManager.Tests;

public class PersonsControllerIntegrationTests :
        IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly AppDbContext _context;
    public PersonsControllerIntegrationTests(CustomWebApplicationFactory factory)
    {

        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var scope = factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
    }

    #region Index


    [Fact]
    public async Task Index_DefaultRoute_ReturnsSuccessAndShowsPersons()
    {
        AddTestPersons(3);

        var response = await _client.GetAsync("/Persons/Index");
        response.IsSuccessStatusCode.Should().BeTrue();

        var html = await response.Content.ReadAsStringAsync();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var rows = doc.DocumentNode.QuerySelectorAll("table.persons tbody tr").ToList();
        rows.Should().HaveCount(3);
    }

    [Fact]
    public async Task Index_FilterByName_ReturnsMatchingPerson()
    {
        AddTestPersonsWithNames("Charlie", "Alpha", "Bravo");

        var response = await _client.GetAsync("/Persons/Index?searchBy=Name&searchValue=Alpha");
        response.IsSuccessStatusCode.Should().BeTrue();

        var html = await response.Content.ReadAsStringAsync();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var rows = doc.DocumentNode.QuerySelectorAll("table.persons tbody tr").ToList();
        rows.Should().HaveCount(1);
        rows.First().QuerySelector("td").InnerText.Trim().Should().Be("Alpha");
    }

    [Fact]
    public async Task Index_FilterEmptyOrNullSearchValue_ReturnsAllPersons()
    {
        AddTestPersons(3);

        var response = await _client.GetAsync("/Persons/Index?searchValue=");
        response.IsSuccessStatusCode.Should().BeTrue();

        var html = await response.Content.ReadAsStringAsync();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var rows = doc.DocumentNode.QuerySelectorAll("table.persons tbody tr").ToList();
        rows.Should().HaveCount(3);
    }

    [Fact]
    public async Task Index_SortByNameASC_ReturnsSortedAscending()
    {
        AddTestPersonsWithNames("Charlie", "Alpha", "Bravo");

        var response = await _client.GetAsync("/Persons/Index?orderBy=Name&sortOrder=ASC");
        response.IsSuccessStatusCode.Should().BeTrue();

        var html = await response.Content.ReadAsStringAsync();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var names = doc.DocumentNode.QuerySelectorAll("table.persons tbody tr td:first-child")
            .Select(n => n.InnerText.Trim());
        names.Should().Equal("Alpha", "Bravo", "Charlie");
    }

    [Fact]
    public async Task Index_SortByNameDESC_ReturnsSortedDescending()
    {
        AddTestPersonsWithNames("Charlie", "Alpha", "Bravo");

        var response = await _client.GetAsync("/Persons/Index?orderBy=Name&sortOrder=DESC");
        response.IsSuccessStatusCode.Should().BeTrue();

        var html = await response.Content.ReadAsStringAsync();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var names = doc.DocumentNode.QuerySelectorAll("table.persons tbody tr td:first-child")
            .Select(n => n.InnerText.Trim());
        names.Should().Equal("Charlie", "Bravo", "Alpha");
    }

    [Fact]
    public async Task Index_EmptyDb_ShowsEmptyTable()
    {
        var response = await _client.GetAsync("/Persons/Index");
        response.IsSuccessStatusCode.Should().BeTrue();

        var html = await response.Content.ReadAsStringAsync();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        doc.DocumentNode.QuerySelectorAll("table.persons tbody tr").Should().BeEmpty();
        doc.DocumentNode.QuerySelectorAll("table.persons thead th").Should().NotBeEmpty();
    }

    [Fact]
    public async Task Index_XSS_AttemptIsEncoded()
    {
        var country = new Country { Id = Guid.NewGuid(), Name = "Egypt" };
        _context.Countries.Add(country);
        _context.Persons.Add(new Person
        {
            Id = Guid.NewGuid(),
            Name = "<script>alert('XSS')</script>",
            Email = "safe@test.com",
            CountryId = country.Id,
            Gender = "Male"
        });
        await _context.SaveChangesAsync();

        var response = await _client.GetAsync("/?searchValue=<script>");
        response.IsSuccessStatusCode.Should().BeTrue();

        var html = await response.Content.ReadAsStringAsync();

        var possibleEncodings = new[]
        {
            "&lt;script&gt;alert('XSS')&lt;/script&gt;",
            "&lt;script&gt;alert(&#x27;XSS&#x27;)&lt;/script&gt;",
            "&lt;script&gt;alert(&#39;XSS&#39;)&lt;/script&gt;"
        };

        html.Should().ContainAny(possibleEncodings);
    }

    #endregion

    #region Edit

    [Fact]
    public async Task Edit_Get_NonExistingId_RedirectsToIndex()
    {
        var id = Guid.NewGuid();

        var response = await _client.GetAsync($"/Persons/Edit/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Contain("/Persons/Index");
    }

    [Fact]
    public async Task Edit_Get_ReturnsViewWithPersonData()
    {
        var country = new Country { Id = Guid.NewGuid(), Name = "Egypt" };
        _context.Countries.Add(country);

        var person = new Person
        {
            Id = Guid.NewGuid(),
            Name = "Sam",
            Email = "sam@test.com",
            CountryId = country.Id,
            Gender = "Male",
            Address = "Addr",
            DateOfBirth = new DateTime(1990, 1, 1)
        };
        _context.Persons.Add(person);
        await _context.SaveChangesAsync();

        var response = await _client.GetAsync($"/Persons/Edit/{person.Id}");
        response.IsSuccessStatusCode.Should().BeTrue();

        var html = await response.Content.ReadAsStringAsync();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        doc.DocumentNode.QuerySelector("input[name='Id']")?.GetAttributeValue("value", "").Should().Be(person.Id.ToString());
        doc.DocumentNode.QuerySelector("input[name='Name']")?.GetAttributeValue("value", "").Should().Be("Sam");
        doc.DocumentNode.QuerySelector("input[name='Email']")?.GetAttributeValue("value", "").Should().Be("sam@test.com");
        doc.DocumentNode.QuerySelector("textarea[name='Address']")?.InnerText.Trim().Should().Be("Addr");
    }

    [Fact]
    public async Task Edit_Post_Valid_UpdatesPersonAndRedirects()
    {
        var country = new Country { Id = Guid.NewGuid(), Name = "Egypt" };
        _context.Countries.Add(country);

        var person = new Person
        {
            Id = Guid.NewGuid(),
            Name = "Old",
            Email = "old@test.com",
            CountryId = country.Id,
            Gender = "Male"
        };
        _context.Persons.Add(person);
        await _context.SaveChangesAsync();

        var requestData = new Dictionary<string, string>
        {
            { "Id", person.Id.ToString() },
            { "Name", "NewName" },
            { "Email", "new@test.com" },
            { "Gender", "Male" },
            { "CountryId", country.Id.ToString() },
            { "Address", "New Addr" },
            { "ReceiveNewsLetters", "true" }
        };

        var content = new FormUrlEncodedContent(requestData);

        var response = await _client.PostAsync($"/Persons/Edit/{person.Id}", content);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);

        var updated = _context.Persons.AsNoTracking().FirstOrDefault(p => p.Id == person.Id);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("NewName");
        updated.Email.Should().Be("new@test.com");
    }

    [Fact]
    public async Task Edit_Post_Invalid_ReturnsViewWithErrors()
    {
        var country = new Country { Id = Guid.NewGuid(), Name = "Egypt" };
        _context.Countries.Add(country);
        await _context.SaveChangesAsync();

        var requestData = new Dictionary<string, string>
        {
            { "Id", Guid.NewGuid().ToString() },
            { "Email", "bad@test.com" },
            { "Gender", "Male" },
            { "CountryId", country.Id.ToString() }
        };

        var content = new FormUrlEncodedContent(requestData);
        var response = await _client.PostAsync($"/Persons/Edit/{requestData["Id"]}", content);

        response.IsSuccessStatusCode.Should().BeTrue();
        var html = await response.Content.ReadAsStringAsync();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        doc.DocumentNode.QuerySelectorAll(".text-red").Should().HaveCountGreaterThan(0);
        doc.DocumentNode.QuerySelector("select[name='CountryId']")
            .QuerySelectorAll("option")
            .Select(o => o.GetAttributeValue("value", "").Trim())
            .Should().Contain(country.Id.ToString());
    }

    #endregion

    #region Delete

    [Fact]
    public async Task Delete_Get_NonExistingId_RedirectsToIndex()
    {
        var id = Guid.NewGuid();

        var response = await _client.GetAsync($"/Persons/Delete/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Contain("/Persons/Index");
    }

    [Fact]
    public async Task Delete_Get_ReturnsViewWithPerson()
    {
        var country = new Country { Id = Guid.NewGuid(), Name = "Egypt" };
        _context.Countries.Add(country);

        var person = new Person
        {
            Id = Guid.NewGuid(),
            Name = "ToDelete",
            Email = "td@test.com",
            CountryId = country.Id,
            Gender = "Male"
        };
        _context.Persons.Add(person);
        await _context.SaveChangesAsync();

        var response = await _client.GetAsync($"/Persons/Delete/{person.Id}");
        response.IsSuccessStatusCode.Should().BeTrue();

        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("ToDelete");
        html.Should().Contain("td@test.com");
    }

    [Fact(Skip = "InMemory provider does not support ExecuteDeleteAsync used by repository.DeleteAsync; skip integration delete POST")]
    public async Task Delete_Post_Valid_DeletesPersonAndRedirects()
    {
        var country = new Country { Id = Guid.NewGuid(), Name = "Egypt" };
        _context.Countries.Add(country);

        var person = new Person
        {
            Id = Guid.NewGuid(),
            Name = "ToDelete2",
            Email = "td2@test.com",
            CountryId = country.Id,
            Gender = "Male"
        };
        _context.Persons.Add(person);
        await _context.SaveChangesAsync();

        var requestData = new Dictionary<string, string>
        {
            { "Id", person.Id.ToString() },
            { "Name", person.Name },
            { "Email", person.Email }
        };

        var content = new FormUrlEncodedContent(requestData);
        var response = await _client.PostAsync($"/Persons/Delete/{person.Id}", content);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);

        var exists = _context.Persons.Any(p => p.Id == person.Id);
        exists.Should().BeFalse();
    }

    #endregion

    #region Downloads

    [Fact]
    public async Task PersonsCSV_ReturnsCsvFile()
    {
        AddTestPersons(1);

        var response = await _client.GetAsync("/Persons/PersonsCSV");
        response.IsSuccessStatusCode.Should().BeTrue();
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/csv");
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task PersonsExcel_ReturnsExcelFile()
    {
        AddTestPersons(1);

        var response = await _client.GetAsync("/Persons/PersonsExcel");
        response.IsSuccessStatusCode.Should().BeTrue();
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        var cd = response.Content.Headers.ContentDisposition?.FileName ?? string.Empty;
        cd.Should().Contain("Persons_");
    }

    [Fact(Skip = "Rotativa not configured in test environment")]
    public async Task PersonsPDF_ReturnsSuccess()
    {
        AddTestPersons(1);

        var response = await _client.GetAsync("/Persons/PersonsPdf");
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    #endregion

    #region  Create
    [Fact]
    public async Task Create_Get_ReturnsViewWithCountries()
    {
        // Arrange
        var country = new Country { Id = Guid.NewGuid(), Name = "Egypt" };
        _context.Countries.Add(country);
        _context.SaveChanges();

        // Act
        var response = await _client.GetAsync("/Persons/Create");
        response.IsSuccessStatusCode.Should().BeTrue();

        var html = await response.Content.ReadAsStringAsync();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Assert
        var selectElement = doc.DocumentNode.QuerySelector("select[name='CountryId']");
        selectElement.Should().NotBeNull();

        var options = selectElement.QuerySelectorAll("option").ToList();
        options.Select(o => o.GetAttributeValue("value", "").Trim()).Should().Contain(country.Id.ToString());
    }

    [Fact]
    public async Task Create_Post_ValidRequest_AddsPersonAndRedirects()
    {
        // Arrange
        var country = new Country { Id = Guid.NewGuid(), Name = "Egypt" };
        _context.Countries.Add(country);
        _context.SaveChanges();

        var requestData = new Dictionary<string, string>
        {
            { "Name", "Ahmed" },
            { "Email", "ahmed@test.com" },
            { "Gender", "Male" },
            { "CountryId", country.Id.ToString() },
            { "Address", "123 Street" },
            { "DateOfBirth", DateTime.UtcNow.AddYears(-30).ToString("yyyy-MM-dd") },
            { "ReceiveNewsLetters", "true" }
        };

        var content = new FormUrlEncodedContent(requestData);

        var response = await _client.PostAsync("/Persons/Create", content);

        var responseHtml = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Redirect, "Response HTML: " + responseHtml);
        response.Headers.Location?.ToString().Should().Contain("/Persons/Index");

        var person = _context.Persons.FirstOrDefault(p => p.Email == "ahmed@test.com");
        person.Should().NotBeNull();
        person.Name.Should().Be("Ahmed");
        person.CountryId.Should().Be(country.Id);

    }

    [Fact]
    public async Task Create_Post_InvalidRequest_ReturnsViewWithErrors()
    {

        var country = new Country { Id = Guid.NewGuid(), Name = "Egypt" };
        _context.Countries.Add(country);
        _context.SaveChanges();

        var requestData = new Dictionary<string, string>
            {
                { "Email", "invalid@test.com" },
                { "Gender", "Male" },
                { "CountryId", country.Id.ToString() }
            };

        var content = new FormUrlEncodedContent(requestData);
        var response = await _client.PostAsync("/Persons/Create", content);

        response.IsSuccessStatusCode.Should().BeTrue();

        var html = await response.Content.ReadAsStringAsync();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var errors = doc.DocumentNode.QuerySelectorAll(".text-red").ToList();
        errors.Should().HaveCountGreaterThan(0);

        var selectElement = doc.DocumentNode.QuerySelector("select[name='CountryId']");
        selectElement.Should().NotBeNull();
        selectElement.QuerySelectorAll("option")
            .Select(o => o.GetAttributeValue("value", "").Trim())
            .Should().Contain(country.Id.ToString());

    }

    #endregion

    #region Helper Methods

    private void AddTestPersons(int count)
    {
        var country = new Country { Id = Guid.NewGuid(), Name = "Egypt" };
        _context.Countries.Add(country);

        var persons = new List<Person>();
        for (int i = 1; i <= count; i++)
        {
            persons.Add(new Person
            {
                Id = Guid.NewGuid(),
                Name = $"Person {i}",
                Email = $"person{i}@test.com",
                CountryId = country.Id,
                Gender = i % 2 == 0 ? "Male" : "Female"
            });
        }

        _context.Persons.AddRange(persons);
        _context.SaveChanges();
    }

    private void AddTestPersonsWithNames(params string[] names)
    {
        var country = new Country { Id = Guid.NewGuid(), Name = "Egypt" };
        _context.Countries.Add(country);

        foreach (var name in names)
        {
            _context.Persons.Add(new Person
            {
                Id = Guid.NewGuid(),
                Name = name,
                Email = $"{name.ToLower()}@test.com",
                CountryId = country.Id,
                Gender = "Male"
            });
        }

        _context.SaveChanges();
    }

    #endregion
}
