using System.Reflection;
using Microsoft.OpenApi.Models;
using Orders.ExternalServices;
using Orders.Repositories;
using Orders.Services;
using Orders.Settings;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;

namespace Orders
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure settings from appsettings.json
            var mongoDbSettings = builder.Configuration.GetSection("MongoDb").Get<MongoDbSettings>();
            var elasticSettings = builder.Configuration.GetSection("ElasticConfiguration").Get<ElasticConfiguration>();
            var externalServicesSettings = builder.Configuration.GetSection("ExternalServices").Get<ExternalServicesSettings>();

            if (mongoDbSettings == null || elasticSettings == null || externalServicesSettings == null)
            {
                throw new InvalidOperationException("Configuration sections 'MongoDb', 'ElasticConfiguration', or 'ExternalServices' are missing or invalid.");
            }

            // Configure Serilog with Elasticsearch
            ConfigureLogging(builder, elasticSettings);

            // Add MongoDB settings
            builder.Services.AddSingleton(mongoDbSettings);
            builder.Services.AddSingleton(externalServicesSettings);

            // Add services to the container
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IOrderService, OrderService>();

            // Configure HttpClients for external services
            builder.Services.AddHttpClient<IProductServiceClient, ProductServiceClient>(client =>
            {
                client.BaseAddress = new Uri(externalServicesSettings.ProductsServiceUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            builder.Services.AddHttpClient<IUserServiceClient, UserServiceClient>(client =>
            {
                client.BaseAddress = new Uri(externalServicesSettings.UsersServiceUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            // Add PlaceOrders background service
            builder.Services.AddHostedService<PlaceOrders>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Orders API",
                    Version = "v1",
                    Description = "Orders API with XML comments"
                });

                // Get XML file path dynamically
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            });

            // Define allowed origins
            string[] allowedOrigins = new[]
            {
                "http://localhost:3000",               // React dev server
                "http://localhost:3001",               // Production React app
            };

            // Register CORS service with named policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowedOriginsPolicy", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseSwagger();
            app.UseSwaggerUI();

            // Use the CORS policy
            app.UseCors("AllowedOriginsPolicy");

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            // Health check endpoint for Kubernetes
            app.MapGet("/health", () => Results.Ok("Healthy"));

            app.Run();
        }

        private static void ConfigureLogging(WebApplicationBuilder builder, ElasticConfiguration elasticConfig)
        {
            var logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .WriteTo.Debug()
                .WriteTo.Console()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticConfig.Uri))
                {
                    AutoRegisterTemplate = true,
                    IndexFormat = elasticConfig.IndexFormat,
                    NumberOfShards = elasticConfig.NumberOfShards,
                    NumberOfReplicas = elasticConfig.NumberOfReplicas
                })
                .CreateLogger();

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(logger);
        }
    }
}
