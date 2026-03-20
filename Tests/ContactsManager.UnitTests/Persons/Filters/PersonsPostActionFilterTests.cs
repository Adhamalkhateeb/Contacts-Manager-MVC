using AutoFixture;
using ContactsManager.Application.Features.Countries.DTOs;
using ContactsManager.Application.Features.Countries.Queries.GetCountries;
using ContactsManager.Contracts.Requests.Person;
using ContactsManager.Domain.Common.Results;
using ContactsManager.Web.Controllers;
using ContactsManager.Web.Filters.ActionFilters;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;

namespace ContactsManager.UnitTests.Persons.Filters;

public class PersonsPostActionFilterTests
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly PersonsPostActionFilter _sut;
    private readonly PersonsController _controller;
    private readonly IFixture _fixture;

    public PersonsPostActionFilterTests()
    {
        _fixture = new Fixture();

        var loggerMock = new Mock<ILogger<PersonsPostActionFilter>>();
        var controllerLogger = new Mock<ILogger<PersonsController>>();

        _sut = new PersonsPostActionFilter(_mediatorMock.Object, loggerMock.Object);
        _controller = new PersonsController(_mediatorMock.Object, controllerLogger.Object);
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenModelStateInvalidBeforeAction_ShortCircuitsAndPopulatesCountries()
    {
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetCountriesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                _fixture.Build<CountryDto>().With(x => x.Name, "Egypt").CreateMany(1).ToList()
            );

        var request = _fixture.Create<CreatePersonRequest>();
        var context = CreateExecutingContext(
            new Dictionary<string, object?> { ["request"] = request }
        );

        _controller.ModelState.AddModelError("Name", "Name is required");

        await _sut.OnActionExecutionAsync(
            context,
            () =>
                Task.FromResult(
                    new ActionExecutedContext(context, new List<IFilterMetadata>(), _controller)
                )
        );

        context.Result.Should().BeOfType<ViewResult>();
        var countries = _controller.ViewBag.Countries as List<SelectListItem>;
        countries.Should().NotBeNull();
        countries!.Should().HaveCount(1);
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenModelStateBecomesInvalidAfterAction_AndResultIsView_PopulatesCountries()
    {
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetCountriesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                _fixture.Build<CountryDto>().With(x => x.Name, "USA").CreateMany(1).ToList()
            );

        var context = CreateExecutingContext(new Dictionary<string, object?>());

        await _sut.OnActionExecutionAsync(
            context,
            () =>
            {
                _controller.ModelState.AddModelError("Email", "Email is invalid");

                return Task.FromResult(
                    new ActionExecutedContext(context, new List<IFilterMetadata>(), _controller)
                    {
                        Result = new ViewResult(),
                    }
                );
            }
        );

        var countries = _controller.ViewBag.Countries as List<SelectListItem>;
        countries.Should().NotBeNull();
        countries!.Should().HaveCount(1);
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenModelStateBecomesInvalidAfterAction_ButResultIsRedirect_DoesNotPopulateCountries()
    {
        var context = CreateExecutingContext(new Dictionary<string, object?>());

        await _sut.OnActionExecutionAsync(
            context,
            () =>
            {
                _controller.ModelState.AddModelError("Email", "Email is invalid");

                return Task.FromResult(
                    new ActionExecutedContext(context, new List<IFilterMetadata>(), _controller)
                    {
                        Result = new RedirectToActionResult("Index", "Persons", null),
                    }
                );
            }
        );

        (_controller.ViewBag.Countries as List<SelectListItem>).Should().BeNull();
        _mediatorMock.Verify(
            x => x.Send(It.IsAny<GetCountriesQuery>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    private ActionExecutingContext CreateExecutingContext(
        IDictionary<string, object?> actionArguments
    )
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ControllerActionDescriptor(),
            new ModelStateDictionary()
        );

        _controller.ControllerContext = new ControllerContext(actionContext);

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            actionArguments,
            _controller
        );
    }
}
