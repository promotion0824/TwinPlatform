using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace NotificationCore.Test.Infrastructure;

public class TestContext
{
    public ITestOutputHelper Output { get; }
    public DatabaseFixture DatabaseFixture { get; }

    public TestContext(ITestOutputHelper output, DatabaseFixture databaseFixture)
    {
        Output = output;
        DatabaseFixture = databaseFixture;
    }
}
