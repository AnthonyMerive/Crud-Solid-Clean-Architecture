using System.Text;
using Microsoft.Extensions.Options;
using Models.Settings;
using RabbitMQ.Client;

namespace Classes.Infra.Adapters
{
    public class RabbitMQAdapter : IQueue
    {
        private readonly IConnectionFactory connectionFactory;
        private readonly string queueName;
        private readonly string exchangeName;
        private readonly string exchangeType;
        private readonly string routingKey;
        private readonly string connectionName;
        private readonly ILogger<RabbitMQAdapter> logger;

        public RabbitMQAdapter(
            IConnectionFactory connection,
            IOptions<Settings> settings,
            ILogger<RabbitMQAdapter> logger
        )
        {
            this.connectionFactory = connection;
            this.queueName = settings.Value.RabbitMQ.QueueName;
            this.exchangeName = settings.Value.RabbitMQ.ExchangeName;
            this.routingKey = settings.Value.RabbitMQ.RoutingKey;
            this.exchangeType = settings.Value.RabbitMQ.ExchangeType;
            this.connectionName = settings.Value.RabbitMQ.ConnectionName;
            this.logger = logger;
        }

        public async Task<bool> PublishMessageAsync(string message)
        {
            try
            {
                var connection = connectionFactory.CreateConnection(connectionName);

                var channel = connection.CreateModel();

                InitializeRabbitMQ(channel);

                if (channel.IsClosed) return false;

                byte[] messageBodyBytes = Encoding.UTF8.GetBytes(message);
                var properties = channel.CreateBasicProperties();
                properties.Priority = 0;

                await Task.Run(() => channel.BasicPublish(exchangeName, routingKey, properties, messageBodyBytes));

                if (connection.IsOpen)
                {
                    channel.Close();
                    connection.Close();
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(RabbitMQAdapterLogs.PUBLISH_ERROR, ex.Message);
                return false;
            }
        }

        private void InitializeRabbitMQ(IModel channel)
        {
            channel.ExchangeDeclare(exchangeName, exchangeType, durable: true);

            channel.QueueDeclare(queue: queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            channel.QueueBind(queue: queueName,
                   exchange: exchangeName,
                   routingKey: routingKey);
        }
    }

    internal static class RabbitMQAdapterLogs
    {
        public const string PUBLISH_ERROR = "[Notify-SMS] Ha ocurrido un error al publicar el mensaje en RabbitMQ - ERROR: {ex.Message}";
    }

    public interface IQueue
    {
        public Task<bool> PublishMessageAsync(string message);
    }
}
