using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Mvc;
using OptelDataGenerator;

namespace OTelDataGenerator.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;

        private static List<Product> _products = new()
        {
            new() { Id = 1, Name = "Product A" },
            new() { Id = 2, Name = "Product B" },
            new() { Id = 3, Name = "Product C" }
        };

        public ProductsController(ILogger<ProductsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            using var myActivity = OpenTelemetryConfig.ActivitySource.StartActivity("GetAllProducts");
            
            var cancellationTokenSource = new CancellationTokenSource();
            myActivity?.SetTag("Some custom tags keys", 1);
            myActivity?.AddEvent(new ActivityEvent("Getting products"));
            if (_products.Count > 3)
            {
                cancellationTokenSource.Cancel();
            }
            try
            {
                // Simulate a delay of 5 seconds
                await Task.Delay(TimeSpan.FromSeconds(_products.Count), cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                myActivity?.SetStatus(ActivityStatusCode.Error, "user waited too long");
                throw new Exception("The request timed out.");
            }
            return Ok(_products);
        }

        [HttpGet("{id}")]
        public Task<ActionResult<Product>> GetProduct(int id)
        {
            if (id == 0)
            {
                _logger.LogError("product id cant to be zero");
            }
            
            var counter = OpenTelemetryConfig.Meter.CreateCounter<int>("Requests");
            var histogram = OpenTelemetryConfig.Meter.CreateHistogram<float>("RequestDuration", unit: "ms");
            OpenTelemetryConfig.Meter.CreateObservableGauge("ThreadCount", () => new[] { new Measurement<int>(ThreadPool.ThreadCount) });

            // Measure the number of requests
            counter.Add(1, KeyValuePair.Create<string, object?>("name", "GetProduct"));

            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return Task.FromResult<ActionResult<Product>>(NotFound());

            return Task.FromResult<ActionResult<Product>>(Ok(product));
        }


        [HttpPost("productName")]
        public ActionResult<Product> AddProduct(Product product)
        {
            using var myActivity = OpenTelemetryConfig.ActivitySource.StartActivity("AddProducts");
            myActivity?.SetTag(OpenTelemetryConfig.AddedProductId, product.Id);
            myActivity?.SetTag(OpenTelemetryConfig.AddedProductName, product.Name);

            try
            {
                myActivity?.AddEvent(new ActivityEvent("Trying to add product"));
                product.Id = _products.Count + 1;
                _products.Add(product);
                myActivity?.AddEvent(new ActivityEvent("Product Added"));
            }
            catch (Exception e)
            {
                myActivity?.SetStatus(ActivityStatusCode.Error, "Here is what went wrong {e}");
                throw;
            }
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }


        [HttpDelete("{id}")]
        public ActionResult DeleteProduct(int id)
        {
            var existingProduct = _products.FirstOrDefault(p => p.Id == id);
            if (existingProduct == null)
                return NotFound();

            _products.Remove(existingProduct);

            return NoContent();
        }
    }
}