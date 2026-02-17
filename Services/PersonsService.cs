using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Entities;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services.Helpers;

namespace Services;

public class PersonsService : IPersonsService
{
    private List<Person> _persons;
    private ICountriesService _countriesService;
    public PersonsService(ICountriesService countriesService)
    {
        _persons = [];
        _countriesService = countriesService;
    }
    public PersonResponse Add(PersonAddRequest? personAddRequest)
    {
        ArgumentNullException.ThrowIfNull(personAddRequest);

        ValidationHelper.ValidateModel(personAddRequest);

        var person = personAddRequest.ToPerson();
        person.Id = Guid.NewGuid();
        _persons.Add(person);

        return ConvertPersonToPersonResponse(person);
    }

    public bool Delete(Guid? personId)
    {
        if (personId is null)
            throw new ArgumentNullException(nameof(personId));

        if (personId == Guid.Empty)
            throw new ArgumentException("Id can't be empty");

        var person = _persons.FirstOrDefault(p => p.Id == personId);

        if (person is null)
            return false;

        return _persons.RemoveAll(p => p.Id == personId) > 0;
    }

    public List<PersonResponse> GetAll()
    {
        return _persons.Select(ConvertPersonToPersonResponse).ToList();
    }

    public PersonResponse? GetById(Guid? id)
    {
        if (id is null)
            return null;

        return _persons.FirstOrDefault(p => p.Id == id)?.ToPersonResponse();
    }

    public List<PersonResponse> GetFiltered(string searchBy, string? searchValue)
    {
        var allPersons = GetAll();
        var personsQuery = allPersons.AsEnumerable();

        if (string.IsNullOrWhiteSpace(searchValue))
            return personsQuery.ToList();

        personsQuery = searchBy switch
        {
            nameof(PersonResponse.Name) =>
                personsQuery.Where(p =>
                    !string.IsNullOrEmpty(p.Name) &&
                    p.Name.Contains(searchValue, StringComparison.OrdinalIgnoreCase)),

            nameof(PersonResponse.Email) =>
                personsQuery.Where(p =>
                    !string.IsNullOrEmpty(p.Email) &&
                    p.Email.Contains(searchValue, StringComparison.OrdinalIgnoreCase)),

            nameof(PersonResponse.DateOfBirth) =>
                personsQuery.Where(p =>
                    p.DateOfBirth.HasValue &&
                    p.DateOfBirth.Value.ToString("dd MMMM yyyy").Contains(searchValue, StringComparison.OrdinalIgnoreCase)),

            nameof(PersonResponse.Gender) =>
                personsQuery.Where(p =>
                    !string.IsNullOrEmpty(p.Gender) &&
                    p.Gender.Equals(searchValue, StringComparison.OrdinalIgnoreCase)),

            nameof(PersonResponse.Country) =>
                personsQuery.Where(p =>
                    !string.IsNullOrEmpty(p.Country) &&
                    p.Country.Contains(searchValue, StringComparison.OrdinalIgnoreCase)),


            _ => allPersons
        };

        return personsQuery
            .ToList();
    }

    public List<PersonResponse> GetSorted(List<PersonResponse> persons, string orderBy, SortOrder sortOrder)
    {
        if (persons is null || persons.Count == 0 || string.IsNullOrEmpty(orderBy))
            return new List<PersonResponse>();


        var property = typeof(PersonResponse).GetProperty(orderBy);
        if (property == null)
            return persons;

        object? GetKey(PersonResponse p)
        {
            var value = property.GetValue(p);

            if (value is string str)
                return str.ToLower();

            return value;
        }

        var sorted = sortOrder == SortOrder.Descending
            ? persons.OrderByDescending(GetKey)
            : persons.OrderBy(GetKey);

        return sorted.ToList();
    }

    public PersonResponse Update(PersonUpdateRequest? request)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidationHelper.ValidateModel(request);

        var personToUpdate = _persons.FirstOrDefault(p => p.Id == request.Id);

        if (personToUpdate is null)
            throw new ArgumentException($"Person with Id {request.Id} doesn't exist");

        personToUpdate.Name = request.Name;
        personToUpdate.Email = request.Email;
        personToUpdate.Address = request.Address;
        personToUpdate.DateOfBirth = request.DateOfBirth;
        personToUpdate.Gender = request.Gender.ToString();
        personToUpdate.CountryId = request.CountryId;
        personToUpdate.ReceiveNewsLetter = request.ReceiveNewsLetter;

        return personToUpdate.ToPersonResponse();
    }

    private PersonResponse ConvertPersonToPersonResponse(Person person)
    {
        var personResponse = person.ToPersonResponse();
        personResponse.Country = _countriesService.GetById(person.CountryId)?.Name;
        return personResponse;
    }

}

