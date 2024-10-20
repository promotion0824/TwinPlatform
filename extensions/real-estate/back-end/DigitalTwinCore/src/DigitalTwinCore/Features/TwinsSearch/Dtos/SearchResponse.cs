using DigitalTwinCore.Features.TwinsSearch.Models;

namespace DigitalTwinCore.Features.TwinsSearch.Dtos
{
    public class SearchResponse
    {
        public SearchTwin[] Twins { get; set; }
        public string QueryId { get; set; }
        public int NextPage { get; set; }
    }
}