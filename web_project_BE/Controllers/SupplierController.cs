using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Text;
using web_project_BE.Data;
using web_project_BE.Models;

namespace web_project_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SupplierController(ApplicationDbContext context) => _context = context;

        [HttpGet("getAll")]
        [Authorize(Policy = "AdminOrManagerPolicy")]
        public async Task<IEnumerable<Supplier>> GetSupplier()
             => await _context.Suppliers.ToListAsync();

        [HttpGet("getAllSupplierWithStatusYes")]
        [Authorize(Policy = "AdminOrManagerPolicy")]
        public async Task<IEnumerable<Supplier>> GetSupplierWithStatusYes()
             => await _context.Suppliers.Where(s => s.supplier_status.Equals(supplier_status.Yes.ToString())).ToListAsync();


        [HttpGet("supplier_name")]
        public async Task<IActionResult> getIdSupplier(string supplier_name)
        {
            var obj = _context.Suppliers.FirstOrDefault(c => c.supplier_name == supplier_name);

            if (obj == null)
                return NotFound(new { Message = "Supplier is not found!" });

            return Ok(obj.supplier_id);
        }

        [HttpGet("{supplier_id}")]
        public async Task<IActionResult> GetSupplierById(int supplier_id)
        {
            var supplier = await _context.Suppliers.FindAsync(supplier_id);
            return supplier == null ? NotFound(new { Message = "Supplier with the id " + supplier_id + " is not found!!" }) : Ok(supplier);
        }

        [HttpPost]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> AddSupplier(Supplier supplier)
        {
            if (supplier == null)
                return BadRequest();

            if (await checkExistSupplier(supplier.supplier_name))
                return BadRequest(new { Message = "Supplier already exist!" });

            if (await CheckEmailExist(supplier.supplier_email))
                return BadRequest(new { Message = "Email already exist!" });

            var email = CheckEmailValid(supplier.supplier_email);
            if (!string.IsNullOrEmpty(email))
                return BadRequest(new { Message = email.ToString() });

            if (await CheckPhoneExist(supplier.supplier_phone))
                return BadRequest(new { Message = "Phone number already exist!" });

            var phone = CheckPhoneValid(supplier.supplier_phone);
            if (!string.IsNullOrEmpty(phone))
                return BadRequest(new { Message = phone.ToString() });

            supplier.supplier_status = supplier_status.Yes.ToString();  

            await _context.Suppliers.AddAsync(supplier);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Add Supplier Succeed"
            });
        }

        private Task<bool> checkExistSupplier(string supplier_name)
        {
            return _context.Suppliers.AnyAsync(s => s.supplier_name == supplier_name);
        }

        private Task<bool> CheckEmailExist(string email)
        {
            return _context.Suppliers.AnyAsync(s => s.supplier_email == email);
        }

        private string CheckEmailValid(string email)
        {
            StringBuilder sb = new StringBuilder();
            if (!(Regex.IsMatch(email, "^[\\w\\d]+@gre+\\.ac\\.uk$")
                || Regex.IsMatch(email, "^([a-zA-z0-9]+@gmail+\\.[a-zA-Z]{2,})$")
                || Regex.IsMatch(email, "^([a-zA-z0-9]+@fpt+\\.edu+\\.vn)$")))
                sb.Append("Email is invalid" + Environment.NewLine);

            return sb.ToString();
        }

        private string CheckPhoneValid(string phone)
        {
            StringBuilder sb = new StringBuilder();
            if (phone.Length < 0 || phone.Length > 10)
                sb.Append("Phone is invalid!" + Environment.NewLine);
            if (!(Regex.IsMatch(phone, "^(03|05|09|07|08)[0-9]{8}$")))
                sb.Append("Phone number invalid" + Environment.NewLine);

            return sb.ToString();
        }

        private Task<bool> CheckPhoneExist(string phone)
        {
            return _context.Suppliers.AnyAsync(s => s.supplier_phone == phone);
        }

        [HttpPut]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> UpdateSupplier(Supplier supplier)
        {
            if (supplier == null)
                return BadRequest();

            var sup = await _context.Suppliers.FirstOrDefaultAsync(s => s.supplier_id == supplier.supplier_id);

            if (sup == null)
                return NotFound(new { Message = "Supplier with the id " + supplier.supplier_id + " is not found!" });

            if (supplier.supplier_name != sup.supplier_name)
            {
                if (await checkExistSupplier(supplier.supplier_name))
                    return BadRequest(new { Message = "Supplier already exist!" });
            }

            if (supplier.supplier_email != sup.supplier_email)
            {
                if (await CheckEmailExist(supplier.supplier_email))
                    return BadRequest(new { Message = "Email already exist!" });
            }

            var email = CheckEmailValid(supplier.supplier_email);
            if (!string.IsNullOrEmpty(email))
                return BadRequest(new { Message = email.ToString() });

            var phone = CheckPhoneValid(supplier.supplier_phone);
            if (!string.IsNullOrEmpty(phone))
                return BadRequest(new { Message = phone.ToString() });

            _context.Entry(sup).CurrentValues.SetValues(supplier);
            await _context.SaveChangesAsync();

            if (sup.supplier_status.Equals(supplier_status.Yes.ToString()))
            {
                var product = _context.Products
                .Where(p => p.p_supplier_id == sup.supplier_id)
                .ToList();

                foreach (var p in product)
                {
                    p.product_status = product_status.Yes.ToString();

                    _context.Products.Entry(p).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
            }

            return Ok(new
            {
                Message = "Update Supplier Succeed"
            });
        }

        [HttpDelete("{supplier_id}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> DeleteSupplier(int supplier_id)
        {
            var supplier = await _context.Suppliers.FindAsync(supplier_id);
            if (supplier == null)
                return NotFound(new { Message = "Supplier with id " + supplier_id + " is not found!" });

            supplier.supplier_status = supplier_status.No.ToString();

            _context.Suppliers.Entry(supplier).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var product = _context.Products
                .Where(p => p.p_supplier_id == supplier_id)
                .ToList();

            foreach (var p in product)
            {
                p.product_status = product_status.No.ToString();

                _context.Products.Entry(p).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                Message = "Delete Supplier Succeed"
            });
        }

        [HttpGet("search")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> search(string key)
        {
            var supplier = _context.Suppliers.Where(s => s.supplier_name.Contains(key) || s.supplier_status.ToString().Contains(key)).ToList();

            if (supplier == null)
                return NotFound(new { Message = "Supplier not found!" });

            return Ok(supplier);
        }
    }
}
