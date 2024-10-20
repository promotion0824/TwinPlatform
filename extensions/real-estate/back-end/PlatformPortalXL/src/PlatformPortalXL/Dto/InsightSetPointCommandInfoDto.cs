using System.Collections.Generic;

namespace PlatformPortalXL.Dto
{
    public class InsightSetPointCommandInfoDto
    {
        public AvailableSetPointCommandDto Available { get; set; }
        public IList<SetPointCommandDto> History { get; set; }
    }
}
