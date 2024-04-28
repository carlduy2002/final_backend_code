using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using web_project_BE.Data;
using web_project_BE.Models;

namespace web_project_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        readonly ApplicationDbContext _context;

        public RoleController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> getAllRole()
        {
            var roles = _context.Roles.ToList();

            return Ok(roles);
        }

        [HttpGet("role_name")]
        public async Task<IActionResult> getIdRole(string role_name)
        {
            var obj = _context.Roles.FirstOrDefault(c => c.role_name == role_name);

            if (obj == null)
                return NotFound(new { Message = "Role is not found!" });

            return Ok(obj.role_id);
        }

        [HttpPost]
        //[Authorize(Policy = "Admin")]
        public async Task<IActionResult> addRole(Roles roles)
        {
            if (roles == null)
                return BadRequest(new { Message = "Role is null" });

            _context.Roles.Add(roles);  
            await _context.SaveChangesAsync();

            return Ok(new {Message = "Add Role Succeed"});
        }
    }
}
