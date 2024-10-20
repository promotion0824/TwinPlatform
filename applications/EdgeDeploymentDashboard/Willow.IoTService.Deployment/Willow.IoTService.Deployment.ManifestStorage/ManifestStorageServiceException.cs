namespace Willow.IoTService.Deployment.ManifestStorage;

public abstract class ManifestStorageServiceException : Exception
{
    protected ManifestStorageServiceException()
    {
    }

    protected ManifestStorageServiceException(string? message) : base(message)
    {
    }

    protected ManifestStorageServiceException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
