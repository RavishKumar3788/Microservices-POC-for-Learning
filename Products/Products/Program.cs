
using Products.Repositories;
using Products.Services;
using Products.Settings;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;

namespace Products
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure settings from appsettings.json
            var mongoDbSettings = builder.Configuration.GetSection("MongoDb").Get<MongoDbSettings>();
            var elasticSettings = builder.Configuration.GetSection("ElasticConfiguration").Get<ElasticConfiguration>();

            if (mongoDbSettings == null || elasticSettings == null)
            {
                throw new InvalidOperationException("Configuration sections 'MongoDb' or 'ElasticConfiguration' are missing or invalid.");
            }

            // Configure Serilog with Elasticsearch
            ConfigureLogging(builder, elasticSettings);

            // Add MongoDB settings
            builder.Services.AddSingleton(mongoDbSettings);

            // Add services to the container
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IProductService, ProductService>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline

            app.UseSwagger();
            app.UseSwaggerUI();


            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

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
                    IndexFormat = string.Format(elasticConfig.IndexFormat, DateTime.UtcNow),
                    NumberOfShards = elasticConfig.NumberOfShards,
                    NumberOfReplicas = elasticConfig.NumberOfReplicas
                })
                .CreateLogger();

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(logger);
        }
    }
}
