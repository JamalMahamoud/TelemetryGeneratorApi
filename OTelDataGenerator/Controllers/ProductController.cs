using Microsoft.AspNetCore.Mvc;

namespace OptelDataGenerator.Controllers
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private static List<Product> _products = new()
        {
            new() { Id = 1, Name = "Product A"},
            new() { Id = 2, Name = "Product B"},
            new() { Id = 3, Name = "Product C"}
        };

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var cancellationTokenSource = new CancellationTokenSource();

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
                throw new Exception("The request timed out.");
            }
            return Ok(_products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            if (id == 0)
            {
                return StatusCode(500, "Internal Server Error");
            }

            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return NotFound();

            return Ok(product);
        }


        [HttpPost("productName")]
        public ActionResult<Product> AddProduct(Product product)
        {
            product.Id = _products.Count + 1;
            _products.Add(product);

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
