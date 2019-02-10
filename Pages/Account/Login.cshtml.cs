using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ReestrGNVLS.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace ReestrGNVLS.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly string connectionString = new SqlConnectionStringBuilder
        {
            DataSource = "hidden",
            UserID = "hidden",
            Password = "hidden",
            Pooling = true,
        }.ConnectionString;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(ILogger<LoginModel> logger)
        {
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Display(Name = "логин")]
            [Required(ErrorMessage = "необходимо ввести логин")]
            [DataType(DataType.Text)]
            [StringLength(5, MinimumLength = 5, ErrorMessage = "длина имени пользователя = 5 символов")]
            public string Name { get; set; }

            [Display(Name = "пароль")]
            [Required(ErrorMessage = "необходимо ввести пароль")]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        private async Task<bool> CheckLogin(string login, string password)
        {
            int count = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand query = new SqlCommand
                    {
                        CommandText = $@"select count(a.idapt) count
                                            from opeka_base.dbo.AptInfo a
                                            where a.idapt = @login",
                        Connection = connection
                    };
                    query.Parameters.AddWithValue("login", login);
                    count = (int)await query.ExecuteScalarAsync();
                    if (count == 1)
                    {
                        string pass = new string(login.Reverse().ToArray());
                        if (pass == password)
                        {
                            return true;
                        }
                    }
                }
            }
            catch (SqlException)
            {
                ViewData["Message"] = "Ошибка соединения с базой данных, повторите попытку входа";
            }
            catch (Exception)
            {
                throw;
            }
            return false;
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            // Clear the existing external cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;

            if (ModelState.IsValid)
            {
                try
                {
                    // Use Input.Email and Input.Password to authenticate the user
                    // with your custom authentication logic.

                    var user = await AuthenticateUser(Input.Name, Input.Password);

                    if (user == null)
                    {
                        ModelState.AddModelError(string.Empty, "Введен неверный логин или пароль.");
                        return Page();
                    }

                    HttpContext.Session.SetString("aptekaId", Input.Name);

                    var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Role, "User"),
                };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        //AllowRefresh = <bool>,
                        // Refreshing the authentication session should be allowed.

                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(720),
                        // The time at which the authentication ticket expires. A 
                        // value set here overrides the ExpireTimeSpan option of 
                        // CookieAuthenticationOptions set with AddCookie.

                        IsPersistent = false,
                        // Whether the authentication session is persisted across 
                        // multiple requests. Required when setting the 
                        // ExpireTimeSpan option of CookieAuthenticationOptions 
                        // set with AddCookie. Also required when setting 
                        // ExpiresUtc.

                        //IssuedUtc = <DateTimeOffset>,
                        // The time at which the authentication ticket was issued.

                        //RedirectUri = <string>
                        // The full path or absolute URI to be used as an http 
                        // redirect response value.
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                    _logger.LogInformation($"User {user.Name} logged in at {DateTime.UtcNow}.");

                    return LocalRedirect(Url.GetLocalUrl(returnUrl));
                }
                catch (Exception e)
                {
                    ViewData["Message"] = e.Message + Environment.NewLine + e.StackTrace;
                    return Page();
                }
            }

            // Something failed. Redisplay the form.
            return Page();
        }

        private async Task<ApplicationUser> AuthenticateUser(string name, string password)
        {
            if (await CheckLogin(name, password))
            {
                return new ApplicationUser()
                {
                    Name = name
                };
            }
            else
            {
                return null;
            }
        }
    }
}
