using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Supplier.Services;
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
        private readonly Repository _personRepository;

        #endregion

        #region Auxiliary Variables

        private readonly string _rabbitQueueName;

        #endregion

        public SupplierBusiness(RabbitMQMessageSender rabbitMQMessageSender, 
                                IConfiguration configuration, 
                                ILogger<SupplierBusiness> logger,
                                Repository namesRepository)
        {
            _sync = new(false, EventResetMode.ManualReset);
            _rabbitMQMessageSender = rabbitMQMessageSender;

            _rabbitQueueName = configuration["RABBITMQ_QUEUENAME"];

            _logger = logger;
            _personRepository = namesRepository;
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
                List<PersonObject> mongoObjs = await _personRepository.GetAsync();
                if(mongoObjs == null || mongoObjs.Count == 0)
                {
                    _logger.LogInformation($"Could not find any objects on mongo");
                    Thread.Sleep(30000);
                    continue;
                }

                _logger.LogTrace($"Starting parsing lines...");

                foreach(PersonObject personObject in mongoObjs)
                {
                    _rabbitMQMessageSender.SendMessage(
                        new Types.BaseMessage(JsonSerializer.Serialize(personObject)),
                        _rabbitQueueName
                    );

                    PersonObject updatedObj = personObject;
                    updatedObj.Exported     = true;
                    await _personRepository.UpdateAsync(updatedObj.Id, updatedObj);
                }

                _logger.LogInformation($"Finishing parsing lines...");
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
