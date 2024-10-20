using Alba;
using LightBDD.XUnit2;
using SiteCore.ServiceTests.Utils;
using System;

namespace SiteCore.ServiceTests.Server
{
    public abstract class BaseServerFixture : FeatureFixture, IDisposable
    {
        protected TestServerFixture Fixture { get; }
        protected IAlbaHost Host { get; }
        private bool disposedValue;

        public BaseServerFixture()
        {
            Fixture = new TestServerFixture(RandomPort.GetRandomUnUsedPort());
            Host = Fixture.albaHost;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Fixture.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
