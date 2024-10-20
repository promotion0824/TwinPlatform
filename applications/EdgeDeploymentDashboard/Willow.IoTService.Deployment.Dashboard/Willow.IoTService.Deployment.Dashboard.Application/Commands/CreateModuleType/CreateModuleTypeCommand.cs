namespace Willow.IoTService.Deployment.Dashboard.Application.Commands.CreateModuleType;

using MediatR;
using Microsoft.AspNetCore.Http;
using Willow.IoTService.Deployment.Dashboard.Application.AuditLogging;

public record CreateModuleTypeCommand(string ModuleType, string Version, IFormFile Content) : IRequest<string>, IAuditLog;
