using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_project_BE.Data;
using web_project_BE.Models;

namespace web_project_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SizeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SizeController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        public async Task<IEnumerable<Size>> GetSize()
            => await _context.Sizes.ToListAsync();

        [HttpGet("{size_id}")]
        public async Task<IActionResult> getSizeById(int size_id)
        {
            var obj = _context.Sizes.FirstOrDefault(s => s.size_id == size_id);

            if (obj == null)
                return NotFound(new { Message = "Size is not found!" });

            return Ok(obj);
        }

        [HttpGet("size_number")]
        public async Task<IActionResult> getIdSize(int size_number)
        {
            var obj = _context.Sizes.FirstOrDefault(s => s.size_number == size_number);

            if (obj == null)
                return NotFound(new { Message = "Size is not found!" });

            return Ok(obj.size_id);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrManagerPolicy")]
        public async Task<IActionResult> AddSize(Size size)
        {
            if (size == null)
                return BadRequest(new { Message = "Size is null" });

            if (await checkExistsize(size.size_number))
                return BadRequest(new { Message = "Size already exist!" });

            size.size_status = size_status.Yes.ToString();

            await _context.Sizes.AddAsync(size);
            await _context.SaveChangesAsync();

            return Ok(new {Message = "Add Size Succeed"});
        }

        [HttpPut]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> updateSize(Size sizes)
        {
            if (sizes == null)
                return BadRequest();

            var size = _context.Sizes.Where(s => s.size_id == sizes.size_id).FirstOrDefault();

            if (size == null)
                return NotFound(new { Message = "Size with the id " + sizes.size_id + " is not found!" });

            if (sizes.size_number != size.size_number)
            {
                if (await checkExistsize(sizes.size_number))
                    return BadRequest(new { Message = "Size already exist!" });
            }

            size.size_number = sizes.size_number;

            _context.Entry(size).CurrentValues.SetValues(sizes);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Update Size Succeed"
            });
        }

        [HttpDelete("{size_id}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> DeleteSize(int size_id)
        {
            var size = await _context.Sizes.FindAsync(size_id);

            if(size == null)
                return NotFound(new { Message = "Size is not found!"});

            size.size_status = size_status.No.ToString();

            _context.Sizes.Entry(size).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Delete Size Succeed" });
        }

        private Task<bool> checkExistsize(int size_number)
        {
            return _context.Sizes.AnyAsync(s => s.size_number.Equals(size_number));
        }
    }
}
