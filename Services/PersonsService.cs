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
    public PersonsService(ICountriesService countriesService, bool initialize = true)
    {
        _persons = [];
        _countriesService = countriesService;

        if (initialize)
        {
            _persons.AddRange(new List<Person>
            {

               new Person {
                    Id =  Guid.Parse("9cf2c135-ee12-4d8c-a414-e1225ffeed43"),
                    Name= "Clywd Collen",
                    Gender= "Male",
                    DateOfBirth = DateTime.Parse("1990-02-03"),
                    Address= "957 Macpherson Hill",
                    Email= "ccollen0@nasa.gov",
                    ReceiveNewsLetters= true,
                    CountryId = Guid.Parse("2f18a149-4122-4b65-8987-69f04bd2b758")
                },
                new Person{
                    Id= Guid.Parse("c2c779ed-3243-49ea-9911-f7563d8b3fc8"),
                    Name= "Hilde Southcomb",
                    Gender= "Female",
                    DateOfBirth= DateTime.Parse("1993-05-07"),
                    Address= "1 Fulton Place",
                    Email= "hsouthcomb1@1und1.de",
                    ReceiveNewsLetters= true,
                    CountryId = Guid.Parse("2f18a149-4122-4b65-8987-69f04bd2b758")
                },
                new Person {
                    Id= Guid.Parse("47147e85-1df6-4a16-b83d-c955e466a14f"),
                    Name= "Shirlene Middle",
                    Gender= "Female",
                    DateOfBirth= DateTime.Parse("1997-07-02"),
                    Address= "2861 Jenna Court",
                    Email= "smiddle2@tmall.com",
                    ReceiveNewsLetters= false,
                    CountryId = Guid.Parse("2f18a149-4122-4b65-8987-69f04bd2b758")
                },
                new Person  {
                    Id= Guid.Parse("8e340358-29e4-4022-a787-dd01dec58279"),
                    Name= "Aldric Bysouth",
                    Gender= "Male",
                    DateOfBirth= DateTime.Parse("1994-12-11"),
                    Address= "194 Declaration Road",
                    Email= "abysouth3@bing.com",
                    ReceiveNewsLetters= true,
                    CountryId = Guid.Parse("dbbbcca0-f997-4720-b8fb-37ec2dc71f2e")
                },new Person{
                    Id= Guid.Parse("52dd758b-8049-49cb-821a-f1296dfa7876"),
                    Name= "Ruthanne Raycroft",
                    Gender= "Female",
                    DateOfBirth= DateTime.Parse("2000-08-27"),
                    Address= "3224 West Point",
                    Email= "rraycroft4@unc.edu",
                    ReceiveNewsLetters= true,
                    CountryId = Guid.Parse("dbbbcca0-f997-4720-b8fb-37ec2dc71f2e")
                },new Person{
                    Id= Guid.Parse("75f231e2-b4bb-4ef7-b01c-4380809858c3"),
                    Name= "Nikolia Jagiello",
                    Gender= "Female",
                    DateOfBirth= DateTime.Parse("1994-01-13"),
                    Address= "69 Evergreen Crossing",
                    Email= "njagiello5@netvibes.com",
                    ReceiveNewsLetters= true,
                    CountryId = Guid.Parse("dbbbcca0-f997-4720-b8fb-37ec2dc71f2e")
                },new Person {
                    Id= Guid.Parse("c3452d3f-7f70-4836-a735-ba913aa5f385"),
                    Name= "Gusti Samsin",
                    Gender= "Female",
                    DateOfBirth= DateTime.Parse("1994-09-29"),
                    Address= "7 Riverside Plaza",
                    Email= "gsamsin6@mapy.cz",
                    ReceiveNewsLetters= false,
                    CountryId = Guid.Parse("efd85b43-e69b-4c39-92e9-6843f692fe3a")
                },new Person {
                    Id= Guid.Parse("e81ff2cc-c712-4be5-8dc8-14196fdfb908"),
                    Name= "Rozina Pedlingham",
                    Gender= "Female",
                    DateOfBirth= DateTime.Parse("1997-09-20"),
                    Address= "70 Walton Road",
                    Email= "rpedlingham7@google.com.hk",
                    ReceiveNewsLetters= false,
                    CountryId = Guid.Parse("efd85b43-e69b-4c39-92e9-6843f692fe3a")
                },new Person {
                    Id= Guid.Parse("631e8feb-5bc7-4abb-937c-51d4cf5f48fc"),
                    Name= "Maddie Diment",
                    Gender= "Male",
                    DateOfBirth= DateTime.Parse("1992-09-29"),
                    Address= "5 5th Avenue",
                    Email= "mdiment8@scientificamerican.com",
                    ReceiveNewsLetters= true,
                    CountryId = Guid.Parse("efd85b43-e69b-4c39-92e9-6843f692fe3a")
                },new Person {
                    Id= Guid.Parse("62a8e82a-5ca1-4ddd-af04-2dd11b3d891f"),
                    Name= "Eadmund Truesdale",
                    Gender= "Male",
                    DateOfBirth= DateTime.Parse("1998-07-02"),
                    Address= "1 Dakota Plaza",
                    Email= "etruesdale9@nymag.com",
                    ReceiveNewsLetters= false,
                    CountryId = Guid.Parse("eeafd16e-a9b5-4aab-9036-eb80ca1e2146")
                }

            });
        }
    }

    public PersonResponse Add(PersonAddRequest? personAddRequest)
    {
        ArgumentNullException.ThrowIfNull(personAddRequest);

        ValidationHelper.ValidateModel(personAddRequest);

        var person = personAddRequest.ToPerson();
        person.Id = Guid.NewGuid();

        _persons.Add(person);

        return ConvertPersonToPersonResponse(person)!;
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
        return _persons.Select(ConvertPersonToPersonResponse).ToList()!;
    }

    public PersonResponse? GetById(Guid? id)
    {
        if (!id.HasValue)
            return null;

        var person = _persons.FirstOrDefault(p => p.Id == id.Value);
        return ConvertPersonToPersonResponse(person);
    }

    public List<PersonResponse> GetFiltered(string searchBy, string? searchValue)
    {
        var personsQuery = _persons.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(searchValue))
        {
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
                        p.DateOfBirth.Value.ToString("dd MMMM yyyy")
                            .Contains(searchValue, StringComparison.OrdinalIgnoreCase)),

                nameof(PersonResponse.Gender) =>
                    personsQuery.Where(p =>
                        !string.IsNullOrEmpty(p.Gender) &&
                        p.Gender.Equals(searchValue, StringComparison.OrdinalIgnoreCase)),

                nameof(PersonResponse.ReceiveNewsLetters) =>
                    personsQuery.Where(p =>
                        p.ReceiveNewsLetters.ToString()
                            .Equals(searchValue, StringComparison.OrdinalIgnoreCase)),

                _ => personsQuery
            };
        }

        return personsQuery
            .Select(p => ConvertPersonToPersonResponse(p)!)
            .ToList();
    }

    public List<PersonResponse> GetSorted(List<PersonResponse> persons, string orderBy, SortOrder sortOrder)
    {
        if (persons is null)
            return new List<PersonResponse>();

        if (string.IsNullOrEmpty(orderBy))
            return persons;


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

        var sorted = sortOrder == SortOrder.DESC
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
        personToUpdate.ReceiveNewsLetters = request.ReceiveNewsLetters;

        return ConvertPersonToPersonResponse(personToUpdate)!;
    }

    private PersonResponse? ConvertPersonToPersonResponse(Person? person)
    {
        if (person is null)
            return null;

        var response = person.ToPersonResponse();
        response.Country = _countriesService
            .GetById(person.CountryId)?.Name;

        return response;
    }

}

