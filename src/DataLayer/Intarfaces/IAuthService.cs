using DataLayer.DTOs;

namespace DataLayer.Intarfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> AuthenticateAsync(LoginDto loginDto);
    }
}
