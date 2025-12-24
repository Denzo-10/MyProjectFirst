using System.Security.Claims;
using DataLayer.Context;
using DataLayer.DTOs;
using DataLayer.Intarfaces;
using DataLayer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly AppDbContext _context;

        [BindProperty]
        public LoginDto LoginDto { get; set; } 

        public string ErrorMessage { get; set; }

        public LoginModel(IAuthService authService, AppDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var authResult = await _authService.AuthenticateAsync(LoginDto);

                if (authResult == null)
                {
                    ErrorMessage = "Неверный логин или пароль";
                    return Page();
                }

                // Получаем пользователя из базы
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Login == LoginDto.Login);

                if (user == null)
                {
                    ErrorMessage = "Пользователь не найден";
                    return Page();
                }

                // Создаем claims для аутентификации
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Login),
                    new Claim(ClaimTypes.Role, user.Role.Name),
                    new Claim("FullName", user.FullName),
                    new Claim("Token", authResult.Token) // Сохраняем JWT токен
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(3)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // Сохраняем информацию в сессии
                HttpContext.Session.SetString("UserId", user.UserId.ToString());
                HttpContext.Session.SetString("UserRole", user.Role.Name);
                HttpContext.Session.SetString("UserFullName", user.FullName);
                HttpContext.Session.SetString("Token", authResult.Token);

                returnUrl ??= Url.Content("~/");
                return LocalRedirect(returnUrl);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка авторизации: {ex.Message}";
                return Page();
            }
        }
    }
}