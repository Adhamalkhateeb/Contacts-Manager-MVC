using ContactsManager.Application.Features.Countries.DTOs;
using ContactsManager.Domain.Common.Results;
using MediatR;

namespace ContactsManager.Application.Features.Countries.Commands.CreateCountry;

public record CreateCountryCommand(string Name) : IRequest<Result<CountryDto>>;
