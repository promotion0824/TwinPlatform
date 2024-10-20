namespace Willow.IoTService.Deployment.Dashboard.Application.AuditLogging;

using MediatR;
using Microsoft.Extensions.Logging;
using Willow.IoTService.Deployment.DataAccess.PortService;

public class AuditLogPipeline<TRequest, TResponse>(IAuditLogger<TRequest> auditLogger, IUserInfoService userInfoService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IAuditLog
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();
        auditLogger.LogInformation(userInfoService.GetUserId(), typeof(TRequest).Name, request);
        return response;
    }
}
