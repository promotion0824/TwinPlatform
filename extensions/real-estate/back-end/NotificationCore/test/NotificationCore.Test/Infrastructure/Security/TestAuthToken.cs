using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationCore.Test.Infrastructure.Security;

public class TestAuthToken
{
    public const string TestScheme = "TestScheme";

    public string Scope { get; set; }
    public string[] Roles { get; set; }
    public string Auth0UserId { get; set; }
    public Guid UserId { get; set; }
}
