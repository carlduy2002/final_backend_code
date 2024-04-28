using Azure.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Neo4jClient.DataAnnotations.Cypher.Functions;
using NETCore.MailKit.Core;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using web_project_BE.Data;
using web_project_BE.Helper;
using web_project_BE.Models;
using web_project_BE.Models.Dto;
using static System.Runtime.InteropServices.JavaScript.JSType;
using IEmailService = web_project_BE.UtilityServices.IEmailService;

namespace web_project_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _env;
        public AccountController(ApplicationDbContext context, IConfiguration configuration, IEmailService emailService, IWebHostEnvironment env)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _env = env;
        }

        //Get All Account
        [HttpGet("GetAllAccount")]
        [Authorize(Policy = "AdminOrManagerPolicy")]
        public async Task<IActionResult> GetAllAccount()
        {
            var lstAccount = _context.Accounts
                .Join(
                    _context.Roles,
                    account => account.account_role_id,
                    role => role.role_id,
                    (account, role) => new
                    {
                        account_id = account.account_id,
                        account_username = account.account_username,
                        account_email = account.account_email,
                        account_address = account.account_address,
                        account_phone = account.account_phone,
                        account_birthday = account.account_birthday,
                        role = role.role_name,
                        account_status = account.account_status
                    }
                );

            if (lstAccount == null)
                return BadRequest(new { Message = "Account is null" });

            return Ok(lstAccount);
        }

        [HttpGet("Username")]
        public async Task<IActionResult> getAccountID(string username)
        {
            var user = _context.Accounts.FirstOrDefault(a => a.account_username == username);

            if(user == null) 
                return NotFound(new {Message = "User not found!"});

            return Ok(user.account_id);
        }

        [HttpGet("User")]
        public async Task<IActionResult> getUserByUsername(string user)
        {
            var account = _context.Accounts.Select(a => new Account
            {
                account_id = a.account_id,
                account_username = a.account_username,
                account_email = a.account_email,
                account_address = a.account_address,
                account_phone = a.account_phone,
                account_birthday = a.account_birthday,
                account_gender = a.account_gender,
                account_avatar = a.account_avatar,
                account_password = a.account_password,

            }).Where(a => a.account_username == user).ToList();

            if (account == null)
                return NotFound(new { Message = "User not found!" });

            return Ok(account);
        }


        //Get Account by id
        [HttpGet("Id")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var account_id = await _context.Accounts.FindAsync(id);
            return account_id == null ? NotFound(new { Message = "Account with the id " + id + " is not found!!" }) : Ok(account_id);
        }

        //Register account
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] Account account)
        {
            if (account == null)
                return BadRequest();

            if (await CheckUsernameExist(account.account_username))
                return BadRequest(new { Message = "Username already exist!" });

            var username = CheckUsernameValid(account.account_username);
            if (!string.IsNullOrEmpty(username))
                return BadRequest(new { Message = username.ToString() });

            if (await CheckEmailExist(account.account_email))
                return BadRequest(new { Message = "Email already exist!" });

            var email = CheckEmailValid(account.account_email);
            if (!string.IsNullOrEmpty(email))
                return BadRequest(new { Message = email.ToString() });

            var pwd = CheckPasswordValid(account.account_password);
            if (!string.IsNullOrEmpty(pwd))
                return BadRequest(new { Message = pwd.ToString() });

            if (await CheckPhoneExist(account.account_phone))
                return BadRequest(new { Message = "Phone number already exist!" });

            var phone = CheckPhoneValid(account.account_phone);
            if (!string.IsNullOrEmpty(phone))
                return BadRequest(new { Message = phone.ToString() });

            var birthday = CheckBirthDayValid(account.account_birthday.ToString());
            if (!string.IsNullOrEmpty(birthday))
                return BadRequest(new {Message = birthday.ToString()});

            if (account.account_confirm_password != account.account_password)
                return BadRequest(new { Message = "Password and Confirm Password is not match!" });

            account.account_password = PasswordHasher.HashPassword(account.account_password);
            account.account_confirm_password = PasswordHasher.HashPassword(account.account_confirm_password);
            account.account_status = account_status.Unlock.ToString();

            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();

            Cart cart = new Cart();
            cart.account_id = account.account_id;
            await _context.Carts.AddAsync(cart);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Register Succeed"
            });
        }

        //[HttpPost("AddAccount")]
        //public async Task<IActionResult> addaccount()
        //{

        //    var count = 1000;

        //    for(var i = 10; i < count; i++)
        //    {
        //        Account account = new Account();

        //        account.account_username = "Customer " + i.ToString();
        //        account.account_email = "Customer" + i.ToString() + "@gmail.com";
        //        account.account_address = "Customer Address " + i.ToString();
        //        account.account_password = PasswordHasher.HashPassword("Admin@123");
        //        account.account_confirm_password = PasswordHasher.HashPassword("Admin@123");
        //        account.account_birthday = DateTime.Parse("2000-07-10 00:00:00.0000000");
        //        account.account_gender = "Male";
        //        account.account_status = account_status.Unlock.ToString();
        //        account.account_role_id = 3;
                
        //        if(i < 10)
        //        {
        //            account.account_phone = "094512694" + i.ToString();
        //        }
                
        //        if(i > 9 && i < 100)
        //        {
        //            account.account_phone = "09451269" + i.ToString();
        //        }

        //        if (i > 99 && i < 1000)
        //        {
        //            account.account_phone = "0945126" + i.ToString();
        //        }

        //        await _context.Accounts.AddAsync(account);
        //        await _context.SaveChangesAsync();

        //        var account_id = _context.Accounts.Max(a => a.account_id);

        //        Cart cart = new Cart();
        //        cart.account_id = account_id;
        //        await _context.Carts.AddAsync(cart);
        //        await _context.SaveChangesAsync();
        //    }

        //    return Ok(new
        //    {
        //        Message = "Register Succeed"
        //    });
        //}


        [HttpPut("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile()
        {
            var username = Request.Form["username"];
            var email = Request.Form["email"];
            var address = Request.Form["address"];
            var phone = Request.Form["phone"];
            var id = Request.Form["id"];
            var birthday = Request.Form["birthday"];
            var gender = Request.Form["gender"];
            var image = Request.Form.Files["uploadFile"];

            var acc = await _context.Accounts.FirstOrDefaultAsync(a => a.account_id.Equals(int.Parse(id)));

            if (acc == null)
                return NotFound(new { Message = "Account with the id " + int.Parse(id) + " is not found!" });

            if (username != acc.account_username)
            {
                if (await CheckUsernameExist(username))
                    return BadRequest(new { Message = "Username already exist!" });
            }

            if(email != acc.account_email)
            {
                if (await CheckEmailExist(email))
                    return BadRequest(new { Message = "Email already exist!" });
            }

           
            if (!string.IsNullOrEmpty(CheckEmailValid(email)))
                return BadRequest(new { Message = CheckEmailValid(email).ToString() });

            if (!string.IsNullOrEmpty(CheckPhoneValid(phone)))
                return BadRequest(new { Message = CheckPhoneValid(phone).ToString() });

            if (!string.IsNullOrEmpty(CheckBirthDayValid(birthday)))
                return BadRequest(new { Message = CheckBirthDayValid(birthday).ToString() });

            acc.account_username = username;
            acc.account_email = email;
            acc.account_address = address;
            acc.account_phone = phone;
            acc.account_birthday = DateTime.Parse(birthday);
            acc.account_gender = gender;
            acc.account_status = account_status.Unlock.ToString();
            acc.account_password = acc.account_password;

            if(image != null)
            {
                if (IsImageFile(image.FileName))
                {
                    SaveFileAsync(image, "Photos");
                    acc.account_avatar = image.FileName;
                }
            }

            acc.token = CreateJwt(acc);

            var newAccessToken = acc.token;
            var newRefreshToken = CreateRefreshToken();

            acc.refesh_token = newRefreshToken;
            acc.refesh_token_exprytime = DateTime.Now.AddDays(1);

            _context.Entry(acc).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                Message = "Profile Updated"
            });
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
            int i = 0;
            while (System.IO.File.Exists(filePath))
            {
                i++;
                fileName = "(" + i + ")" + fileName;
                filePath = Path.Combine(Directory.GetCurrentDirectory(), directory, fileName);
            }

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


        //Update Account
        [HttpPost("BanAccount")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> BanAccount(int id)
        {
            var account = await _context.Accounts.FindAsync(id);

            if(account == null)
                return NotFound(new { Message = "Account is Not Found!" });

            account.account_status = account_status.Lock.ToString();

            _context.Entry(account).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Ban Account Succeed"
            });
        }

        [HttpPost("UnBanAccount")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> UnBanAccount(int account_id)
        {
            var account = await _context.Accounts.FindAsync(account_id);

            if (account == null)
                return NotFound(new { Message = "Account is Not Found!" });

            account.account_status = account_status.Unlock.ToString();

            _context.Entry(account).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "UnBan Account Succeed"
            });
        }

        [HttpPost("Password")]
        public async Task<IActionResult> checkOldPassword(int user_id, string password)
        {
            var user = await _context.Accounts.FirstOrDefaultAsync(a => a.account_id == user_id);

            if (user == null)
                return NotFound(new { Message = "User not found!" });

            if (!PasswordHasher.VerifyPassword(password, user.account_password))
                return BadRequest(new { Message = "Password is incorrectly!" });

            return Ok();
        }

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword(string newPass, string conPass, int user_id)
        {
            var user = await _context.Accounts.FirstOrDefaultAsync(a => a.account_id == user_id);

            if (user == null)
                return NotFound(new { Message = "User not found!" });

            var pwd = CheckPasswordValid(newPass);
            if (!string.IsNullOrEmpty(pwd))
                return BadRequest(new { Message = pwd.ToString() });

            if (PasswordHasher.VerifyPassword(newPass, user.account_password))
                return BadRequest(new { Message = "This password already exists before, please enter another password!" });

            if (conPass != newPass)
                return BadRequest(new { Message = "Password and Confirm Password is not match!" });

            user.account_status = account_status.Unlock.ToString();
            user.account_password = PasswordHasher.HashPassword(newPass);
            user.account_confirm_password = PasswordHasher.HashPassword(conPass);

            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Change Password Succeed"
            });
        }

        //Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (username == null && password == null)
                return BadRequest();

            var user = await _context.Accounts.FirstOrDefaultAsync(x => x.account_username == username);

            if (user == null)
                return NotFound(new { Message = "Username or Password is invalid!" });

            if(user.account_status == account_status.Lock.ToString())
                return BadRequest(new {Message = "Account has been locked!!" });

            if (!PasswordHasher.VerifyPassword(password, user.account_password))
                return BadRequest(new { Message = "Username or Password is invalid!" });

            user.token = CreateJwt(user);
            var newAccessToken = user.token;
            var newRefreshToken = CreateRefreshToken();
            user.refesh_token = newRefreshToken;
            user.refesh_token_exprytime = DateTime.UtcNow.AddDays(1);
            await _context.SaveChangesAsync();

            return Ok(new TokenApiDto()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                Message = "Login Succeed"
            });
        }

        //Check username that exist or not
        private Task<bool> CheckUsernameExist(string username)
        {
            return _context.Accounts.AnyAsync(x => x.account_username == username);
        }

        //Check email that exist or not
        private Task<bool> CheckEmailExist(string email)
        {
            return _context.Accounts.AnyAsync(x => x.account_email == email);
        }

        private Task<bool> CheckPhoneExist(string phone)
        {
            return _context.Accounts.AnyAsync(x => x.account_phone == phone);
        }

        private string CheckEmailValid(string email)
        {
            StringBuilder sb = new StringBuilder();

            if(!(Regex.IsMatch(email, "[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}")))
                sb.Append("Email is invalid" + Environment.NewLine);

            return sb.ToString();
        }

        private string CheckUsernameValid(string username)
        {
            StringBuilder sb = new StringBuilder();

            if (!(Regex.IsMatch(username, "^.{5,20}$")))
                sb.Append("Username length must larger than 5 and smaller than 20 characters!" + Environment.NewLine);

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

        private string CheckBirthDayValid(string birthday)
        {
            StringBuilder sb = new StringBuilder();

            //birthday = birthday.Substring(0,9);

            DateTime dateTime = DateTime.Parse(birthday);
            DateTime current = DateTime.Now;

            if (dateTime > current)
                sb.Append("Birthday is invalid!" + Environment.NewLine);
           
            return sb.ToString();
        }

        private string CheckPasswordValid(string password)
        {
            StringBuilder sb = new StringBuilder();
            if(password.Length < 8)
                sb.Append("Minimum password length should be 8" + Environment.NewLine);
            if(!(Regex.IsMatch(password, "[a-z]") && Regex.IsMatch(password, "[A-Z]")
                && Regex.IsMatch(password, "[0-9]")))
                sb.Append("Password should be Alphanumeric" + Environment.NewLine);
            if (!Regex.IsMatch(password, "[<,>,@,!,#,$,$,^,&,*,(,),_,+,\\,//]"))
                sb.Append("Password should contain special characters" + Environment.NewLine);

            return sb.ToString();
        }


        private string CreateJwt(Account acc)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaa");

            var account = _context.Roles
                .Include(r => r.accounts)
                .Where(r => r.role_id == acc.account_role_id)
                .Select(r => r.role_name)
                .FirstOrDefault();

            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, account),
                new Claim(ClaimTypes.Name, $"{acc.account_username}")
            });

            var credential = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.UtcNow.AddDays(1).ToUniversalTime(),
                SigningCredentials = credential
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);

            return jwtTokenHandler.WriteToken(token);
        }


        private string CreateRefreshToken()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var refreshToken = Convert.ToBase64String(tokenBytes);

            var tokenInUser = _context.Accounts.Any(a => a.refesh_token == refreshToken);
            if (tokenInUser)
            {
                return CreateRefreshToken();
            }

            return refreshToken;
        }

        private ClaimsPrincipal GetPrincipleFromExpiredToken(string token)
        {
            var key = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaa");
            var tokenValidate = new TokenValidationParameters 
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = false,
            };
            
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidate, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("This is Invalid Token");

            return principal;
        }

        [HttpPost("Refresh")]
        public async Task<IActionResult> Refresh(TokenApiDto tokenApiDto)
        {
            if (tokenApiDto == null)
                return BadRequest("Invalid Client Request!");

            string accessToken = tokenApiDto.AccessToken;
            string refreshToken = tokenApiDto.RefreshToken;

            var principal = GetPrincipleFromExpiredToken(accessToken);
            var username = principal.Identity.Name;
            var user = await _context.Accounts.FirstOrDefaultAsync(u => u.account_username == username);

            if (user == null || user.refesh_token != refreshToken || user.refesh_token_exprytime <= DateTime.Now)
                return BadRequest("Invalid Request!");

            var newAccessToken = CreateJwt(user);
            var newRefreshToken = CreateRefreshToken();
            user.refesh_token = newRefreshToken;
            await _context.SaveChangesAsync();

            return Ok(new TokenApiDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
            });
        }


        [HttpPost("Send-reset-email/{email}")]
        public async Task<IActionResult> SendEmail(string email)
        {
            var user = _context.Accounts.FirstOrDefault(x => x.account_email == email);
            if (user is null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "email doesn't exist"
                });
            }

            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var emailToken = Convert.ToBase64String(tokenBytes);
            user.reset_password_token = emailToken;
            user.reset_password_exprytime = DateTime.Now.AddMinutes(15);

            var hashEmail = PasswordHasher.HashPassword(email);
            string from = _configuration["EmailSettings:From"];
            var emailModel = new EmailModel(email, "Reset Password!!", EmailBody.EmailStringBody(email, emailToken));
            _emailService.SendEmail(emailModel);
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                StatusCode = 200,
                Message = "Email Sent!, Please check your email!"
            });
        }



        [HttpPost("Reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO resetPasswordDto)
        {
            var newToken = resetPasswordDto.EmailToken.Replace(" ", "+");
            var user = await _context.Accounts.AsNoTracking().FirstOrDefaultAsync(x => x.account_email == resetPasswordDto.Email);
            if (user == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "Email doesn't exist"
                });
            }

            var tokenCode = user.reset_password_token;
            DateTime emailTokenExpiry = user.reset_password_exprytime;
            if (tokenCode != resetPasswordDto.EmailToken || emailTokenExpiry < DateTime.Now)
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "Invalid Reset Link"
                });
            }

            var pwd = CheckPasswordValid(resetPasswordDto.NewPassword);
            if (!string.IsNullOrEmpty(pwd))
                return BadRequest(new { Message = pwd.ToString() });

            if (PasswordHasher.VerifyPassword(resetPasswordDto.NewPassword, user.account_password))
                return BadRequest(new { Message = "This password already exists before, please enter another password!" });

            if (resetPasswordDto.ConfirmPassword != resetPasswordDto.NewPassword)
                return BadRequest(new { Message = "Password and Confirm Password is not match!" });

            user.account_password = PasswordHasher.HashPassword(resetPasswordDto.NewPassword);
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                StatusCode = 200,
                Message = "Reset Password Successfully"
            });
        }


        [HttpPost("UploadImage")]
        public async Task<IActionResult> uploadImage()
        {
            try
            {
                var httpRequest = Request.Form;
                var postedFile = httpRequest.Files[0];
                string filename = postedFile.FileName;
                var physicalPath = _env.ContentRootPath + "/Photos/" + filename;

                using (var stream = new FileStream(physicalPath, FileMode.Create))
                {
                    postedFile.CopyTo(stream);
                }

                return new JsonResult(filename);
            }
            catch (Exception)
            {
                return BadRequest(new { Message = "Not Succeed" });
            }
        }

    }
}
