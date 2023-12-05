using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Supplier.Services;
using System.Diagnostics;
using System.Text.Json;
using Types;

namespace Supplier.Business
{
    public class SupplierBusiness
    {
        #region Default Variables

        private CancellationToken _token;
        private readonly EventWaitHandle _sync;

        #endregion

        #region Services

        private readonly RabbitMQMessageSender _rabbitMQMessageSender;
        private readonly ILogger<SupplierBusiness> _logger;
        private readonly Repository _repository;

        #endregion

        #region Auxiliary Variables

        private readonly string _rabbitQueueName;

        #endregion

        public SupplierBusiness(RabbitMQMessageSender rabbitMQMessageSender, 
                                IConfiguration configuration, 
                                ILogger<SupplierBusiness> logger,
                                Repository repository)
        {
            _sync = new(false, EventResetMode.ManualReset);
            _rabbitMQMessageSender = rabbitMQMessageSender;

            _rabbitQueueName = configuration["RABBITMQ_QUEUENAME"];

            _logger = logger;
            _repository = repository;
        }


        public async Task<TaskStatus> Run(CancellationToken cancellationToken)
        {
            _token = cancellationToken;
            return await Task.Factory.StartNew(() => SendMessages().Result);
        }

        public async Task<TaskStatus> SendMessages()
        {
            while (!_token.IsCancellationRequested)
            {
                List<CodeObject> mongoObjs = await _repository.GetAsync();
                if (mongoObjs == null || mongoObjs.Count == 0)
                {
                    _logger.LogInformation($"Could not find any objects on mongo");
                    Thread.Sleep(30000);
                    continue;
                }

                _logger.LogTrace($"Starting parsing lines...");


                Stopwatch watch = new();
                watch.Start();

                int amountSended = 0;
                foreach(CodeObject codeObject in mongoObjs)
                {
                    _rabbitMQMessageSender.SendMessage(
                        new Types.BaseMessage(JsonSerializer.Serialize(codeObject))
                    );

                    await _repository.UpdateAsync(codeObject);
                    amountSended++;
                    if (amountSended % 200 == 0)
                        Console.WriteLine($"Already sent {amountSended} messages");
                        //_logger.LogInformation($"Already sent {amountSended} messages");
                }

                watch.Stop();
                _logger.LogInformation($"Finishing parsing lines... After {watch.ElapsedMilliseconds} miliseconds");


                await _repository.SaveChanges();

                Thread.Sleep(30000);
            }

            _sync.Set();
            return TaskStatus.RanToCompletion;
        }
            
        public void FinishProcessing()
        {
            _sync.WaitOne();
        }
    }
}
