using System;
using System.Threading;
using System.Threading.Tasks;
using DemoWorkerService.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DemoWorkerService
{
    public class TimedWorker : IHostedService, IDisposable
    {
        private readonly ILogger<TimedWorker> _logger;
        private readonly IMyService _myService;
        private Timer _timer;
        private static object _lock = new object();
        private int _counter = 0;

        public TimedWorker(ILogger<TimedWorker> logger, IMyService myService)
        {
            _logger = logger;
            _myService = myService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(3));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        private void DoWork(object state)
        {
            if (Monitor.TryEnter(_lock))
            {
                try
                {
                    _logger.LogDebug($"Running DoWork iteration {_counter}");
                    _myService.DoWorkAsync().Wait();
                    _logger.LogDebug($"DoWork {_counter} finished, will start iteration {_counter + 1}");
                }
                finally
                {
                    _counter++;
                    Monitor.Exit(_lock);
                }
            }
            else
            {
                _logger.LogDebug($"Iteration {_counter } of DoWork is still running");
            }
        }
    }
}