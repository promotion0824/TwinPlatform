namespace PlatformPortalXL.Models;

public class ActiveInsightCountByModelIdDto
{
    /// <summary>
    /// The twin model id
    /// </summary>
    public string ModelId { get; set; }
    /// <summary>
    /// Number of active insights for the model
    /// </summary>
    public int Count { get; set; }
}
