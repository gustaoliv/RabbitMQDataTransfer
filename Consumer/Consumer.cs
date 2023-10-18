using Consumer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Types;
using ZstdSharp.Unsafe;

namespace Consumer
{
    public class Consumer : BackgroundService
    {
        private readonly ILogger<Consumer>  _logger;
        private readonly Repository _repository;

        private readonly IConnection _connection;
        private readonly IModel      _channel;

        private readonly string _exchangeName;
        private readonly string _queueName;

        private readonly string _hostName;
        private readonly string _password;
        private readonly string _userName;

        public Consumer(ILogger<Consumer> logger, IConfiguration configuration, Repository repository)
        {
            _logger = logger;
            _repository = repository;

            _hostName = configuration["RABBITMQ_HOST"];
            _password = configuration["RABBITMQ_PSWD"];
            _userName = configuration["RABBITMQ_USER"];
            _exchangeName = configuration["RABBITMQ_EXCHANGENAME"];
            _queueName = configuration["RABBITMQ_QUEUENAME"];


            var factory = new ConnectionFactory
            {
                HostName = _hostName,
                UserName = _userName,
                Password = _password,
            };

            _connection = factory.CreateConnection();
            _channel    = _connection.CreateModel();


            _channel.ExchangeDeclare(_exchangeName, ExchangeType.Direct, true);
            _channel.QueueDeclare(_queueName, true, false, false, null);

            _channel.QueueBind(_queueName, _exchangeName, "Names");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (chanel, evt) =>
            {
                var content = Encoding.UTF8.GetString(evt.Body.ToArray());
                BaseMessage message = JsonSerializer.Deserialize<BaseMessage>(content);
                ProcessMessage(message).GetAwaiter().GetResult();
                _channel.BasicAck(evt.DeliveryTag, false);
            };

            _channel.BasicConsume(_queueName, false, consumer);
            return Task.CompletedTask;
        }

        private async Task ProcessMessage(BaseMessage message)
        {
            try
            {
                PersonObject personObj = JsonSerializer.Deserialize<PersonObject>(message.Content);
                if(personObj == null)
                {
                    _logger.LogError($"[Consumer.ProcessMessage] Could not deserialize message. {message.Content}");
                    return;
                }

                _logger.LogInformation($"[Consumer.ProcessMessage] Parsing for person: {personObj.Name}");

                personObj.Id        = null;
                personObj.Name      = RemoveAccentuation(personObj.Name).ToUpper();
                personObj.CPF       = personObj.CPF.Replace(".", "").Replace("-", "");
                personObj.Gender    = personObj.Gender.Equals("Masculino", StringComparison.OrdinalIgnoreCase) ? "M" : "F";
                personObj.Phone     = personObj.Phone.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");

                await _repository.CreateAsync(personObj);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[Consumer.ProcessMessage] Unexpected error. {ex.Message} :: {ex.StackTrace}");
                return;
            }
        }

        private static string RemoveAccentuation(string texto)
        {
            string formD        = texto.Normalize(NormalizationForm.FormD);
            StringBuilder sb    = new StringBuilder();

            foreach (char letra in formD)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(letra) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(letra);
                }
            }

            return sb.ToString();
        }
    }
}
