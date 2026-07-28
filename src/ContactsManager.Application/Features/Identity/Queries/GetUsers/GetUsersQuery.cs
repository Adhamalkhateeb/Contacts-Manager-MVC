using ContactsManager.Application.Features.Identity.Dtos;
using ContactsManager.Domain.Common.Results;
using MediatR;

namespace ContactsManager.Application.Features.Identity.Queries.GetUsers;

public sealed record GetUsersQuery() : IRequest<Result<List<AppUserDto>>>;
