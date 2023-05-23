namespace Models.Settings
{
    public class Settings
    {
        public const string POSITION = "Settings";
        public MongoSettings MongoDb { get; set; }
        public RabbitMQSettings RabbitMQ { get; set; }
        public TraceabilitySettings Traceability { get; set; }
    }
    public class MongoSettings
    {
        public string ConnectionString { get; set; }
        public MongoConfig Notifications { get; set; }
    }
    public class MongoConfig
    {
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
    }
    public class RabbitMQSettings
    {
        public string HostName { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }
        public string Protocol { get; set; }
        public string QueueName { get; set; }
        public string RoutingKey { get; set; }
        public string ExchangeName { get; set; }
        public string ExchangeType { get; set; }
        public int ConnectionTimeOut { get; set; }
        public string ConnectionName { get; set; }
    }
    public class TraceabilitySettings
    {
        public bool Enabled { get; set; }
        public string Mode { get; set; }
        public string ServiceName { get; set; }
        public string NameSpace { get; set; }
    }
}
