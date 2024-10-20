namespace Willow.IoTService.Deployment.Dashboard.Application.Commands.DeleteModule;

using MediatR;

public class DeleteModuleHandler : IRequestHandler<DeleteModuleCommand, DeletedModuleResponse>
{
    public Task<DeletedModuleResponse> Handle(DeleteModuleCommand request, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }
}
