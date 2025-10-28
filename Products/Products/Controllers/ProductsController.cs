using Microsoft.AspNetCore.Mvc;
using Products.Models;
using Products.Services;
using Products.ViewModels;

namespace Products.Controllers
{
    /// <summary>
    /// Controller for managing products.
    /// Provides endpoints to retrieve, add, and bulk insert products.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductsController"/> class.
        /// </summary>
        /// <param name="productService">Service for product operations.</param>
        /// <param name="logger">Logger instance.</param>
        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all products.
        /// </summary>
        /// <returns>A list of all products.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        /// <summary>
        /// Adds products in bulk from a file data.json.
        /// </summary>
        /// <remarks>
        /// This endpoint reads products from a JSON file and adds them to the database in bulk.
        /// </remarks>
        /// <returns>A list of added products.</returns>
        [HttpGet("addProducts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Product>>> AddProducts()
        {
            try
            {
                _logger.LogInformation("Request received for AddProducts");
                var addedProducts = await _productService.AddProductsFromFileAsync();
                _logger.LogInformation("Successfully added {Count} products", addedProducts.Count);
                return Ok(addedProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding products from file");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Adds a new product.
        /// </summary>
        /// <param name="productViewModel">The product view model containing product details.</param>
        /// <returns>The created product.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Product>> AddProduct([FromBody] ProductViewModel productViewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid product model received");
                    return BadRequest(ModelState);
                }

                var product = new Product
                {
                    Name = productViewModel.Name,
                    Description = productViewModel.Description,
                    Price = productViewModel.Price,
                    CreatedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Request received to add new product: {ProductName}", product.Name);
                var addedProduct = await _productService.AddProductAsync(product);
                _logger.LogInformation("Successfully added product with ID: {ProductId}", addedProduct.Id);

                return CreatedAtAction(nameof(GetAllProducts), new { id = addedProduct.Id }, addedProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding product: {ProductName}", productViewModel.Name);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Health check endpoint.
        /// </summary>
        /// <returns>Health status of the API.</returns>
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok("Product API is running");
        }

        [HttpGet("getProductById/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProductById(string id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        /// <summary>
        /// Endpoint to create a test exception for error handling.
        /// </summary>
        /// <returns>Throws an exception.</returns>
        [HttpGet("CreateError")]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CreateError([FromQuery] List<string> ids)
        {
            throw new Exception("This is a test exception for error handling.");
        }
    }
}