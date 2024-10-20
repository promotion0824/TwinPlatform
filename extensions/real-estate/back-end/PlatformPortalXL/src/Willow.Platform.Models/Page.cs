using System.Collections.Generic;

namespace Willow.Platform.Models
{
    public class Page<T>
    {
        public IEnumerable<T> Content { get; set; }
        public string ContinuationToken { get; set; }
    }
}
