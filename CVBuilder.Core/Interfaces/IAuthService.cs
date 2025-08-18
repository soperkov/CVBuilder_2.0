namespace CVBuilder.Core.Interfaces
{
    public interface IAuthService
    {
        Task<string?> RegisterAsync(RegisterUserDto dto);
        Task<string?> LoginAsync(LoginUserDto dto);
        Task<UserInfoDto?> GetUserInfoAsync(int userId);
    }
}
