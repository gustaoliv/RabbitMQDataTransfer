using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Types;


namespace Supplier.Services
{
    public class RabbitMQMessageSender
    {
        private readonly string _hostName;
        private readonly string _password;
        private readonly string _userName;
        private readonly string _rabbitQueueName;
        private IConnection _connection;
        private readonly IModel _channel;


        private readonly ILogger<RabbitMQMessageSender> _logger;


        public RabbitMQMessageSender(IConfiguration configuration, ILogger<RabbitMQMessageSender> logger)
        {
            _hostName           = configuration["RABBITMQ_HOST"];
            _password           = configuration["RABBITMQ_PSWD"];
            _userName           = configuration["RABBITMQ_USER"];
            _rabbitQueueName    = configuration["RABBITMQ_QUEUENAME"];


            if (ConnectionExists())
            {
                _channel = _connection.CreateModel();

                _channel.QueueDeclare(queue: _rabbitQueueName, true, false, false, arguments: null);
            }

            _logger = logger;
        }

        public void SendMessage(BaseMessage message)
        {
            byte[] body = GetMessageAsByteArray(message);

            

            _channel.BasicPublish(exchange: "", routingKey: _rabbitQueueName, basicProperties: null, body: body);   
        }

        private static byte[] GetMessageAsByteArray(BaseMessage message)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };

            var json = JsonSerializer.Serialize(message, options: options);

            var body = Encoding.UTF8.GetBytes(json);
            return body;
        }

        private bool ConnectionExists()
        {
            if (_connection != null) return true;

            CreateConnection();


            return _connection != null;
        }

        private void CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _hostName,
                    UserName = _userName,
                    Password = _password,
                };

                _connection = factory.CreateConnection();
            }
            catch (Exception ex)
            {
                _logger.LogError($"[RabbitMQMessageSender.CreateConnection] Something went wrong creating connection. {ex.Message} :: {ex.StackTrace}");
                throw;
            }
        }
    }
}
