using Orders.DTOs;
using Orders.ExternalServices;
using Orders.Models;
using Orders.Repositories;

namespace Orders.Services
{
    public class PlaceOrders : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PlaceOrders> _logger;

        public PlaceOrders(IServiceProvider serviceProvider, ILogger<PlaceOrders> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PlaceOrders background service is starting.");

            stoppingToken.Register(() =>
                _logger.LogInformation("PlaceOrders background service is stopping."));


            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Placing orders at: {time}", DateTimeOffset.Now);

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        // Fetch products and users once before the loop
                        var productServiceClient = scope.ServiceProvider.GetRequiredService<IProductServiceClient>();
                        var userServiceClient = scope.ServiceProvider.GetRequiredService<IUserServiceClient>();

                        var products = await productServiceClient.GetAllProductsAsync();
                        var users = await userServiceClient.GetAllUsersAsync();

                        _logger.LogInformation("Fetched {ProductsCount} products and {UsersCount} users for order generation.",
                            products.Count, users.Count);

                        var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

                        if (products.Count > 0 && users.Count > 0)
                        {
                            // Create a random order with random product and random user
                            var order = CreateRandomOrder(products, users);
                            await orderRepository.CreateAsync(order);

                            _logger.LogInformation(
                                "Order created: OrderId={OrderId}, UserId={UserId}, ProductId={ProductId}, Quantity={Quantity}, TotalPrice={TotalPrice}",
                                order.Id, order.UserId, order.ProductId, order.Quantity, order.TotalPrice);
                        }
                        else
                        {
                            _logger.LogWarning("Cannot create order. Products count: {ProductsCount}, Users count: {UsersCount}",
                                products.Count, users.Count);
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // Delay for 30 seconds
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in PlaceOrders background service.");
                }
            }

            _logger.LogInformation("PlaceOrders background service has stopped.");
        }

        private Order CreateRandomOrder(List<ProductDto> products, List<UserDto> users)
        {
            var random = new Random();

            // Select random product and user
            var randomProduct = products[random.Next(products.Count)];
            var randomUser = users[random.Next(users.Count)];

            // Generate random quantity between 1 and 10
            var quantity = random.Next(1, 11);

            // Calculate total price
            var totalPrice = randomProduct.Price * quantity;

            return new Order
            {
                UserId = randomUser.Id,
                ProductId = randomProduct.Id,
                ProductPrice = randomProduct.Price,
                Quantity = quantity,
                TotalPrice = totalPrice,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}