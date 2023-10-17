using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Supplier.Business;


namespace Supplier
{
    public class Supplier : IHostedService, IDisposable
    {
        private readonly ILogger<Supplier> _logger;

        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly SupplierBusiness _business;
        
        private CancellationTokenSource _tokenSource;
        private Task _mainTask;

        public Supplier(IHostApplicationLifetime hostApplicationLifetime, SupplierBusiness business, ILogger<Supplier> logger)
        {
            _hostApplicationLifetime    = hostApplicationLifetime;
            _business                   = business;

            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _hostApplicationLifetime.ApplicationStopping.Register(Abort);
            _tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _mainTask = RunBusinessAsync();

            return Task.CompletedTask;
        }

        private async Task<TaskStatus> RunBusinessAsync()
        {
            try
            {
                return await _business.Run(_tokenSource.Token);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[RunBusinessAsync] Something went wrong running the business. {ex.Message} :: {ex.StackTrace}.");
                return TaskStatus.Faulted;
            }
            finally
            {
                _hostApplicationLifetime.StopApplication();
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_mainTask != null && (!_mainTask.IsCompleted || cancellationToken.IsCancellationRequested))
                _tokenSource?.Cancel();

            _business.FinishProcessing();
            await _mainTask.ConfigureAwait(false);
        }

        public void Dispose()
        {
            _logger.LogInformation("[Dispose] Disposing.");
        }

        private void Abort()
        {
            _logger.LogInformation("[Abort] Cancelling execution for Supplier.");
            _tokenSource.Cancel();
        }

    }
}
