using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient.DataAnnotations.Cypher.Functions;
using web_project_BE.Data;

namespace web_project_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ImageController(ApplicationDbContext context) => _context = context;

        [HttpGet("{product_image}")]
        public async Task<IActionResult> GetImageByProImage(string product_image)
        {
            //var image = _context.Images.Where(i => i.i_product_id == product_id).ToList();
            //var str = "chelseabrown1.png";
            var product = _context.Products.Where(p => p.product_image.Equals(product_image)).Select(p => p.product_id).FirstOrDefault();

            var image = _context.Images.Where(i => i.i_product_id == product).ToList();

            if (image == null)
                return NotFound();

            List<object> result = new List<object>();
            result.Add(image);

            return Ok(image);
        }

        [HttpDelete("delete")]
        //[Authorize(Policy = "Admin")]
        public async Task<IActionResult> DeleteProductImages(int pro_id)
        {
            var image = _context.Images.Where(i => i.i_product_id == pro_id).ToList();

            if (image == null)
                return NotFound();

            for(int i = 0; i < image.Count; i++)
            {
                _context.Images.Remove(image[i]);
                await _context.SaveChangesAsync();
            }

            return Ok(new {Message = "Upload Image Succeed"});
        }
    }
}
