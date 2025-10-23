using Microsoft.AspNetCore.Mvc;
using Products.Models;
using Products.Services;

namespace Products.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts()
        {
            try
            {
                _logger.LogInformation("Request received for GetAllProducts");
                var products = await _productService.GetAllProductsAsync();
                _logger.LogInformation("Successfully retrieved {Count} products", products.Count);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all products");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Product>> AddProduct([FromBody] Product product)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid product model received");
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Request received to add new product: {ProductName}", product.Name);
                var addedProduct = await _productService.AddProductAsync(product);
                _logger.LogInformation("Successfully added product with ID: {ProductId}", addedProduct.Id);

                return CreatedAtAction(nameof(GetAllProducts), new { id = addedProduct.Id }, addedProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding product: {ProductName}", product.Name);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}