using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Text;
using web_project_BE.Data;
using web_project_BE.Models;
using Microsoft.EntityFrameworkCore;
using MailKit.Search;
using Newtonsoft.Json;

namespace web_project_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticalController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StatisticalController(ApplicationDbContext context) => _context = context;

        [HttpGet("GetTotalOrderInYear")]
        public async Task<IActionResult> GetTotalOrderInYear(int year)
        {
            if (year == 0)
            {
                var date = DateTime.Now;

                var currentYear = date.Year;

                var countOrder = _context.Orders
                    .Where(o => o.order_date.Year == currentYear)
                    .GroupBy(o => o.order_date.Month)
                    .Select(g => new
                    {
                        month = g.Key,
                        total_orders = g.Count(),

                        canceled_order = _context.Orders
                            .Where(o => o.order_date.Month == g.Key && o.order_status.Equals(order_status.Cancelled.ToString()))
                            .Count(),

                        Delivered_order = _context.Orders
                            .Where(o => o.order_date.Month == g.Key && o.order_status.Equals(order_status.Delivered.ToString()))
                            .Count(),

                        Pending_order = _context.Orders
                            .Where(o => o.order_date.Month == g.Key && o.order_status.Equals(order_status.Pending.ToString()))
                            .Count(),

                        Returned_order = _context.Orders
                            .Where(o => o.order_date.Month == g.Key && o.order_status.Equals(order_status.Returned.ToString()))
                            .Count(),

                        Rejected_order = _context.Orders
                            .Where(o => o.order_date.Month == g.Key && o.order_status.Equals(order_status.Rejected.ToString()))
                            .Count(),

                        Awaiting_Pickup_order = _context.Orders
                            .Where(o => o.order_date.Month == g.Key && o.order_status.Equals(order_status.Awaiting_Pickup.ToString()))
                            .Count(),
                    })
                    .ToList();

                return Ok(countOrder);
            }
            else
            {
                var countOrder = _context.Orders
                    .Where(o => o.order_date.Year == year)
                    .GroupBy(o => o.order_date.Month)
                    .Select(g => new
                    {
                        month = g.Key,
                        total_orders = g.Count(),

                        canceled_order = _context.Orders
                            .Where(o => o.order_date.Month == g.Key && o.order_status.Equals(order_status.Cancelled.ToString()))
                            .Count(),

                        Delivered_order = _context.Orders
                            .Where(o => o.order_date.Month == g.Key && o.order_status.Equals(order_status.Delivered.ToString()))
                            .Count(),

                        Pending_order = _context.Orders
                            .Where(o => o.order_date.Month == g.Key && o.order_status.Equals(order_status.Pending.ToString()))
                            .Count(),

                        Returned_order = _context.Orders
                            .Where(o => o.order_date.Month == g.Key && o.order_status.Equals(order_status.Returned.ToString()))
                            .Count(),

                        Rejected_order = _context.Orders
                            .Where(o => o.order_date.Month == g.Key && o.order_status.Equals(order_status.Rejected.ToString()))
                            .Count(),

                        Awaiting_Pickup_order = _context.Orders
                            .Where(o => o.order_date.Month == g.Key && o.order_status.Equals(order_status.Awaiting_Pickup.ToString()))
                            .Count(),
                    })
                    .ToList();

                return Ok(countOrder);
            }
        }

        [HttpGet("TotalRevenueInMonth")]
        public async Task<IActionResult> TotalRevenueInMonth(int month, int year)
        {
            Dictionary<string, double> lstPrice = new Dictionary<string, double>();
            List<object> st = new List<object>();

            double totalPrice = 0;
            double totalPricePreviousMonth = 0;
            double percentageIncrease = 0;

            if(month == 0 && year == 0)
            {
                var date = DateTime.Now;
                var currentMonth = date.Month;
                var currentYear = date.Year;

                var countOrder = _context.Orders
                    .Where(o => o.order_date.Month == currentMonth && o.order_date.Year == currentYear && o.order_status.Equals(order_status.Delivered.ToString()))
                    .ToList();

                var orderPreviousMonth = _context.Orders
                    .Where(o => o.order_date.Month == (currentMonth - 1) && o.order_date.Year == currentYear && o.order_status.Equals(order_status.Delivered.ToString()))
                    .ToList();

                foreach (var order in countOrder)
                {
                    var Price = _context.Order_details
                        .Where(od => od.od_order_id == order.order_id)
                        .Select(od => new
                        {
                            totalPrice = od.od_product_price
                        })
                        .ToList();

                    foreach (var p in Price)
                    {
                        totalPrice += (double)p.totalPrice;
                    }
                }

                foreach (var order in orderPreviousMonth)
                {
                    var Price = _context.Order_details
                        .Where(od => od.od_order_id == order.order_id)
                        .Select(od => new
                        {
                            totalPrice = od.od_product_price
                        })
                        .ToList();

                    foreach (var p in Price)
                    {
                        totalPricePreviousMonth += (double)p.totalPrice;
                    }
                }

                if (totalPricePreviousMonth != 0 && totalPrice != 0)
                {
                    percentageIncrease = Math.Min(((double)(totalPrice - totalPricePreviousMonth) / totalPricePreviousMonth) * 100, 100);
                }
                else if (totalPrice == 0)
                {
                    percentageIncrease = 0;
                }
                else
                {
                    percentageIncrease = 100;
                }

                string formattedPercentage = percentageIncrease.ToString("0.0");

                lstPrice.Add("totalPrice", totalPrice);
                lstPrice.Add("percentageIncrease", double.Parse(formattedPercentage));

                st.Add(lstPrice);

                return Ok(st);
            }
            else
            {
                if (!string.IsNullOrEmpty(CheckMonthValid(month)))
                    return BadRequest(new { Message = CheckMonthValid(month).ToString() });

                if (!string.IsNullOrEmpty(CheckYearValid(year)))
                    return BadRequest(new { Message = CheckYearValid(year).ToString() });

                var countOrder = _context.Orders
                    .Where(o => o.order_date.Month == month && o.order_date.Year == year && o.order_status.Equals(order_status.Delivered.ToString()))
                    .ToList();

                var orderPreviousMonth = _context.Orders
                    .Where(o => o.order_date.Month == (month - 1) && o.order_date.Year == year && o.order_status.Equals(order_status.Delivered.ToString()))
                    .ToList();

                foreach (var order in countOrder)
                {
                    var Price = _context.Order_details
                        .Where(od => od.od_order_id == order.order_id)
                        .Select(od => new
                        {
                            totalPrice = od.od_product_price
                        })
                        .ToList();

                    foreach (var p in Price)
                    {
                        totalPrice += (double)p.totalPrice;
                    }
                }

                foreach (var order in orderPreviousMonth)
                {
                    var Price = _context.Order_details
                        .Where(od => od.od_order_id == order.order_id)
                        .Select(od => new
                        {
                            totalPrice = od.od_product_price
                        })
                        .ToList();

                    foreach (var p in Price)
                    {
                        totalPricePreviousMonth += (double)p.totalPrice;
                    }
                }

                if (totalPricePreviousMonth != 0 && totalPrice != 0)
                {
                    percentageIncrease = Math.Min(((double)(totalPrice - totalPricePreviousMonth) / totalPricePreviousMonth) * 100, 100);
                }
                else if (totalPrice == 0)
                {
                    percentageIncrease = 0;
                }
                else
                {
                    percentageIncrease = 100;
                }

                string formattedPercentage = percentageIncrease.ToString("0.0");

                lstPrice.Add("totalPrice", totalPrice);
                lstPrice.Add("percentageIncrease", double.Parse(formattedPercentage));

                st.Add(lstPrice);

                return Ok(st);
            }
        }

        [HttpGet("BestSale")]
        public async Task<IActionResult> BestSeller(int year)
        {
            List<object> list = new List<object>();
            var sum = 0;

            var name = "";
            var product_price = "";
            var product_image = "";
            var category = "";
            var supplier = "";

            if(year == 0)
            {
                var date = DateTime.Now;
                var currentYear = date.Year;

                var getOrderInYear = _context.Order_details
                    .Include(o => o.order)
                    .Where(o => o.order.order_date.Year == currentYear && o.order.order_status.Equals(order_status.Delivered.ToString()))
                    .ToList();

                var product_name = _context.Products
                    .Select(p => new
                    {
                        product_id = p.product_id,
                        product_name = p.product_name,
                    })
                    .GroupBy(p => p.product_name)
                    .ToList();

                foreach (var product in product_name)
                {
                    sum = 0;
                    foreach (var item in product)
                    {
                        name = _context.Products
                            .Where(p => p.product_id == item.product_id)
                            .Select(p => p.product_name)
                            .FirstOrDefault();

                        product_price = _context.Products
                            .Where(p => p.product_id == item.product_id)
                            .Select(p => p.product_sell_price.ToString())
                            .FirstOrDefault();

                        product_image = _context.Products
                            .Where(p => p.product_id == item.product_id)
                            .Select(p => p.product_image)
                            .FirstOrDefault();

                        category = _context.Products
                            .Include(p => p.category)
                            .Where(p => p.product_id == item.product_id)
                            .Select(p => p.category.category_name)
                            .FirstOrDefault();

                        supplier = _context.Products
                            .Include(p => p.supplier)
                            .Where(p => p.product_id == item.product_id)
                            .Select(p => p.supplier.supplier_name)
                            .FirstOrDefault();

                        foreach (var qty in getOrderInYear)
                        {
                            if (qty.od_product_id == item.product_id)
                            {
                                sum += qty.od_quantity;
                            }
                        }
                    }

                    if (sum != 0)
                    {
                        var getInfor = _context.Products
                            .Select(p => new
                            {
                                total_sale = sum,
                                product_name = name,
                                product_image = product_image,
                                product_price = product_price,
                                category_name = category,
                                supplier_name = supplier,

                            })
                            .FirstOrDefault();
                        list.Add(getInfor);
                    }
                }

                return Ok(list);
            }
            else
            {
                var getOrderInYear = _context.Order_details
                    .Include(o => o.order)
                    .Where(o => o.order.order_date.Year == year && o.order.order_status.Equals(order_status.Delivered.ToString()))
                    .ToList();

                var product_name = _context.Products
                    .Select(p => new
                    {
                        product_id = p.product_id,
                        product_name = p.product_name,
                    })
                    .GroupBy(p => p.product_name)
                    .ToList();

                foreach (var product in product_name)
                {
                    sum = 0;
                    foreach (var item in product)
                    {
                        name = _context.Products
                            .Where(p => p.product_id == item.product_id)
                            .Select(p => p.product_name)
                            .FirstOrDefault();

                        product_price = _context.Products
                            .Where(p => p.product_id == item.product_id)
                            .Select(p => p.product_sell_price.ToString())
                            .FirstOrDefault();

                        product_image = _context.Products
                            .Where(p => p.product_id == item.product_id)
                            .Select(p => p.product_image)
                            .FirstOrDefault();

                        category = _context.Products
                            .Include(p => p.category)
                            .Where(p => p.product_id == item.product_id)
                            .Select(p => p.category.category_name)
                            .FirstOrDefault();

                        supplier = _context.Products
                            .Include(p => p.supplier)
                            .Where(p => p.product_id == item.product_id)
                            .Select(p => p.supplier.supplier_name)
                            .FirstOrDefault();

                        foreach (var qty in getOrderInYear)
                        {
                            if (qty.od_product_id == item.product_id)
                            {
                                sum += qty.od_quantity;
                            }
                        }
                    }

                    if (sum != 0)
                    {
                        var getInfor = _context.Products
                            .Select(p => new
                            {
                                total_sale = sum,
                                product_name = name,
                                product_image = product_image,
                                product_price = product_price,
                                category_name = category,
                                supplier_name = supplier,

                            })
                            .FirstOrDefault();
                        list.Add(getInfor);
                    }
                }

                return Ok(list);
            }
        }


        [HttpGet("BestSeller")]
        public async Task<IActionResult> Top4BestSeller(int year)
        {
            List<object> list = new List<object>();
            var sum = 0;

            var id = 0;
            var name = "";
            var product_price = "";
            var product_image = "";
            var category = "";
            var supplier = "";

            if(year == 0)
            {
                var date = DateTime.Now;
                var currentYear = date.Year; 

                var getOrderInYear = _context.Order_details
                    .Include(o => o.order)
                    .Where(o => o.order.order_date.Year == currentYear && o.order.order_status.Equals(order_status.Delivered.ToString()))
                    .ToList();

                var product_name = _context.Products
                    .Select(p => new
                    {
                        product_id = p.product_id,
                        product_name = p.product_name,
                    })
                    .GroupBy(p => p.product_name)
                    .ToList();

                foreach (var product in product_name)
                {
                    sum = 0;
                    foreach (var item in product)
                    {
                        id = item.product_id;

                        name = _context.Products
                            .Where(p => p.product_id == item.product_id)
                            .Select(p => p.product_name)
                            .FirstOrDefault();

                        product_price = _context.Products
                            .Where(p => p.product_id == item.product_id)
                            .Select(p => p.product_sell_price.ToString())
                            .FirstOrDefault();

                        product_image = _context.Products
                            .Where(p => p.product_id == item.product_id)
                            .Select(p => p.product_image)
                            .FirstOrDefault();

                        category = _context.Products
                            .Include(p => p.category)
                            .Where(p => p.product_id == item.product_id)
                            .Select(p => p.category.category_name)
                            .FirstOrDefault();

                        supplier = _context.Products
                            .Include(p => p.supplier)
                            .Where(p => p.product_id == item.product_id)
                            .Select(p => p.supplier.supplier_name)
                            .FirstOrDefault();

                        foreach (var qty in getOrderInYear)
                        {
                            if (qty.od_product_id == item.product_id)
                            {
                                sum += qty.od_quantity;
                            }
                        }
                    }

                    if (sum != 0)
                    {
                        var getInfor = _context.Products
                            .Select(p => new
                            {
                                id = id,
                                total_sale = sum,
                                product_name = name,
                                product_image = product_image,
                                product_price = product_price,
                                category_name = category,
                                supplier_name = supplier,

                            })
                            .FirstOrDefault();
                        list.Add(getInfor);
                    }
                }

                var top4 = list.Cast<dynamic>()
                   .OrderByDescending(item => item.total_sale)
                   .Take(4)
                   .ToList();

                return Ok(top4);
            }
            else
            {
                var getOrderInYear = _context.Order_details
                    .Include(o => o.order)
                    .Where(o => o.order.order_date.Year == year && o.order.order_status.Equals(order_status.Delivered.ToString()))
                    .ToList();

                var product_name = _context.Products
                    .Select(p => new
                    {
                        product_id = p.product_id,
                        product_name = p.product_name,
                    })
                    .GroupBy(p => p.product_name)
                    .ToList();

                foreach (var product in product_name)
                {
                    sum = 0;
                    foreach (var item in product)
                    {
                        id = item.product_id;

                        name = _context.Products
                            .Where(p => p.product_id == item.product_id)
                            .Select(p => p.product_name)
                            .FirstOrDefault();

                        product_price = _context.Products
                            .Where(p => p.product_id == item.product_id)
                            .Select(p => p.product_sell_price.ToString())
                            .FirstOrDefault();

                        product_image = _context.Products
                            .Where(p => p.product_id == item.product_id)
                            .Select(p => p.product_image)
                            .FirstOrDefault();

                        category = _context.Products
                            .Include(p => p.category)
                            .Where(p => p.product_id == item.product_id)
                            .Select(p => p.category.category_name)
                            .FirstOrDefault();

                        supplier = _context.Products
                            .Include(p => p.supplier)
                            .Where(p => p.product_id == item.product_id)
                            .Select(p => p.supplier.supplier_name)
                            .FirstOrDefault();

                        foreach (var qty in getOrderInYear)
                        {
                            if (qty.od_product_id == item.product_id)
                            {
                                sum += qty.od_quantity;
                            }
                        }
                    }

                    if (sum != 0)
                    {
                        var getInfor = _context.Products
                            .Select(p => new
                            {
                                id = id,
                                total_sale = sum,
                                product_name = name,
                                product_image = product_image,
                                product_price = product_price,
                                category_name = category,
                                supplier_name = supplier,

                            })
                            .FirstOrDefault();
                        list.Add(getInfor);
                    }
                }

                var top4 = list.Cast<dynamic>()
                   .OrderByDescending(item => item.total_sale)
                   .Take(4)
                   .ToList();

                return Ok(top4);
            }
        }

        private string CheckMonthValid(int month)
        {
            StringBuilder sb = new StringBuilder();
            if (!(Regex.IsMatch(month.ToString(), "^(?:[1-9]|1[0-2])$")))
                sb.Append("Please enter month from 1 to 12!");

            return sb.ToString();
        }

        private string CheckYearValid(int year)
        {
            StringBuilder sb = new StringBuilder();
            if (!(Regex.IsMatch(year.ToString(), "^\\d{4}$")))
                sb.Append("Please enter valid year");

            return sb.ToString();
        }
    }
}
