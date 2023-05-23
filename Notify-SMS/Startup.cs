using System.Diagnostics.CodeAnalysis;
using System.Net.Security;
using System.Security.Authentication;
using Classes.Domain;
using Classes.Domain.Gateway;
using Classes.Infra.Adapters;
using Models.Settings;
using MongoDB.Driver;
using RabbitMQ.Client;

namespace Notify
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration) { Configuration = configuration; }

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                      .SetBasePath(env.ContentRootPath)
                      .AddJsonFile(ConfigNames.APPSETTINGS_FILE_NAME, optional: false, reloadOnChange: true)
                      .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //--------------------------------------------------------------
            //DependencyInjection:

            //Initializations:
            var settings = Configuration.GetSection(Settings.POSITION).Get<Settings>();

            var mongoDbConnectionString = settings.MongoDb.ConnectionString;

            var rabbitMQConnection = new ConnectionFactory()
            {
                HostName = settings.RabbitMQ.HostName,
                Port = settings.RabbitMQ.Port,
                UserName = settings.RabbitMQ.UserName,
                Password = settings.RabbitMQ.Password,
                VirtualHost = settings.RabbitMQ.VirtualHost,
                RequestedConnectionTimeout = TimeSpan.FromSeconds(5),
                Ssl = new SslOption
                {
                    ServerName = settings.RabbitMQ.HostName,
                    Enabled = settings.RabbitMQ.Protocol.ToUpper().Equals(ConfigNames.RABBIT_AMQPS_NAME)
                    || settings.RabbitMQ.Protocol.ToUpper().Equals(ConfigNames.RABBIT_HTTPS_NAME),
                    Version = SslProtocols.Tls12,
                    AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateNameMismatch
                    | SslPolicyErrors.RemoteCertificateChainErrors
                    | SslPolicyErrors.RemoteCertificateNotAvailable
                }
            };

            //--------------------------------------------------------------

            //Clients:
            services.AddSingleton<IMongoClient>(new MongoClient(mongoDbConnectionString));
            services.AddSingleton<IConnectionFactory>(rabbitMQConnection);

            //--------------------------------------------------------------

            //Adapters:
            services.AddSingleton<IDatabase, MongoDBAdapter>();
            services.AddSingleton<IQueue, RabbitMQAdapter>();
            //--------------------------------------------------------------

            //Services:
            services.AddSingleton<INotifyService, NotifyService>();
            //--------------------------------------------------------------

            //Options:
            services.Configure<Settings>(Configuration.GetSection(Settings.POSITION));
            //--------------------------------------------------------------

            services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
            services.AddOptions();
            //--------------------------------------------------------------
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) { app.UseDeveloperExceptionPage(); }
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/api/status", async context =>
                {
                    await context.Response.WriteAsync("OK");
                });
                endpoints.MapControllers();
            });
        }
    }

    internal static class ConfigNames
    {
        public const string APPSETTINGS_FILE_NAME = "appsettings.json";
        public const string RABBIT_AMQPS_NAME = "AMQPS";
        public const string RABBIT_HTTPS_NAME = "HTTPS";
    }
}
