using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace DigitalTwinCore.Infrastructure
{
    public sealed class TimedLock 
    {
        // Warning: Do not implement IDispose for this outer class
        //   or a SemaphoreReleasedException will occur --
        //  the lock must only be released when the inner LockReleaser instance goes out of scope.

        private readonly SemaphoreSlim toLock;

        public TimedLock()
        {
            toLock = new SemaphoreSlim(1, 1);
        }

        public async Task<LockReleaser> Lock()
        {
            return await Lock(Timeout.InfiniteTimeSpan);
        }

        public async Task<LockReleaser> Lock(TimeSpan timeout)
        {
            if (await toLock.WaitAsync(timeout))
            { 
                return new LockReleaser(toLock);
            }
            throw new TimeoutException();
        }

        public struct LockReleaser : IDisposable, IEquatable<LockReleaser>
        {
            private readonly SemaphoreSlim toRelease;

            public LockReleaser(SemaphoreSlim toRelease)
            {
                this.toRelease = toRelease;
            }

            public void Dispose()
            {
                toRelease.Release();
            }

            public bool Equals([AllowNull] LockReleaser other)
            {
                return toRelease.Equals(other.toRelease);
            }
        }
    }
}
