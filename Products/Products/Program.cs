
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

            // Configure Serilog with Elasticsearch
            ConfigureLogging(builder);

            // Add MongoDB settings
            var mongoDbSettings = new MongoDbSettings
            {
                ConnectionString = "mongodb://mongodb:27017",
                DatabaseName = "ProductsDb",
                CollectionName = "Products"
            };
            builder.Services.AddSingleton(mongoDbSettings);

            // Add services to the container
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IProductService, ProductService>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }

        private static void ConfigureLogging(WebApplicationBuilder builder)
        {
            var logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .WriteTo.Debug()
                .WriteTo.Console()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://elasticsearch:9200"))
                {
                    AutoRegisterTemplate = true,
                    IndexFormat = $"products-{DateTime.UtcNow:yyyy-MM}",
                    NumberOfShards = 2,
                    NumberOfReplicas = 1
                })
                .CreateLogger();

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(logger);
        }
    }
}
