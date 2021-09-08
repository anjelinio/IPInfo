using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IPInfo.Batch
{
    public class BatchUpdateService : IHostedService, IDisposable
    {
        private readonly ILogger<BatchUpdateService> _logger;
        private Timer _timer;

        protected IServiceScopeFactory _scopeFactory;

        protected BatchUpdateConfiguration _configuration;

        public BatchUpdateService(ILogger<BatchUpdateService> logger, BatchUpdateConfiguration configuration, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        } 

        protected virtual void DoWork(object state)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _batchJobs = scope.ServiceProvider.GetRequiredService<IBatchJobRepository>() ?? throw new ArgumentNullException(nameof(IBatchJobRepository));
                var _infoProvider = scope.ServiceProvider.GetRequiredService<IIPInfoProvider>() ?? throw new ArgumentNullException(nameof(IIPInfoProvider));

                var requestsCount = 0;

                while (requestsCount < _configuration.MaxItems)
                {
                    var operation = _batchJobs.GetNext();
                    if (null == operation)
                        break;

                    var requests = operation.Requests.Where(r => !BatchUpdateRequest.TerminalStatuces.Contains(r.Status))
                                                     .Take(_configuration.MaxItems - requestsCount)
                                                     .ToArray();

                    _logger.LogInformation($"Next available job - {operation.Id} Requests: {requests.Length}");

                    foreach (var request in requests)
                    {
                        request.Status = BatchRequestStatus.Started;

                        _logger.LogInformation($"Job {operation.Id} Updating: {request.Ip} - Count: {requestsCount}");

                        var update = _infoProvider.GetDetails(request.Ip, true).GetAwaiter().GetResult();
                        request.Status = update.IsSuccess ? BatchRequestStatus.Completed : BatchRequestStatus.Faulted;

                        requestsCount++;
                    }

                    SetNewStatus(operation);
                }
            }
        }

        protected virtual void SetNewStatus(BatchUpdateRequest operation)
        {
            var anyFaulted = operation.Requests.Any(r => r.Status == BatchRequestStatus.Faulted);
            var anyWaiting = operation.Requests.Any(r => r.Status == BatchRequestStatus.Created || r.Status == BatchRequestStatus.Started);
            
            if (anyFaulted)
                operation.Status = BatchRequestStatus.Faulted;
            else
                operation.Status = anyWaiting ? BatchRequestStatus.Started 
                                              : BatchRequestStatus.Completed;
        }

        #region plumbing

        private static SemaphoreSlim globalLock = new SemaphoreSlim(1);

        protected virtual void Tick(object state)
        {
            globalLock.Wait();

            try
            {
                DoWork(state);
            }
            catch (Exception e)
            {
                _logger.LogError($"ERROR - {e.Message} : \n {e.StackTrace}");
            }
            finally
            {
                globalLock.Release();
            }
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BatchUpdate Hosted Service running.");

            _timer = new Timer(Tick, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(_configuration.Tick));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        #endregion
    }
}
