namespace Willow.Mediator;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// A base class for MediatR controllers.
/// </summary>
public abstract class MediatorController : ControllerBase
{
    private IMediator? mediator;

    /// <summary>
    /// Gets the mediator.
    /// </summary>
    protected IMediator Mediator => mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();
}
