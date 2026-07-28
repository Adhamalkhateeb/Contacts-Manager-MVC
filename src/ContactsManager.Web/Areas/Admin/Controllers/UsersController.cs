using System.Security.Claims;
using ContactsManager.Application.Features.Identity.Commands.AssignRole;
using ContactsManager.Application.Features.Identity.Commands.RemoveRole;
using ContactsManager.Application.Features.Identity.Queries.GetUsers;
using ContactsManager.Domain.Identity;
using ContactsManager.Web.Areas.Admin.Models;
using ContactsManager.Web.Controllers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContactsManager.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public sealed class UsersController(IMediator mediator) : MvcController
{
    private readonly IMediator _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUsersQuery(), cancellationToken);

        return result.Match(
            users =>
                View(
                    users
                        .Select(u => new AdminUserViewModel(
                            u.UserId,
                            u.UserName,
                            u.Email,
                            u.Roles.ToList(),
                            u.Roles.Contains(nameof(Role.Admin), StringComparer.Ordinal)
                        ))
                        .ToList()
                ),
            errors => HandleError(errors)
        );
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PromoteToAdmin(Guid userId)
    {
        var result = await _mediator.Send(new AssignRoleCommand(userId, Role.Admin));

        return result.Match(
            _ => RedirectToAction(nameof(Index)),
            errors =>
            {
                TempData["ResultErrorMessage"] = errors[0].Description;
                return RedirectToAction(nameof(Index));
            }
        );
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DemoteFromAdmin(Guid userId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (
            Guid.TryParse(currentUserId, out var parsedCurrentUserId)
            && parsedCurrentUserId == userId
        )
        {
            TempData["ResultErrorMessage"] = "You cannot remove your own admin role.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _mediator.Send(new RemoveRoleCommand(userId, Role.Admin));

        return result.Match(
            _ => RedirectToAction(nameof(Index)),
            errors =>
            {
                TempData["ResultErrorMessage"] = errors[0].Description;
                return RedirectToAction(nameof(Index));
            }
        );
    }
}
