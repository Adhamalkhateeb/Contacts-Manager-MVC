using System;
using ContactsManager.Application.Features.Countries.DTOs;
using ContactsManager.Domain.Common.Results;
using MediatR;

namespace ContactsManager.Application.Features.Countries.Queries.GetCountryById;

public sealed record GetCountryByIdQuery(Guid countryId) : IRequest<Result<CountryDto>> { }
