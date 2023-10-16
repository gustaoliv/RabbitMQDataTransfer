using Supplier.Services;


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

        #endregion

        public SupplierBusiness(RabbitMQMessageSender rabbitMQMessageSender)
        {
            _sync = new(false, EventResetMode.ManualReset);
            _rabbitMQMessageSender = rabbitMQMessageSender;
        }


        public async Task<TaskStatus> Run(CancellationToken cancellationToken)
        {
            _token = cancellationToken;
            return await Task.Factory.StartNew(() => SendMessages());
        }

        public TaskStatus SendMessages()
        {
            while (!_token.IsCancellationRequested)
            {
                string text     = File.ReadAllText("C:\\Users\\gusta\\Meu Drive\\Programação\\C#\\DotNet6\\RabbitQueueManager\\Supplier\\Input\\Inputs.txt");
                string[] lines  = text.Split('\n');

                foreach(string line in lines)
                {
                    _rabbitMQMessageSender.SendMessage(
                        new Types.BaseMessage()
                        {
                            Id              = Guid.NewGuid().ToString(),
                            MessageCreated  = DateTime.Now,
                            Content         = line
                        },
                        "Names_Queue"
                    );
                }


                
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
