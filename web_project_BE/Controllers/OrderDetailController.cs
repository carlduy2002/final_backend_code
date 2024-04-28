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
    public class OrderDetailController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrderDetailController(ApplicationDbContext context) => _context = context;

        [HttpGet("view_detail")]
        [Authorize(Policy = "AdminOrManagerPolicy")]
        public async Task<IActionResult> ViewOrderDetail(int id)
        {
            var lstOrderDetail = _context.Order_details.Include(o => o.order).
                Include(o => o.product).
                Select(p => new
                {
                    od_id = p.od_id,
                    od_quantity = p.od_quantity,
                    od_product_id = p.od_product_id,
                    od_order_id = p.od_order_id,
                    product_image = p.product.product_image,
                    product_name = p.product.product_name,
                    product_price = p.product.product_sell_price,
                    category_name = _context.Categories
                        .Where(c => c.category_id == p.product.p_category_id)
                        .Select(c => c.category_name)
                        .FirstOrDefault(),
                    supplier_name = _context.Suppliers
                        .Where(s => s.supplier_id == p.product.p_supplier_id)
                        .Select(s => s.supplier_name)
                        .FirstOrDefault(),

                }).Where(o => o.od_order_id == id).ToList();

            return Ok(lstOrderDetail);
        }

        [HttpGet("view_order_history_detail")]
        [Authorize(Policy = "Customer")]
        public async Task<IActionResult> ViewOrderHistoryDetail(int id)
        {
            var lstOrderDetail = _context.Order_details.Include(o => o.order).
                Include(o => o.product).
                Select(p => new
                {
                    od_id = p.od_id,
                    od_quantity = p.od_quantity,
                    od_product_id = p.od_product_id,
                    od_order_id = p.od_order_id,
                    product_image = p.product.product_image,
                    product_name = p.product.product_name,
                    product_price = p.product.product_sell_price,
                    category_name = _context.Categories
                        .Where(c => c.category_id == p.product.p_category_id)
                        .Select(c => c.category_name)
                        .FirstOrDefault(),
                    supplier_name = _context.Suppliers
                        .Where(s => s.supplier_id == p.product.p_supplier_id)
                        .Select(s => s.supplier_name)
                        .FirstOrDefault(),

                }).Where(o => o.od_order_id == id).ToList();

            return Ok(lstOrderDetail);
        }
    }
}
