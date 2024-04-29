using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient.DataAnnotations.Cypher.Functions;
using System.Text.RegularExpressions;
using System.Text;
using web_project_BE.Data;
using web_project_BE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.CodeDom;
using System.Security.Cryptography;
using web_project_BE.Helper;
using IEmailService = web_project_BE.UtilityServices.IEmailService;


namespace web_project_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;


        public OrderController(ApplicationDbContext context, IWebHostEnvironment env, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _env = env;
            _configuration = configuration;
            _emailService = emailService;
        }

        [HttpGet]
        [Authorize(Policy = "AdminOrManagerPolicy")]
        public async Task<IActionResult> GetAllOrder()
        {
            var lstOrder = _context.Orders.Include(o => o.account).
                Select(p => new 
                {
                    order_id = p.order_id,
                    order_date = p.order_date,
                    delivery_date = p.delivery_date,
                    order_address = p.order_address,
                    order_phone = p.order_phone,
                    order_quantity = p.order_quantity,
                    order_note = p.order_note,
                    order_status = p.order_status,
                    order_payment = p.order_payment,
                    o_account_id = p.o_account_id,
                    account_name = p.account.account_username,
                    order_total_price = p.order_total_price

                }).ToList();

            return Ok(lstOrder);
        }

        [HttpGet("get_order_history")]
        [Authorize(Policy = "Customer")]
        public async Task<IActionResult> GetOrderHistory(string username)
        {
            if (username == null)
                return BadRequest();

            var accountId = _context.Accounts
                .Where(a => a.account_username.Equals(username))
                .Select(a => a.account_id)
                .FirstOrDefault();

            if(accountId == null)
                return BadRequest();

            var orders = _context.Orders
                .Include(o => o.account)
                .Where(o => o.o_account_id.Equals(accountId))  
                .Select(o => new
                {
                    order_id = o.order_id,
                    order_date = o.order_date,
                    order_address = o.order_address,
                    order_phone = o.order_phone,
                    order_quantity = o.order_quantity,
                    order_status = o.order_status,
                    order_total_price = o.order_total_price
                })
                .ToList();

            return Ok(orders);
        }

        [HttpPost]
        [Authorize(Policy = "Customer")]
        public async Task<IActionResult> ProcessOrder([FromBody] CartAndOrder model)
        {
            List<Cart_Detail> cart = model.Cart;
            Order order = model.Order;

            if (order == null || cart == null || cart.Count == 0)
                return BadRequest();

            var cart_id = 0;
            var totalQty = 0;

            foreach (var item in cart)
            {
                totalQty += item.cd_quantity;

                var product = await _context.Products.FirstOrDefaultAsync(p => p.product_id == item.cd_product_id);

                if (item.cd_quantity > product.product_quantity_stock)
                    return BadRequest(new {Message = "Product named " + product.product_name + " is out of stock!"});

                cart_id = item.cd_cart_id;
            }

            var account_id = _context.Carts
                .Where(c => c.cart_id.Equals(cart_id))
                .Select(c => c.account_id)
                .FirstOrDefault();

            var phone = CheckPhoneValid(order.order_phone);
            if (!string.IsNullOrEmpty(phone))
                return BadRequest(new { Message = phone.ToString() });

            order.order_date = DateTime.Now;
            order.order_address = order.order_address?.ToString();
            order.order_phone = order.order_phone?.ToString();
            order.order_quantity = totalQty;
            order.order_note = order.order_note?.ToString();
            order.order_status = order_status.Pending.ToString();
            order.order_payment = order.order_payment?.ToString();
            order.o_account_id = account_id;
            order.order_total_price = order.order_total_price;

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            foreach (var cartDetail in cart)
            {
                var price = _context.Products
                    .Where(p => p.product_id == cartDetail.cd_product_id)
                    .Select(p => p.product_sell_price)
                    .FirstOrDefault();

                Order_Detail order_Detail = new Order_Detail
                {
                    od_quantity = cartDetail.cd_quantity,
                    od_order_id = order.order_id,
                    od_product_id = cartDetail.cd_product_id,
                    od_product_price = (cartDetail.cd_quantity * price)
                };

                _context.Order_details.Add(order_Detail);
                await _context.SaveChangesAsync();

                var product = await _context.Products.FirstOrDefaultAsync(p => p.product_id == cartDetail.cd_product_id);

                if (product == null)
                    return BadRequest();

                product.product_quantity_stock -= cartDetail.cd_quantity;

                if(product.product_quantity_stock.Equals(0))
                {
                    product.product_status = product_status.No.ToString();
                    _context.Entry(product).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }

                _context.Entry(product).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }

            var getMaxOrderId = _context.Orders.Max(o => o.order_id);

            var getAdminEmail = _context.Accounts
             .Include(a => a.role)
             .Where(a => a.role.role_name.Equals("Admin"))
             .Select(a => a.account_email)
             .FirstOrDefault();

            var getUser = _context.Accounts
                .Where(a => a.account_id == order.o_account_id)
                .Select(a => a.account_username)
                .FirstOrDefault();

            var message = "You received this email because an new order with id: " + getMaxOrderId + " just ordered by account name: ";

            SendEmail(getAdminEmail, getUser, message);

            var cartDateil = _context.Cart_details
                .Where(cd => cd.cd_cart_id == cart_id)
                .ToList();

            foreach(var c in cartDateil)
            {
                _context.Cart_details.Remove(c);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                Message = "Order Success"
            });
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

        [HttpPut("ConfirmOrder")]
        [Authorize(Policy = "AdminOrManagerPolicy")]
        public async Task<IActionResult> ConfirmOrder(int id, string confimer)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
                return NotFound(new { Message = "Order is Not Found!" });

            if (confimer == null)
                return BadRequest();

            order.delivery_date = DateTime.Now;

            if (!string.IsNullOrEmpty(CheckDeliveryDate(id, order.delivery_date.ToString())))
                return BadRequest(new { Message = CheckDeliveryDate(id, order.delivery_date.ToString()).ToString() });

            order.order_status = order_status.Awaiting_Pickup.ToString();

            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var getcanceler = _context.Accounts
                .Include(a => a.role)
                .Where(a => a.account_username.Equals(confimer))
                .Select(a => a.role.role_name)
                .FirstOrDefault();

            var getUserEmail = _context.Accounts
                 .Where(a => a.account_id == order.o_account_id)
                 .Select(a => a.account_email)
                 .FirstOrDefault();

            var messageForCustomer = "You received this email to confirm your order with id: " + id + " just confirmed";

            if (getcanceler.Equals("Admin"))
            {
                SendEmail(getUserEmail, "", messageForCustomer);
            }
            else
            {
                var getAdminEmail = _context.Accounts
                  .Include(a => a.role)
                  .Where(a => a.role.role_name.Equals("Admin"))
                  .Select(a => a.account_email)
                  .FirstOrDefault();

                var messageForAdmin = "You received this email because an order with id: " + id + " just confirmed by manager named:";

                SendEmail(getUserEmail, "", messageForCustomer);
                SendEmail(getAdminEmail, confimer, messageForAdmin);
            }

            return Ok(new
            {
                Message = "Confirm Order Succeed"
            });
        }

        //[HttpPost("Reorder")]
        //public async Task<IActionResult> Reorder([FromBody] Order order)
        //{
        //    if (order == null)
        //        return BadRequest(new { Message = "Order is not found" });

        //    var orders = _context.Orders
        //        .Where(o => o.order_id == order.order_id)
        //        .FirstOrDefault();

        //    var order_detail = _context.Order_details
        //        .Where(od => od.od_order_id == orders.order_id)
        //        .ToList();

        //    if(order_detail == null)
        //        return BadRequest(new { Message = "Order is not found" });

        //    var phone = CheckPhoneValid(order.order_phone);
        //    if (!string.IsNullOrEmpty(phone))
        //        return BadRequest(new { Message = phone.ToString() });

        //    foreach (var item in order_detail)
        //    {
        //        var product = await _context.Products.FirstOrDefaultAsync(p => p.product_id == item.od_product_id);

        //        if (item.od_quantity > product.product_quantity_stock)
        //            return BadRequest(new { Message = "Product named " + product.product_name + " is out of stock!" });
        //    }

        //    Order newOrder = new Order();
        //    Order_Detail newOrder_Detail = new Order_Detail();

        //    newOrder.order_date = DateTime.Now;
        //    newOrder.order_phone = order.order_phone;
        //    newOrder.order_address = order.order_address;
        //    newOrder.order_payment = order.order_payment;
        //    newOrder.order_note = order.order_note;
        //    newOrder.order_total_price = order.order_total_price;



        //}

        [HttpPut("RejectOrder")]
        [Authorize(Policy = "AdminOrManagerPolicy")]
        public async Task<IActionResult> RejectOrder(int id, string rejector)
        {
            var order = await _context.Orders.FindAsync(id);
            var account =  _context.Accounts
                .Where(a => a.account_id == order.o_account_id)
                .FirstOrDefault();

            if (order == null)
                return NotFound(new { Message = "Order is Not Found!" });

            order.delivery_date = DateTime.Now;
            order.order_status = order_status.Rejected.ToString();

            account.account_rejected_times += 1;

            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            _context.Entry(account).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var getcanceler = _context.Accounts
                .Include(a => a.role)
                .Where(a => a.account_username.Equals(rejector))
                .Select(a => a.role.role_name)
                .FirstOrDefault();

            var getUsername = _context.Accounts
                 .Where(a => a.account_id == order.o_account_id)
                 .Select(a => a.account_username)
                 .FirstOrDefault();

            var message = "You received this email to confirm the order with id: " + id + " of customer named: " + getUsername + " just rejected";

            if (getcanceler.Equals("Admin"))
            {
                var getManagerEmail = _context.Accounts
                  .Include(a => a.role)
                  .Where(a => a.role.role_name.Equals("Manager"))
                  .Select(a => a.account_email)
                  .FirstOrDefault();

                SendEmail(getManagerEmail, "", message);
            }
            else
            {
                var getAdminEmail = _context.Accounts
                  .Include(a => a.role)
                  .Where(a => a.role.role_name.Equals("Admin"))
                  .Select(a => a.account_email)
                  .FirstOrDefault();

                SendEmail(getAdminEmail, "", message);
            }

            return Ok(new
            {
                Message = "Confirm Order Rejected Succeed"
            });
        }

        [HttpPut("ReturnOrder")]
        [Authorize(Policy = "AdminOrManagerPolicy")]
        public async Task<IActionResult> ReturnOrder(int id, string returner)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
                return NotFound(new { Message = "Order is Not Found!" });

            order.delivery_date = DateTime.Now;
            order.order_status = order_status.Returned.ToString();

            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var getcanceler = _context.Accounts
                .Include(a => a.role)
                .Where(a => a.account_username.Equals(returner))
                .Select(a => a.role.role_name)
                .FirstOrDefault();

            var getUsername = _context.Accounts
                 .Where(a => a.account_id == order.o_account_id)
                 .Select(a => a.account_username)
                 .FirstOrDefault();

            var message = "You received this email to confirm the order with id: " + id + " of customer named: " + getUsername + " just returned";

            if (getcanceler.Equals("Admin"))
            {
                var getManagerEmail = _context.Accounts
                  .Include(a => a.role)
                  .Where(a => a.role.role_name.Equals("Manager"))
                  .Select(a => a.account_email)
                  .FirstOrDefault();

                SendEmail(getManagerEmail, "", message);
            }
            else
            {
                var getAdminEmail = _context.Accounts
                  .Include(a => a.role)
                  .Where(a => a.role.role_name.Equals("Admin"))
                  .Select(a => a.account_email)
                  .FirstOrDefault();

                SendEmail(getAdminEmail, "", message);
            }

            return Ok(new
            {
                Message = "Confirm Order Returned Succeed"
            });
        }

        [HttpPut("DeliveredOrder")]
        [Authorize(Policy = "AdminOrManagerPolicy")]
        public async Task<IActionResult> DeliveredOrder(int id, string deliver)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
                return NotFound(new { Message = "Order is Not Found!" });

            if (deliver == null)
                return BadRequest();

            order.delivery_date = DateTime.Now;
            order.order_status = order_status.Delivered.ToString();

            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var getcanceler = _context.Accounts
                .Include(a => a.role)
                .Where(a => a.account_username.Equals(deliver))
                .Select(a => a.role.role_name)
                .FirstOrDefault();

            var getUsername = _context.Accounts
                 .Where(a => a.account_id == order.o_account_id)
                 .Select(a => a.account_username)
                 .FirstOrDefault();

            var message = "You received this email to confirm the order with id: " + id + " of customer named: " + getUsername + " just returned";

            if (getcanceler.Equals("Admin"))
            {
                var getManagerEmail = _context.Accounts
                  .Include(a => a.role)
                  .Where(a => a.role.role_name.Equals("Manager"))
                  .Select(a => a.account_email)
                  .FirstOrDefault();

                SendEmail(getManagerEmail, "", message);
            }
            else
            {
                var getAdminEmail = _context.Accounts
                  .Include(a => a.role)
                  .Where(a => a.role.role_name.Equals("Admin"))
                  .Select(a => a.account_email)
                  .FirstOrDefault();

                SendEmail(getAdminEmail, "", message);
            }

            return Ok(new
            {
                Message = "Confirm Order Delivered Succeed"
            });
        }

        private void SendEmail(string email, string adder, string message)
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var emailToken = Convert.ToBase64String(tokenBytes);

            string from = _configuration["EmailSettings:From"];
            var emailBody = EmailBody.OrderNotifyEmail(adder, message);
            var emailModel = new EmailModel(email, "Order Notify Email", emailBody);
            _emailService.SendEmail(emailModel);
        }

        [HttpPut("CancelOrder")]
        [Authorize(Policy = "Customer")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            var account = _context.Accounts
                .Where(a => a.account_id == order.o_account_id)
                .FirstOrDefault();

            if (order == null)
                return NotFound(new { Message = "Order is Not Found!" });

            order.delivery_date = DateTime.Now;
            order.order_status = order_status.Cancelled.ToString();

            account.account_rejected_times += 1;

            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            _context.Entry(account).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var order_detail = _context.Order_details
                .Where(od => od.od_order_id == order.order_id)
                .ToList();

            foreach(var od in order_detail)
            {
                var getProductByID = _context.Products
                    .Where(p => p.product_id == od.od_product_id)
                    .FirstOrDefault();

                getProductByID.product_quantity_stock += od.od_quantity;

                _context.Entry(getProductByID).State = EntityState.Modified;
                _context.SaveChanges();
            }

            var getAdminEmail = _context.Accounts
              .Include(a => a.role)
              .Where(a => a.role.role_name.Equals("Admin"))
              .Select(a => a.account_email)
              .FirstOrDefault();

            var getManagerEmail = _context.Accounts
              .Include(a => a.role)
              .Where(a => a.role.role_name.Equals("Manager"))
              .Select(a => a.account_email)
              .FirstOrDefault();

            var getUser = _context.Accounts
                .Where(a => a.account_id == order.o_account_id)
                .Select(a => a.account_username)
                .FirstOrDefault();

            var message = "You received this email because an order with id: " + id + " just canceled by account name: ";

            SendEmail(getManagerEmail, getUser, message);
            SendEmail(getAdminEmail, getUser, message);

            return Ok(new
            {
                Message = "Cancel Order Succeed"
            });
        }


        [HttpPut("CancelOrderByAdmin")]
        [Authorize(Policy = "AdminOrManagerPolicy")]
        public async Task<IActionResult> CancelOrderByAdmin(int id, string content, string canceller)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
                return NotFound(new { Message = "Order is Not Found!" });

            order.delivery_date = DateTime.Now;
            order.order_status = order_status.Cancelled.ToString();

            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var order_detail = _context.Order_details
                .Where(od => od.od_order_id == order.order_id)
                .ToList();

            foreach (var od in order_detail)
            {
                var getProductByID = _context.Products
                    .Where(p => p.product_id == od.od_product_id)
                    .FirstOrDefault();

                getProductByID.product_quantity_stock += od.od_quantity;

                _context.Entry(getProductByID).State = EntityState.Modified;
                _context.SaveChanges();
            }

            var getcanceler = _context.Accounts
                .Include(a => a.role)
                .Where(a => a.account_username.Equals(canceller))
                .Select(a => a.role.role_name)
                .FirstOrDefault();

            var getUserEmail = _context.Accounts
                 .Where(a => a.account_id == order.o_account_id)
                 .Select(a => a.account_email)
                 .FirstOrDefault();

            if (getcanceler.Equals("Admin"))
            {
                SendEmail(getUserEmail, "", content);
            }
            else
            {
                var getAdminEmail = _context.Accounts
                  .Include(a => a.role)
                  .Where(a => a.role.role_name.Equals("Admin"))
                  .Select(a => a.account_email)
                  .FirstOrDefault();

                var message = "You received this email because an order with id: " + id + " just canceled by manager named:";

                SendEmail(getUserEmail, "", content);
                SendEmail(getAdminEmail, canceller, message);
            }

            return Ok(new
            {
                Message = "Cancel Order Succeed"
            });
        }

        [HttpGet("search")]
        public async Task<IActionResult> search(string key)
        {
            List<Order> order = new List<Order>();

            if(int.TryParse(key, out int result))
            {
               order = _context.Orders.Where(p => p.order_date.Month.Equals(result)).ToList();
                return Ok(order);
            }

            order = _context.Orders.Where(p => p.order_status.Contains(key)).ToList();

            if (order == null)
                return NotFound(new { Message = "Order not found!" });

            return Ok(order);
        }

        private string CheckDeliveryDate(int id, string delivery_date)
        {
            var order = _context.Orders
                .Where(o => o.order_id == id)
                .FirstOrDefault();

            StringBuilder sb = new StringBuilder();
            if (DateTime.Parse(delivery_date) < order.order_date)
                sb.Append("Delivery date cannot smaller than order date!");

            return sb.ToString();
        }

        //[HttpPost("AddData")]
        //public async Task<IActionResult> AddData(string month)
        //{
        //    var count = 74;
        //    var pro = 375;

        //    for(var i = 43; i <= count; i++)
        //    {
        //        Order order = new Order();

        //        order.order_date = DateTime.Parse(month);
        //        order.delivery_date = DateTime.Parse(month);
        //        order.order_quantity = 1;
        //        order.order_address = "Order Address " + i.ToString();

        //        if (i < 10)
        //        {
        //            order.order_phone = "094512694" + i.ToString();
        //            order.o_account_id = i;
        //        }

        //        if (i > 9 && i < 100)
        //        {
        //            order.order_phone = "09451269" + i.ToString();
        //            order.o_account_id = i;
        //        }

        //        if (i > 99 && i < 1000)
        //        {
        //            order.order_phone = "0945126" + i.ToString();
        //            order.o_account_id = i;
        //        }

        //        order.order_status = order_status.Delivered.ToString();
        //        order.order_payment = "Cash On Delivery";
        //        order.order_total_price = 205;

        //        await _context.Orders.AddAsync(order);
        //        await _context.SaveChangesAsync();

        //        var order_id = _context.Orders.Max(a => a.order_id);

        //        Order_Detail od = new Order_Detail();

        //        od.od_order_id = order_id;
        //        od.od_quantity = 1;
        //        od.od_product_price = 200;
                
        //        for(var a = 118; a <= 235; a++)
        //        {
        //            od.od_product_id = a;

        //            var product = _context.Products
        //                .Where(p => p.product_id == a)
        //                .FirstOrDefault();

        //            product.product_quantity_stock -= 1;


        //            _context.Products.Entry(product).State = EntityState.Modified;
        //            await _context.SaveChangesAsync();
        //        }

        //        await _context.Order_details.AddAsync(od);
        //        await _context.SaveChangesAsync();
        //    }

        //    return Ok();
        //}
    }
}
