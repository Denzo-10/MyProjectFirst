using DataLayer.DTOs;
using DataLayer.Intarfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var authResponse = await _authService.AuthenticateAsync(loginDto);

            if (authResponse == null)
                return Unauthorized("Неверный логин или пароль");

            return Ok(authResponse);
        }
    }
}