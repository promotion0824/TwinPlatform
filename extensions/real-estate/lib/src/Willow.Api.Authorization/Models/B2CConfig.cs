using System;
using System.Collections.Generic;
using System.Text;

namespace Willow.Api.Authorization
{
    public class B2CConfig
    {
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string TenantId { get; set; }
    }
}
