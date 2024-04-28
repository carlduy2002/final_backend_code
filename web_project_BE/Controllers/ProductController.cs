using Azure.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Scripting.Runtime;
using Neo4jClient.DataAnnotations.Cypher.Functions;
using System.Text;
using System.Text.RegularExpressions;
using web_project_BE.Data;
using web_project_BE.Models;
using System;
using static IronPython.Modules.PythonIterTools;
using System.Drawing;
using System.Security.Cryptography;
using web_project_BE.Helper;
using IEmailService = web_project_BE.UtilityServices.IEmailService;
using static IronPython.Modules._ast;

namespace web_project_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
 

        public ProductController(ApplicationDbContext context, IWebHostEnvironment env, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _env = env;
            _configuration = configuration;
            _emailService = emailService;
        }

        //View All Product
        [HttpGet]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> GetAllProduct()
        {
            var lstProduct = _context.Products
                .Include(p => p.category)
                .Include(p => p.supplier)
                .Include(p => p.size)
                .Select(p => new 
                {
                    product_id = p.product_id,
                    product_name = p.product_name,
                    product_quantity_stock = p.product_quantity_stock,
                    product_original_price = p.product_original_price,
                    product_sell_price = p.product_sell_price,
                    product_description = p.product_description,
                    product_status = p.product_status,
                    p_size_id = p.p_size_id,
                    p_category_id = p.p_category_id,
                    p_supplier_id = p.p_supplier_id,
                    size_number = p.size.size_number,
                    category_name = p.category.category_name,
                    supplier_name = p.supplier.supplier_name,
                    product_image = p.product_image,
                    product_import_date = p.product_import_date,

                })
                .ToList();

            return Ok(lstProduct);
        }

        //view product by manager
        [HttpGet("get_all_product_by_manager")]
        [Authorize(Policy = "Manager")]
        public async Task<IActionResult> GetAllProductByManager()
        {
            var lstProduct = _context.Products
                .Include(p => p.category)
                .Include(p => p.supplier)
                .Include(p => p.size)
                .Select(p => new
                {
                    product_id = p.product_id,
                    product_name = p.product_name,
                    product_quantity_stock = p.product_quantity_stock,
                    product_original_price = p.product_original_price,
                    product_sell_price = p.product_sell_price,
                    product_description = p.product_description,
                    product_status = p.product_status,
                    p_size_id = p.p_size_id,
                    p_category_id = p.p_category_id,
                    p_supplier_id = p.p_supplier_id,
                    size_number = p.size.size_number,
                    category_name = p.category.category_name,
                    supplier_name = p.supplier.supplier_name,
                    product_image = p.product_image,
                    product_import_date = p.product_import_date,

                })
                .Where(p => p.product_status.Equals(product_status.Yes.ToString()) 
                    || p.product_status.Equals(product_status.New.ToString()) 
                    || p.product_status.Equals(product_status.Waiting_deleted.ToString()))
                .ToList();

            return Ok(lstProduct);
        }

        [HttpGet("GetProductWithStatus")]
        public async Task<IActionResult> GetProductWithStatus()
        {
            var lstProduct = _context.Products
                .Include(p => p.category)
                .Include(p => p.supplier)
                .Include(p => p.size)
                .Select(p => new 
                {
                    product_id = p.product_id,
                    product_name = p.product_name,
                    product_quantity_stock = p.product_quantity_stock,
                    product_original_price = p.product_original_price,
                    product_sell_price = p.product_sell_price,
                    product_description = p.product_description,
                    product_status = p.product_status,
                    p_size_id = p.p_size_id,
                    p_category_id = p.p_category_id,
                    p_supplier_id = p.p_supplier_id,
                    size_number = p.size.size_number,
                    category_name = p.category.category_name,
                    supplier_name = p.supplier.supplier_name,
                    product_image = p.product_image,
                    product_import_date = p.product_import_date,
                })
                .Where(p => p.product_status == product_status.Yes.ToString())
                .GroupBy(p => p.product_name)
                .Select(g => g.First())
                .ToList();

            return Ok(lstProduct);
        }

        //Get Product By Image
        [HttpGet("product_image")]
        public async Task<IActionResult> GetProductByImage(string product_image)
        {
            var lstProduct = _context.Products
                .Include(p => p.category)
                .Include(p => p.supplier)
                .Include(p => p.size)
                .Select(p => new 
                {
                    product_id = p.product_id,
                    product_name = p.product_name,
                    product_quantity_stock = p.product_quantity_stock,
                    product_original_price = p.product_original_price,
                    product_sell_price = p.product_sell_price,
                    product_description = p.product_description,
                    product_status = p.product_status,
                    p_category_id = p.p_category_id,
                    category_name = p.category.category_name,
                    p_supplier_id = p.p_supplier_id,
                    supplier_name = p.supplier.supplier_name,
                    p_size_id = p.p_size_id,
                    size_number = p.size.size_number,
                    product_image = p.product_image
                }).Where(p => p.product_image.Equals(product_image)).FirstOrDefault();

            //if (lstProduct == null)
            //    return NotFound(new { Message = "Product with the id " + lstProduct.product_id + " is not found!" });
            List<object> product = new List<object>();
            product.Add(lstProduct);

            return Ok(product);
        }


        //Get Product By Id
        [HttpGet("product_id")]
        public async Task<IActionResult> GetProductById(int product_id)
        {
            var lstProduct = _context.Products
                .Include(p => p.category)
                .Include(p => p.supplier)
                .Include(p => p.size)
                .Select(p => new
                {
                    product_id = p.product_id,
                    product_name = p.product_name,
                    product_quantity_stock = p.product_quantity_stock,
                    product_original_price = p.product_original_price,
                    product_sell_price = p.product_sell_price,
                    product_description = p.product_description,
                    product_status = p.product_status,
                    p_category_id = p.p_category_id,
                    category_name = p.category.category_name,
                    p_supplier_id = p.p_supplier_id,
                    supplier_name = p.supplier.supplier_name,
                    p_size_id = p.p_size_id,
                    size_number = p.size.size_number,
                    product_image = p.product_image
                }).Where(p => p.product_id == product_id).FirstOrDefault();

            if (lstProduct == null)
                return NotFound(new { Message = "Product with the id " + product_id  + " is not found!" });

            List<object> product = new List<object>();
            product.Add(lstProduct);

            return Ok(product);
        }


        [HttpGet("{product_name}/size")]
        public async Task<IActionResult> GetSizeOfProduct(string product_name)
        {
            var lstSizes = _context.Sizes
            .Join(_context.Products.Where(p => p.product_name == product_name),
                size => size.size_id,
                product => product.p_size_id,
                (size, product) => new { SizeNumber = size.size_number, ProductName = product.product_name })
            .ToList();

            if (lstSizes == null)
                return NotFound();

            return Ok(lstSizes);
        }


        [HttpPost]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> AddNewProduct(List<IFormFile> files)
        {
            var product_name = Request.Form["product_name"];
            var product_quantity = Request.Form["product_quantity"];
            var product_originalPrice = Request.Form["product_originalPrice"];
            var product_sellPrice = Request.Form["product_sellPrice"];
            var product_description = Request.Form["product_description"];
            var p_category_id = Request.Form["p_category_id"];
            var p_supplier_id = Request.Form["p_supplier_id"];
            var p_size_id = Request.Form["p_size_id"];

            if (files == null || files.Count == 0)
                return BadRequest();

            var getProduct = _context.Products
                .Select(p => new Product
                {
                    p_size_id = p.p_size_id,
                    product_name = p.product_name,
                })
                .ToList();

            foreach (var i in getProduct)
            {
                if (i.p_size_id.Equals(int.Parse(p_size_id)) && i.product_name.Equals(product_name))
                {
                    return BadRequest(new { Message = "The Product Already Exist" });
                }
            }

            Product product = new Product();
            product.product_name = product_name;
            product.product_quantity_stock = int.Parse(product_quantity);
            product.product_original_price = double.Parse(product_originalPrice);
            product.product_sell_price = double.Parse(product_sellPrice);
            product.product_description = product_description;
            product.p_category_id = int.Parse(p_category_id);
            product.p_supplier_id = int.Parse(p_supplier_id);
            product.p_size_id = int.Parse(p_size_id);
            product.product_import_date = DateTime.Now;
            product.product_status = product_status.Yes.ToString();
            product.product_image = files[0].FileName.ToString();

            var checkInfo = CheckValidateProductInfo(product);
            if (!string.IsNullOrEmpty(checkInfo))
                return BadRequest(new { Message = checkInfo.ToString() });

             await _context.Products.AddAsync(product);
             await _context.SaveChangesAsync();

            int maxProductId = _context.Products.Max(p => p.product_id);

            SaveFileAsync(files[0], "image");
            foreach (var file in files)
            {
                if(file.Length > 0)
                {
                    if (IsImageFile(file.FileName))
                    {
                        SaveFileAsync(file, "Photos");
                        Models.Image image = new Models.Image();
                        image.image_uri = file.FileName;
                        image.i_product_id = maxProductId;

                         await _context.Images.AddAsync(image);
                         await _context.SaveChangesAsync();
                    }
                    else
                    {
                        return BadRequest(new { Message = "File Format Not Supported" });
                    }
                }
            }

            return Ok(new
            {
                Message = "Add Product Succeed"
            });
        }

        [HttpPost("add_new_product_by_manager")]
        [Authorize(Policy = "Manager")]
        public async Task<IActionResult> AddNewProductByManager(List<IFormFile> files)
        {
            var product_name = Request.Form["product_name"];
            var product_quantity = Request.Form["product_quantity"];
            var product_originalPrice = Request.Form["product_originalPrice"];
            var product_sellPrice = Request.Form["product_sellPrice"];
            var product_description = Request.Form["product_description"];
            var p_category_id = Request.Form["p_category_id"];
            var p_supplier_id = Request.Form["p_supplier_id"];
            var p_size_id = Request.Form["p_size_id"];
            var adder = Request.Form["adder"];

            if (files == null || files.Count == 0)
                return BadRequest();

            var getProduct = _context.Products
                .Select(p => new Product
                {
                    p_size_id = p.p_size_id,
                    product_name = p.product_name,
                })
                .ToList();

            foreach (var i in getProduct)
            {
                if (i.p_size_id.Equals(int.Parse(p_size_id)) && i.product_name.Equals(product_name))
                {
                    return BadRequest(new { Message = "The Product Already Exist" });
                }
            }

            Product product = new Product();
            product.product_name = product_name;
            product.product_quantity_stock = int.Parse(product_quantity);
            product.product_original_price = double.Parse(product_originalPrice);
            product.product_sell_price = double.Parse(product_sellPrice);
            product.product_description = product_description;
            product.p_category_id = int.Parse(p_category_id);
            product.p_supplier_id = int.Parse(p_supplier_id);
            product.p_size_id = int.Parse(p_size_id);
            product.product_import_date = DateTime.Now;
            product.product_status = product_status.New.ToString();
            product.product_image = files[0].FileName.ToString();

            var checkInfo = CheckValidateProductInfo(product);
            if (!string.IsNullOrEmpty(checkInfo))
                return BadRequest(new { Message = checkInfo.ToString() });

             await _context.Products.AddAsync(product);
             await _context.SaveChangesAsync();

            int maxProductId = _context.Products.Max(p => p.product_id);

            SaveFileAsync(files[0], "image");
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    if (IsImageFile(file.FileName))
                    {
                        SaveFileAsync(file, "Photos");
                        Models.Image image = new Models.Image();
                        image.image_uri = file.FileName;
                        image.i_product_id = maxProductId;

                         await _context.Images.AddAsync(image);
                         await _context.SaveChangesAsync();
                    }
                    else
                    {
                        return BadRequest(new { Message = "File Format Not Supported" });
                    }
                }
            }

            var getAdminEmail = _context.Accounts
                .Include(a => a.role)
                .Where(a => a.role.role_name.Equals("Admin"))
                .Select(a => a.account_email)
                .FirstOrDefault();

            var message = "You received this email because a new product with id: " +maxProductId+ " just added by manager named:";

            SendEmail(getAdminEmail, adder, message);

            return Ok(new
            {
                Message = "Waiting For Confirmation Of New Product"
            });
        }

        private void SendEmail(string email, string adder, string message)
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var emailToken = Convert.ToBase64String(tokenBytes);

            string from = _configuration["EmailSettings:From"];
            var emailBody = EmailBody.ProductNotifyEmail(adder, message);
            var emailModel = new EmailModel(email, "Product Notify Email", emailBody);
            _emailService.SendEmail(emailModel);
        }

        private async Task<string> SaveFileAsync(IFormFile file, string directory)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            var fileName = Path.GetFileName(file.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), directory, fileName);

            // check file exiting
            //int i = 0;
            //while (System.IO.File.Exists(filePath))
            //{
            //    i++;
            //    fileName = "(" + i + ")" + fileName;
            //    filePath = Path.Combine(Directory.GetCurrentDirectory(), directory, fileName);
            //}

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        private bool IsImageFile(string fileName)
        {
            string fileType = Path.GetExtension(fileName);
            return fileType.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                   fileType.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                   fileType.Equals(".png", StringComparison.OrdinalIgnoreCase);
        }


        private string CheckValidateProductInfo(Product product)
        {
            StringBuilder sp = new StringBuilder();
            Regex proName = new Regex("^[a-zA-Z]");
            Regex proQuantity = new Regex("^[0-9]+$");

            Match matchName = proName.Match(product.product_name);
            Match matchQuantity = proQuantity.Match(product.product_quantity_stock.ToString());
            Match matchOPrice = proQuantity.Match(product.product_original_price.ToString());
            Match matchSPrice = proQuantity.Match(product.product_sell_price.ToString());

            if (!matchName.Success)
                sp.Append("The product name must begin by a word" + Environment.NewLine);
            if (!matchQuantity.Success)
                sp.Append("The quantity must be an integer number!" + Environment.NewLine);
            if (!matchOPrice.Success)
                sp.Append("The original price must be a positive number!" + Environment.NewLine);
            if (!matchSPrice.Success)
                sp.Append("The sell price must be a positive number!" + Environment.NewLine);
            if(product.product_sell_price <= product.product_original_price)
                sp.Append("The sell price must be bigger than poriginal price!" + Environment.NewLine);

            return sp.ToString();
        }


        [HttpPut]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> UpdateProduct(List<IFormFile> files)
        {
            var product_id = Request.Form["product_id"];
            var product_name = Request.Form["product_name"];
            var product_quantity = Request.Form["product_quantity"];
            var product_originalPrice = Request.Form["product_originalPrice"];
            var product_sellPrice = Request.Form["product_sellPrice"];
            var product_description = Request.Form["product_description"];
            var p_category_id = Request.Form["p_category_id"];
            var p_supplier_id = Request.Form["p_supplier_id"];
            var p_size_id = Request.Form["p_size_id"];
            var product_status = Request.Form["product_status"];
            var product_image = Request.Form["product_image"];

            var pro = await _context.Products.FirstOrDefaultAsync(p => p.product_id.Equals(int.Parse(product_id)));

            if (pro == null)
                return NotFound(new { Message = "Product is not found" });

            var lstProduct = _context.Products
                      .Where(p => !p.product_id.Equals(int.Parse(product_id)))
                      .Select(p => new Product
                      {
                          product_name = p.product_name,
                          product_quantity_stock = p.product_quantity_stock,
                          product_original_price = p.product_original_price,
                          product_sell_price = p.product_sell_price,
                          product_description = p.product_description,
                          p_category_id = p.p_category_id,
                          p_supplier_id = p.p_supplier_id,
                          p_size_id = p.p_size_id
                      })
                      .ToList();

            if (files == null || files.Count == 0)
            {
                foreach (var i in lstProduct)
                {
                    if (i.p_size_id.Equals(int.Parse(p_size_id)) && i.product_name.Equals(product_name))
                    {
                        return BadRequest(new { Message = "Product is already exist!" });
                    }
                }

                pro.product_id = int.Parse(product_id);
                pro.product_name = product_name;
                pro.product_quantity_stock = int.Parse(product_quantity);
                pro.product_original_price = double.Parse(product_originalPrice);
                pro.product_sell_price = double.Parse(product_sellPrice);
                pro.product_description = product_description;
                pro.p_category_id = int.Parse(p_category_id);
                pro.p_supplier_id = int.Parse(p_supplier_id);
                pro.p_size_id = int.Parse(p_size_id);
                pro.product_import_date = DateTime.Now;
                pro.product_status = product_status;
                pro.product_import_date = DateTime.Now;
                pro.product_image = product_image;

                if (!string.IsNullOrEmpty(CheckValidateProductInfo(pro)))
                    return BadRequest(new { Message = CheckValidateProductInfo(pro).ToString() });

                _context.Products.Entry(pro).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Update Product Succeed"
                });
            }
            else
            {
                List<string> newImg = new List<string>();
                List<string> oldImg = new List<string>();

                foreach (var i in lstProduct)
                {

                    if (i.p_size_id.Equals(int.Parse(p_size_id)) && i.product_name.Equals(product_name))
                    {
                        return BadRequest(new { Message = "Product is already exist!" });
                    }
                }

                pro.product_id = int.Parse(product_id);
                pro.product_name = product_name;
                pro.product_quantity_stock = int.Parse(product_quantity);
                pro.product_original_price = double.Parse(product_originalPrice);
                pro.product_sell_price = double.Parse(product_sellPrice);
                pro.product_description = product_description;
                pro.p_category_id = int.Parse(p_category_id);
                pro.p_supplier_id = int.Parse(p_supplier_id);
                pro.p_size_id = int.Parse(p_size_id);
                pro.product_import_date = DateTime.Now;
                pro.product_status = product_status;
                pro.product_import_date = DateTime.Now;
                pro.product_image = files[0].FileName.ToString();

                if (!string.IsNullOrEmpty(CheckValidateProductInfo(pro)))
                    return BadRequest(new { Message = CheckValidateProductInfo(pro).ToString() });

                _context.Products.Entry(pro).State = EntityState.Modified;
                await _context.SaveChangesAsync();


                var img = _context.Images.Where(i => i.i_product_id.Equals(int.Parse(product_id))).ToList();

                foreach (var file in files)
                {
                    newImg.Add(file.FileName);
                }

                foreach (var i in img)
                {
                    oldImg.Add(i.image_uri);
                }

                bool listsAreEqual = newImg.SequenceEqual(oldImg);

                if (listsAreEqual)
                {
                    foreach (var i in img)
                    {
                        _context.Entry(i).State = EntityState.Modified;
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    foreach(var i in img)
                    {
                        _context.Images.Remove(i);
                        await _context.SaveChangesAsync();
                    }

                    SaveFileAsync(files[0], "image");
                    foreach (var i in files)
                    {
                        if (IsImageFile(i.FileName))
                        {
                            SaveFileAsync(i, "Photos");
                            Models.Image image = new Models.Image();

                            image.image_uri = i.FileName;
                            image.i_product_id = int.Parse(product_id);

                            await _context.Images.AddAsync(image);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            return BadRequest(new { Message = "File Format Not Supported" });
                        }
                    }
                }

                return Ok(new
                {
                    Message = "Update Product Succeed"
                });
            }
        }

        //update with role manager
        [HttpPut("update_product_with_role_manager")]
        [Authorize(Policy = "Manager")]
        public async Task<IActionResult> UpdateProductByManager(List<IFormFile> files)
        {
            var product_id = Request.Form["product_id"];
            var product_name = Request.Form["product_name"];
            var product_quantity = Request.Form["product_quantity"];
            var product_originalPrice = Request.Form["product_originalPrice"];
            var product_sellPrice = Request.Form["product_sellPrice"];
            var product_description = Request.Form["product_description"];
            var p_category_id = Request.Form["p_category_id"];
            var p_supplier_id = Request.Form["p_supplier_id"];
            var p_size_id = Request.Form["p_size_id"];
            var product_status = Request.Form["product_status"];
            var product_image = Request.Form["product_image"];
            var updater = Request.Form["updater"];

            var pro = await _context.Products.FirstOrDefaultAsync(p => p.product_id.Equals(int.Parse(product_id)));

            if (pro == null)
                return NotFound(new { Message = "Product is not found" });

            var lstProduct = _context.Products
                      .Where(p => !p.product_id.Equals(int.Parse(product_id)))
                      .Select(p => new Product
                      {
                          product_name = p.product_name,
                          product_quantity_stock = p.product_quantity_stock,
                          product_original_price = p.product_original_price,
                          product_sell_price = p.product_sell_price,
                          product_description = p.product_description,
                          p_category_id = p.p_category_id,
                          p_supplier_id = p.p_supplier_id,
                          p_size_id = p.p_size_id
                      })
                      .ToList();

            if (files == null || files.Count == 0)
            {
                foreach (var i in lstProduct)
                {
                    if (i.p_size_id.Equals(int.Parse(p_size_id)) && i.product_name.Equals(product_name))
                    {
                        return BadRequest(new { Message = "Product is already exist!" });
                    }
                }

                pro.product_id = int.Parse(product_id);
                pro.product_name = product_name;
                pro.product_quantity_stock = int.Parse(product_quantity);
                pro.product_original_price = double.Parse(product_originalPrice);
                pro.product_sell_price = double.Parse(product_sellPrice);
                pro.product_description = product_description;
                pro.p_category_id = int.Parse(p_category_id);
                pro.p_supplier_id = int.Parse(p_supplier_id);
                pro.p_size_id = int.Parse(p_size_id);
                pro.product_import_date = DateTime.Now;
                pro.product_status = product_status;
                pro.product_import_date = DateTime.Now;
                pro.product_image = product_image;

                if (!string.IsNullOrEmpty(CheckValidateProductInfo(pro)))
                    return BadRequest(new { Message = CheckValidateProductInfo(pro).ToString() });

                _context.Products.Entry(pro).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                var getAdminEmail = _context.Accounts
               .Include(a => a.role)
               .Where(a => a.role.role_name.Equals("Admin"))
               .Select(a => a.account_email)
               .FirstOrDefault();

                var message = "You received this email because a  product with id: " + int.Parse(product_id) + " just updated by manager named:";

                SendEmail(getAdminEmail, updater, message);

                return Ok(new
                {
                    Message = "Update Product Succeed"
                });
            }
            else
            {
                List<string> newImg = new List<string>();
                List<string> oldImg = new List<string>();

                foreach (var i in lstProduct)
                {

                    if (i.p_size_id.Equals(int.Parse(p_size_id)) && i.product_name.Equals(product_name))
                    {
                        return BadRequest(new { Message = "Product is already exist!" });
                    }
                }

                pro.product_id = int.Parse(product_id);
                pro.product_name = product_name;
                pro.product_quantity_stock = int.Parse(product_quantity);
                pro.product_original_price = double.Parse(product_originalPrice);
                pro.product_sell_price = double.Parse(product_sellPrice);
                pro.product_description = product_description;
                pro.p_category_id = int.Parse(p_category_id);
                pro.p_supplier_id = int.Parse(p_supplier_id);
                pro.p_size_id = int.Parse(p_size_id);
                pro.product_import_date = DateTime.Now;
                pro.product_status = product_status;
                pro.product_import_date = DateTime.Now;
                pro.product_image = files[0].FileName.ToString();

                if (!string.IsNullOrEmpty(CheckValidateProductInfo(pro)))
                    return BadRequest(new { Message = CheckValidateProductInfo(pro).ToString() });

                _context.Products.Entry(pro).State = EntityState.Modified;
                await _context.SaveChangesAsync();


                var img = _context.Images.Where(i => i.i_product_id.Equals(int.Parse(product_id))).ToList();

                foreach (var file in files)
                {
                    newImg.Add(file.FileName);
                }

                foreach (var i in img)
                {
                    oldImg.Add(i.image_uri);
                }

                bool listsAreEqual = newImg.SequenceEqual(oldImg);

                if (listsAreEqual)
                {
                    foreach (var i in img)
                    {
                        _context.Entry(i).State = EntityState.Modified;
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    foreach (var i in img)
                    {
                        _context.Images.Remove(i);
                        await _context.SaveChangesAsync();
                    }

                    SaveFileAsync(files[0], "image");
                    foreach (var i in files)
                    {
                        if (IsImageFile(i.FileName))
                        {
                            SaveFileAsync(i, "Photos");
                            Models.Image image = new Models.Image();

                            image.image_uri = i.FileName;
                            image.i_product_id = int.Parse(product_id);

                            await _context.Images.AddAsync(image);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            return BadRequest(new { Message = "File Format Not Supported" });
                        }
                    }
                }

                var getAdminEmail = _context.Accounts
               .Include(a => a.role)
               .Where(a => a.role.role_name.Equals("Admin"))
               .Select(a => a.account_email)
               .FirstOrDefault();

                var message = "You received this email because a  product with id: " + int.Parse(product_id) + " just updated by manager named:";

                SendEmail(getAdminEmail, updater, message);

                return Ok(new
                {
                    Message = "Update Product Succeed"
                });
            }
        }



        //Delete Product By Id
        [HttpDelete("{product_id}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> DeleteProduct(int product_id)
        {
            var product = await _context.Products.FindAsync(product_id);
            if(product == null) 
                return NotFound(new {Message = "Product with the id " + product_id + " is not found!"});

            product.product_status = product_status.No.ToString();

            _context.Products.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Delete Product Succeed"
            });
        }

        //Confirm the new product added by manager
        [HttpPut("confirm_product")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> confirmProduct()
        {
            var product_id = Request.Form["product_id"];

            var product = _context.Products.Where(p => p.product_id.Equals(int.Parse(product_id))).FirstOrDefault();

            if (product == null)
                return NotFound(new { Message = "Product with the id " + int.Parse(product_id) + " is not found!" });

            product.product_status = product_status.Yes.ToString();

            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "The New Product with ID: " + int.Parse(product_id) + " Confirmed" });
        }

        //Delete with role manager
        [HttpPut("delete_product_by_manager")]
        [Authorize(Policy = "Manager")]
        public async Task<IActionResult> DeleteProductByManager()
        {
            var id = Request.Form["id"];
            var deleter = Request.Form["deleter"];

            var product = await _context.Products.FindAsync(int.Parse(id));
            if (product == null)
                return NotFound(new { Message = "Product with the id " + int.Parse(id) + " is not found!" });

            product.product_status = product_status.Waiting_deleted.ToString();

            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var getAdminEmail = _context.Accounts
               .Include(a => a.role)
               .Where(a => a.role.role_name.Equals("Admin"))
               .Select(a => a.account_email)
            .FirstOrDefault();

            var message = "You received this email because a product with id: " + int.Parse(id) + " just deleted by manager named:";

            SendEmail(getAdminEmail, deleter, message);

            return Ok(new
            {
                Message = "Waiting For Confirmation Of Product Deletion"
            });
        }


        [HttpGet("search")]
        public async Task<IActionResult> search(string key)
        {
            var lstProduct = _context.Products
               .Include(p => p.category)
               .Include(p => p.supplier)
               .Include(p => p.size)
               .Select(p => new 
               {
                   product_id = p.product_id,
                   product_name = p.product_name,
                   product_quantity_stock = p.product_quantity_stock,
                   product_original_price = p.product_original_price,
                   product_sell_price = p.product_sell_price,
                   product_description = p.product_description,
                   product_status = p.product_status,
                   product_import_date = p.product_import_date,
                   p_category_id = p.p_category_id,
                   category_name = p.category.category_name,
                   p_supplier_id = p.p_supplier_id,
                   supplier_name = p.supplier.supplier_name,
                   p_size_id = p.p_size_id,
                   size_number = p.size.size_number,
                   product_image = p.product_image
               })
               .Where(p => p.product_name.Contains(key) || p.product_status.Equals(key))
               .GroupBy(p => p.product_name)
               .Select(g => g.First())
               .ToList();

            if (lstProduct == null || lstProduct.Count == 0)
                return NotFound(new { Message = "Product is not found!" });

            return Ok(lstProduct);
        }

        [HttpGet("CalculateSimilarityImage")]
        public async Task<IActionResult> CalculateSimilarityImage(string path)
        {
            //var str = "chelseablack1.png";

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Photos", path);

            return Ok(new { Path = filePath });
        }
    }
}
