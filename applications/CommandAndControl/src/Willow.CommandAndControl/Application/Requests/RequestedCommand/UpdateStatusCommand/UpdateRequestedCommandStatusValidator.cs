namespace Willow.CommandAndControl.Application.Requests.RequestedCommand.UpdateStatusCommand;

using Willow.CommandAndControl.Application.Helpers;

internal class UpdateRequestedCommandStatusValidator : AbstractValidator<UpdateRequestedCommandStatusDto>
{
    public UpdateRequestedCommandStatusValidator()
    {
        RuleFor(x => x.Action)
         .NotEmpty()
         .NotEqual(string.Empty);

        RuleFor(x => x.Action)
            .Must((status) =>
            {
                return Enum.TryParse(status, true, out RequestedCommandAction requestedAction);
            }).WithMessage("Not a valid Action");

        //RuleFor(x => x.Action)
        //.MustAsync(async (action, cancellationToken) =>
        //{
        //    var id = (string)actionContextAccessor.ActionContext.RouteData.Values.GetValueOrDefault("id");
        //    var requestedCommand = await commandManager.GetRequestedCommandByIdAsync(id, cancellationToken);
        //    if (requestedCommand == null)
        //    {
        //        return false;
        //    }
        //    return !(Enum.Parse<RequestedCommandAction>(action, true) == RequestedCommandAction.Approve &&
        //        requestedCommand.Status.ToLower().Equals(RequestedCommandAction.Reject.ToString(), StringComparison.InvariantCulture));
        //}).WithMessage("Given RequestedCommand Id either does not exist or is already rejected");

        //RuleFor(x => new { x.Id, x.Action })
        //    .MustAsync(async (request, cancellationToken) =>
        //    {
        //        var requestedCommand = await commandManager.GetRequestedCommandByIdAsync(request.Id.ToString(), cancellationToken);
        //        if (requestedCommand == null)
        //        {
        //            return false;
        //        }
        //        return !(Enum.Parse<RequestedCommandAction>(request.Action, true) == RequestedCommandAction.Reject
        //        && requestedCommand.Status.ToLower().Equals(RequestedCommandAction.Approve.ToString(), StringComparison.InvariantCulture));

        //    })
        //    .WithMessage("Command request with given Id has already been approved");
    }
}
