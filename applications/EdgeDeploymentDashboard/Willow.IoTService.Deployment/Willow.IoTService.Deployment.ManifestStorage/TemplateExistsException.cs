namespace Willow.IoTService.Deployment.ManifestStorage;

public class TemplateExistsException(string? message, Exception? innerException) : ManifestStorageServiceException(message, innerException);
