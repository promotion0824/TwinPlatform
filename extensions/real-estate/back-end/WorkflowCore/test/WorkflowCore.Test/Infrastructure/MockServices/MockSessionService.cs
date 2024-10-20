using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowCore.Models;
using WorkflowCore.Services;

namespace WorkflowCore.Test.Infrastructure.MockServices;
public class MockSessionService : ISessionService
{
	public Guid? SourceId { get; private set; }

	public SourceType? SourceType { get; private set; }

    public MappedSiteSetting MappedSiteSetting => throw new NotImplementedException();

    public void SetMappedSiteSetting(MappedSiteSetting mappedSiteSetting)
    {
        throw new NotImplementedException();
    }

    public void SetSessionData(SourceType? sourceType = null, Guid? sourceId = null)
	{
		SourceId = sourceId;
		SourceType = sourceType;
	}

}

