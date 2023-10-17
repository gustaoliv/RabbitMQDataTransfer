using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Supplier.Services;
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
        private readonly NamesRepository _namesRepository;

        #endregion

        #region Auxiliary Variables

        private readonly string _rabbitQueueName;

        #endregion

        public SupplierBusiness(RabbitMQMessageSender rabbitMQMessageSender, 
                                IConfiguration configuration, 
                                ILogger<SupplierBusiness> logger,
                                NamesRepository namesRepository)
        {
            _sync = new(false, EventResetMode.ManualReset);
            _rabbitMQMessageSender = rabbitMQMessageSender;

            _rabbitQueueName = configuration["RABBITMQ_QUEUENAME"];

            _logger = logger;
            _namesRepository = namesRepository;
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
                List<NameObject> mongoObjs = await _namesRepository.GetAsync();
                if(mongoObjs == null || mongoObjs.Count == 0)
                {
                    _logger.LogError($"Could not find any objects on mongo");
                    return TaskStatus.Faulted;
                }

                _logger.LogTrace($"Starting parsing lines...");

                foreach(NameObject nameObject in mongoObjs)
                {
                    _rabbitMQMessageSender.SendMessage(
                        new Types.BaseMessage(nameObject.Nome),
                        _rabbitQueueName
                    );
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
