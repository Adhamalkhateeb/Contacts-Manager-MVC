using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Application.Features.Identity.Commands.Register;
using ContactsManager.Application.Features.Identity.Queries.Login;
using ContactsManager.Contracts.Requests.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContactsManager.Web.Controllers;

[AllowAnonymous]
public class AccountController(IMediator mediator, IIdentityService identityService) : MvcController
{
    private readonly IMediator _mediator = mediator;
    private readonly IIdentityService _identityService = identityService;

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var command = new RegisterCommand(
            request.UserName,
            request.Email,
            request.PhoneNumber,
            request.Password
        );

        var result = await _mediator.Send(command);

        return result.Match(
            success => RedirectToAction(nameof(Login)),
            errors => HandleError(errors, request)
        );
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequest request, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(request);

        var query = new LoginQuery(request.Email, request.Password, request.RememberMe);

        var result = await _mediator.Send(query);

        return result.Match(
            success =>
            {
                if (string.IsNullOrWhiteSpace(returnUrl) || !Url.IsLocalUrl(returnUrl))
                    return RedirectToAction(nameof(PersonsController.Index), "Persons");

                return LocalRedirect(returnUrl);
            },
            errors => HandleError(errors, request)
        );
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _identityService.LogoutAsync();
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public async Task<IActionResult> IsEmailAvailable(string email)
    {
        var isAvailable = await _identityService.IsEmailAvailableAsync(email);
        return Json(isAvailable ? true : "This email is already in use.");
    }

    [HttpGet]
    public async Task<IActionResult> IsUserNameAvailable(string userName)
    {
        var isAvailable = await _identityService.IsUserNameAvailableAsync(userName);
        return Json(isAvailable ? true : "This username is already taken.");
    }
}
