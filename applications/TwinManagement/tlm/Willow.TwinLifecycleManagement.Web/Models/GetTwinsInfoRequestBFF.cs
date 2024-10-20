using Willow.Batch;
using Willow.Model.Requests;

namespace Willow.TwinLifecycleManagement.Web.Models
{
    /// <summary>
    /// Request used in GetTwin Controller to fetch twins.
    ///</summary>
    public class GetTwinsInfoRequestBFF : GetTwinsInfoRequest
    {

        /// <summary>
        /// Gets or sets the filter specifications from MUI data grid.
        /// </summary>
        public TwinFilterSpecificationDto[] FilterSpecifications { get; set; } = Array.Empty<TwinFilterSpecificationDto>();
    }

    /// <summary>
    /// This class is used to handle nswag conflicting schema IDs exception, Willow.Pagination.FilterSpecificationDto and
    /// Willow.AzureDigitalTwins.SDK.Client.FilterSpecificationDto.
    /// </summary>
    public class TwinFilterSpecificationDto : FilterSpecificationDto
    {
    }
}
