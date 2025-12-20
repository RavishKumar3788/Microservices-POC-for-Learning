using Orders.Repositories;
using Orders.Services;
using Orders.Settings;

namespace Orders
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure settings from appsettings.json
            var mongoDbSettings = builder.Configuration.GetSection("MongoDb").Get<MongoDbSettings>();

            if (mongoDbSettings == null)
            {
                throw new InvalidOperationException("Configuration section 'MongoDb' is missing or invalid.");
            }

            // Add MongoDB settings
            builder.Services.AddSingleton(mongoDbSettings);

            // Add services to the container
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IOrderService, OrderService>();

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

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
            app.MapOpenApi();

            // Use the CORS policy
            app.UseCors("AllowedOriginsPolicy");

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
