using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using web_project_BE.Data;
using web_project_BE.Models;

namespace web_project_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartDetailController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartDetailController(ApplicationDbContext context)
        {
            _context = context;
        }

        //Get Cart By Id
        [HttpGet("GetCart")]
        //[Authorize(Policy = "Customer")]
        public async Task<IActionResult> GetCart(int id)
        {
            var cart = _context.Carts.FirstOrDefault(c => c.cart_id == id);

            if (cart == null)
                return BadRequest();

            var ds = _context.Cart_details
                .Join(_context.Products,
                      cartDetail => cartDetail.cd_product_id,
                      product => product.product_id,
                      (cartDetail, product) => new { cartDetail, product })
                .Join(_context.Sizes,
                      joinResult => joinResult.product.p_size_id,
                      size => size.size_id,
                      (joinResult, size) => new
                      {
                          cd_id = joinResult.cartDetail.cd_id,
                          cd_product_id = joinResult.product.product_id,
                          cd_cart_id = joinResult.cartDetail.cd_cart_id,
                          product_image = joinResult.product.product_image,
                          product_name = joinResult.product.product_name,
                          product_price = joinResult.product.product_sell_price,
                          product_size = size.size_number,
                          product_quantity = joinResult.cartDetail.cd_quantity,
                          cd_quantity = joinResult.cartDetail.cd_quantity,
                      }).ToList();

            //var ds = query.ToList();    

            double? total = 0;

            if (ds.Count() >= 1)
            {
                foreach (var item in ds)
                {
                    total += item.product_price * item.product_quantity;
                }
            }

            return Ok(new
            {
                total,
                ds
            });
        }

        [HttpGet("GetCartId")]
        public async Task<IActionResult> GetCartId(string fullname)
        {
            if (fullname == null)
                return BadRequest();

            var cart_id = _context.Carts
                .Include(c => c.Account)
                .Where(c => c.Account.account_username.Equals(fullname))
                .Select(c => c.cart_id)
                .FirstOrDefault();

            return Ok(cart_id);
        }

        [HttpGet("GetProduct")]
        public async Task<IActionResult> GetProduct(string proname, int size)
        {
            var product = _context.Products
            .Join(_context.Sizes,
                  product => product.p_size_id,
                  size => size.size_id,
                  (product, size) => new
                  {
                      cd_product_id = product.product_id,
                      ProductName = product.product_name,
                      ProductSize = size.size_number
                  })
            .Where(item => item.ProductName == proname && item.ProductSize == size)
            .ToList();

            if (product == null)
                return BadRequest(new { Message = "Product is not exist" });

            return Ok(product);
        }

        //Add Cart
        [HttpPost]
        public async Task<IActionResult> AddCart(Cart_Detail cart_Detail)
        {
            if (cart_Detail == null)
                return BadRequest();

            var cartExist = _context.Cart_details.FirstOrDefault(cd => cd.cd_product_id == cart_Detail.cd_product_id 
            && cd.cd_cart_id == cart_Detail.cd_cart_id);

            var product = await _context.Products.FirstOrDefaultAsync(p => p.product_id == cart_Detail.cd_product_id);

            if (cart_Detail.cd_quantity == 0)
                return BadRequest(new { Message = "Product quantity at least one, Please!!" });

            if (cart_Detail.cd_quantity > product.product_quantity_stock)
                return BadRequest(new { Message = "Product in stock only have " + (product.product_quantity_stock) });

            if (cartExist != null)
            {
                cartExist.cd_quantity += cart_Detail.cd_quantity;

                if (cartExist.cd_quantity > product.product_quantity_stock)
                    return BadRequest(new { Message = "Product in stock only have " + (product.product_quantity_stock - (cartExist.cd_quantity - cart_Detail.cd_quantity)) });

                _context.Entry(cartExist).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Add Cart Succeed"
                });
            }

            await _context.Cart_details.AddAsync(cart_Detail);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Add Cart Succeed"
            });
        }


        [HttpPost("plusQty")]
        public async Task<IActionResult> PlusQty(int id)
        {
            var cart = _context.Cart_details.FirstOrDefault(c => c.cd_id == id);
            var product = await _context.Products.FirstOrDefaultAsync(p => p.product_id == cart.cd_product_id);

            if(cart == null)
                return BadRequest();

            cart.cd_quantity += 1;

            if(cart.cd_quantity > product.product_quantity_stock)
                return BadRequest(new { Message = "Product in stock only have " + (product.product_quantity_stock) });

            _context.Entry(cart).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("minusQty")]
        public async Task<IActionResult> MinusQty(int id)
        {
            var cart = _context.Cart_details.FirstOrDefault(c => c.cd_id == id);

            if (cart == null)
                return BadRequest();

            cart.cd_quantity -= 1;

            if(cart.cd_quantity.Equals(0))
            {
                _context.Cart_details.Remove(cart);
                await _context.SaveChangesAsync();
            }

            _context.Entry(cart).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{cd_id}")]
        public async Task<IActionResult> DeleteCart(int cd_id)
        {
            var cart = await _context.Cart_details.FindAsync(cd_id);

            if(cart == null)
                return BadRequest(new {Message = "Delete Failed!"});

            _context.Cart_details.Remove(cart);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Delete Cart Succeed"
            });
        }
    }
}
