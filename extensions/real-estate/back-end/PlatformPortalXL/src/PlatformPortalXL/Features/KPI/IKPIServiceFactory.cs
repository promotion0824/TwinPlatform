using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using PlatformPortalXL.Services;

namespace PlatformPortalXL.Features
{
    public interface IKPIServiceFactory
    {
        IKPIService GetService(Guid customerId);
    }
}
