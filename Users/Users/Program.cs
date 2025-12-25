
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using System.Reflection;
using Users.Repositories;
using Users.Services;
using Users.Settings;

namespace Users
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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
            builder.Services.AddScoped<IUsersRepository, UsersRepository>();
            builder.Services.AddScoped<IUserService, UserService>();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Users API",
                    Version = "v1",
                    Description = "Users API with XML comments"
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

            // 2️⃣ Register CORS service with named policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowedOriginsPolicy", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials(); // optional, if using cookies or auth headers
                });
            });

            // Bind Redis configuration from appsettings
            var redisConfig = builder.Configuration.GetSection("Redis").Get<RedisConfiguration>();
            if (redisConfig != null)
            {
                builder.Services.AddSingleton(redisConfig);
                builder.Services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConfig.Configuration;
                    options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions()
                    {
                        AbortOnConnectFail = redisConfig.AbortOnConnectFail,
                        EndPoints = { redisConfig.Configuration }
                    };
                });
            }

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
