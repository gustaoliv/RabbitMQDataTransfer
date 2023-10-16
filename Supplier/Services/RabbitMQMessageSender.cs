using Microsoft.Extensions.Configuration;
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
        private IConnection _connection;

        public RabbitMQMessageSender(IConfiguration configuration)
        {
            _hostName = configuration["RABBITMQ_HOST"];
            _password = configuration["RABBITMQ_PSWD"];
            _userName = configuration["RABBITMQ_USER"];
        }

        public void SendMessage(BaseMessage message, string queueName)
        {
            if (ConnectionExists())
            {
                using var channel = _connection.CreateModel();

                channel.QueueDeclare(queue: queueName, true, false, false, arguments: null);

                byte[] body = GetMessageAsByteArray(message);

                channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
            }
        }

        private byte[] GetMessageAsByteArray(BaseMessage message)
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
                Console.WriteLine($"[RabbitMQMessageSender.CreateConnection] Something went wrong creating connection. {ex.Message} :: {ex.StackTrace}");
                throw;
            }
        }
    }
}
