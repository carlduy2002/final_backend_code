using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using web_project_BE.Data;
using web_project_BE.Helper;
using web_project_BE.Models;
using IEmailService = web_project_BE.UtilityServices.IEmailService;

namespace web_project_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;


        public CategoryController(ApplicationDbContext context, IWebHostEnvironment env, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _env = env;
            _configuration = configuration;
            _emailService = emailService;
        }

        [HttpGet]
        //[Authorize(Policy = "AdminOrManagerPolicy")]
        public async Task<IEnumerable<Category>> GetCategory()
            => await _context.Categories.Where(c => c.category_status.Equals(category_status.Yes.ToString()) 
                                                || c.category_status.Equals(category_status.Waiting_deleted.ToString())).ToListAsync();

        [HttpGet("GetAllCategory")]
        [Authorize(Policy = "AdminOrManagerPolicy")]
        public async Task<IEnumerable<Category>> GetAllCategory()
            => await _context.Categories.ToListAsync();

        [HttpGet("category_name")]
        public async Task<IActionResult> getIdCategory(string category_name)
        {
            var obj = _context.Categories.FirstOrDefault(c => c.category_name == category_name);

            if (obj == null)
                return NotFound(new { Message = "Category is not found!" });
            
            return Ok(obj.category_id);
        }

        [HttpGet("category_id")]
        //[Authorize(Policy = "Admin")]
        public async Task<IActionResult> GetCategoryById(int category_id)
        {
            var category = await _context.Categories.FindAsync(category_id);
            return category == null ? NotFound(new { Message = "Category with the id " + category_id + " is not found!!" }) : Ok(category);
        }

        [HttpPost("AddCategory")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> AddCategory(Category category)
        {
            if (category == null)
                return BadRequest();

            if(await checkExistCategory(category.category_name))
                return BadRequest(new {Message = "Category already exist!"});

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Add Category Succeed"
            });
        }

        [HttpPost("add_category_by_manager")]
        [Authorize(Policy = "Manager")]
        public async Task<IActionResult> AddCategoryByManager()
        {
            var adder = Request.Form["adder"];
            var name = Request.Form["category_name"];
            var status = Request.Form["category_status"];
            var description = Request.Form["category_description"];

            if (await checkExistCategory(name))
                return BadRequest(new { Message = "Category already exist!" });

            Category category = new Category();

            category.category_name = name;
            category.category_description = description;
            category.category_status = category_status.New.ToString();

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            int maxCategoryId = _context.Categories.Max(c => c.category_id);

            var getAdminEmail = _context.Accounts
               .Include(a => a.role)
               .Where(a => a.role.role_name.Equals("Admin"))
               .Select(a => a.account_email)
               .FirstOrDefault();

            var message = "You received this email because a new category with id: " + maxCategoryId + " just added by manager named:";

            SendEmail(getAdminEmail, adder, message);

            return Ok(new
            {
                Message = "Waiting For Confirmation Of New Category"
            });
        }

        private void SendEmail(string email, string adder, string message)
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var emailToken = Convert.ToBase64String(tokenBytes);

            string from = _configuration["EmailSettings:From"];
            var emailBody = EmailBody.CategoryNotifyEmail(adder, message);
            var emailModel = new EmailModel(email, "Category Notify Email", emailBody);
            _emailService.SendEmail(emailModel);
        }

        private Task<bool> checkExistCategory(string category_name)
        {
            return _context.Categories.AnyAsync(c => c.category_name == category_name);
        }

        [HttpPut("UpdateCategoryByAdmin")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> UpdateCategory(Category category)
        {
            if (category == null)
                return BadRequest();

            var cate = await _context.Categories.FirstOrDefaultAsync(c => c.category_id == category.category_id);

            if (cate == null)
                return NotFound(new { Message = "Category with the id " + category.category_id + " is not found!" });

            if(category.category_name != cate.category_name)
            {
                if (await checkExistCategory(category.category_name))
                    return BadRequest(new { Message = "Category already exist!" });
            }

            category.category_status = category.category_status;

            if (category.category_status.Equals(category_status.Yes.ToString()))
            {
                var product = _context.Products
                .Where(p => p.p_category_id == category.category_id)
                .ToList();

                foreach (var p in product)
                {
                    p.product_status = product_status.Yes.ToString();

                    _context.Products.Entry(p).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
            }

            _context.Entry(cate).CurrentValues.SetValues(category);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Update Category Succeed"
            });
        }


        //update category by manager
        [HttpPut("update_category_by_manager")]
        [Authorize(Policy = "Manager")]
        public async Task<IActionResult> UpdateCategoryByManager()
        {
            var id = Request.Form["category_id"];
            var updater = Request.Form["adder"];
            var name = Request.Form["category_name"];
            var status = Request.Form["category_status"];
            var description = Request.Form["category_description"];

            var cate = await _context.Categories.FirstOrDefaultAsync(c => c.category_id.Equals(int.Parse(id)));

            if (cate == null)
                return NotFound(new { Message = "Category with the id " + int.Parse(id) + " is not found!" });

            if (!name.Equals(cate.category_name))
            {
                if (await checkExistCategory(name))
                    return BadRequest(new { Message = "Category already exist!" });
            }

            cate.category_status = status;
            cate.category_description = description;
            cate.category_name = name;
            cate.category_id = int.Parse(id);

            _context.Entry(cate).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var getAdminEmail = _context.Accounts
               .Include(a => a.role)
               .Where(a => a.role.role_name.Equals("Admin"))
               .Select(a => a.account_email)
               .FirstOrDefault();

            var message = "You received this email because a category with id: " + int.Parse(id) + " just updated by manager named:";

            SendEmail(getAdminEmail, updater, message);

            return Ok(new
            {
                Message = "Update Category Succeed"
            });
        }


        //delete category by admin
        [HttpDelete("{category_id}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> DeleteCategory(int category_id)
        {
            var category = await _context.Categories.FindAsync(category_id);
            if(category == null)
                return NotFound(new { Message = "Category with the id " + category_id + " is not found!!" });

            category.category_status = category_status.No.ToString();

            _context.Categories.Entry(category).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var product = _context.Products
                .Where(p => p.p_category_id == category_id)
                .ToList();

            foreach(var p in product)
            {
                p.product_status = product_status.No.ToString();

                _context.Products.Entry(p).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                Message = "Delete Category Succeed"
            });
        }


        //delete category by manager
        [HttpPost("delete_category_by_manager")]
        [Authorize(Policy = "Manager")]
        public async Task<IActionResult> DeleteCategoryByManager()
        {
            var id = Request.Form["category_id"];
            var deleter = Request.Form["adder"];

            var category = await _context.Categories.FindAsync(int.Parse(id));
            if (category == null)
                return NotFound(new { Message = "Category with the id " + int.Parse(id) + " is not found!!" });

            category.category_status = category_status.Waiting_deleted.ToString();

            _context.Categories.Entry(category).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var getAdminEmail = _context.Accounts
               .Include(a => a.role)
               .Where(a => a.role.role_name.Equals("Admin"))
               .Select(a => a.account_email)
               .FirstOrDefault();

            var message = "You received this email because a category with id: " + int.Parse(id) + " just deleted by manager named:";

            SendEmail(getAdminEmail, deleter, message);

            return Ok(new
            {
                Message = "Waiting For Confirmation Of Category Deletion"
            });
        }

        [HttpGet("FindProductByCategory")]
        public async Task<IActionResult> FindProductByCategory(string categoryKey)
        {
            var lstProduct = _context.Products.Include(p => p.category).
                Select(p => new 
                {
                    product_id = p.product_id,
                    product_name = p.product_name,
                    product_quantity_stock = p.product_quantity_stock,
                    product_original_price = p.product_original_price,
                    product_sell_price = p.product_sell_price,
                    product_description = p.product_description,
                    product_status = p.product_status,
                    p_category_id = p.p_category_id,
                    p_supplier_id = p.p_supplier_id,
                    category_name = p.category.category_name,
                    product_image = p.product_image,
                    product_import_date = p.product_import_date,

                }).Where(p => p.category_name.Contains(categoryKey) && p.product_quantity_stock > 0)
                .GroupBy(p => p.product_name)
                .Select(g => g.First())
                .ToList();

            if (lstProduct.Count == 0)
                return NotFound(new { Message = "Product with category named " + categoryKey + " is out of stock!" });

            return Ok(lstProduct);
        }

        //Confirm the new category added by manager
        [HttpPut("confirm_category")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> confirmCategory()
        {
            var category_id = Request.Form["category_id"];

            var category = _context.Categories.Where(c => c.category_id.Equals(int.Parse(category_id))).FirstOrDefault();

            if (category == null)
                return NotFound(new { Message = "Category with the id " + int.Parse(category_id) + " is not found!" });

            category.category_status = category_status.Yes.ToString();

            _context.Entry(category).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "The New Category with ID: " + int.Parse(category_id) + " Confirmed" });
        }

        [HttpGet("search")]
        [Authorize(Policy = "AdminOrManagerPolicy")]
        //[Authorize(Policy = "Manager")]
        public async Task<IActionResult> search(string key)
        {
            var category = _context.Categories.Where(p => p.category_name.Contains(key) || p.category_status.Contains(key)).ToList();

            if (category == null)
                return NotFound(new { Message = "Category not found!" });

            return Ok(category);
        }
    }
}
