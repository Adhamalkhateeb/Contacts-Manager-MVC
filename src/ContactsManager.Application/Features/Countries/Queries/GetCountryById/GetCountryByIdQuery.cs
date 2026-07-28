using ContactsManager.Application.Features.Countries.DTOs;
using ContactsManager.Domain.Common.Results;
using MediatR;

namespace ContactsManager.Application.Features.Countries.Queries.GetCountryById;

public record GetCountryByIdQuery(Guid Id) : IRequest<Result<CountryDto>>;
