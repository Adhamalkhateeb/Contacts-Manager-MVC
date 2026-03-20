using ContactsManager.Domain.Common.Results;
using MediatR;

namespace ContactsManager.Application.Features.Persons.Queries.GetPersonsAsExcel;

public sealed record class GetPersonsExcelQuery : IRequest<Result<byte[]>> { }
