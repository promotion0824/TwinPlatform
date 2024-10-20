using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using System.Threading;

namespace Willow.TimedDispatch
{
    /// <summary>
    /// Notify Timed message events
    /// </summary>
    public class TimedDispatch : IHostedService, IAsyncDisposable
    {
        private int _executionCount;
        private Timer _timer;
        private readonly ITimedDispatchHandler _handler;
        private readonly TimeSpan _dueTime;
        private readonly TimeSpan _period;
        private readonly int _maxCount;

        public TimedDispatch(ITimedDispatchHandler handler, TimeSpan dueTime, TimeSpan period)
        {
            _handler = handler;
            _dueTime = dueTime;
            _period = period;
        }

        public TimedDispatch(ITimedDispatchHandler handler, TimeSpan dueTime, TimeSpan period, int maxCount)
            : this(handler, dueTime, period)
        {
            _maxCount = maxCount;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_timer == null)
            {
                _timer = new Timer(Dispatch, null, _dueTime, _period);
            }
            else
            {
                _timer.Change(_dueTime, _period);
            }

            return Task.CompletedTask;
        }

        private async void Dispatch(object state)
        {
            var count = Interlocked.Increment(ref _executionCount);

            await _handler?.OnTimedDispatch(state);

            if (_maxCount > 0 && count == _maxCount)
            {
                _timer?.Change(Timeout.Infinite, 0);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }
        public async ValueTask DisposeAsync()
        {
            await _timer.DisposeAsync();
        }
    }
}
