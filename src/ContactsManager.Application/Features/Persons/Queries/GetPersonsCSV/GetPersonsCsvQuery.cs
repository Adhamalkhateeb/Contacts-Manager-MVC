using ContactsManager.Domain.Common.Results;
using MediatR;

namespace ContactsManager.Application.Features.Persons.GetPersonsCSV;

public sealed record GetPersonsCsvQuery() : IRequest<Result<byte[]>>;
