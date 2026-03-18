using System;
using ContactsManager.Application.Features.Countries.DTOs;
using ContactsManager.Domain.Common.Results;
using ContactsManager.Domain.Common.Results.Abstractions;
using MediatR;

namespace ContactsManager.Application.Features.Countries.Commands.CreateCountry;

public sealed record CreateCountryCommand(string name) : IRequest<Result<CountryDto>>;
