namespace CVBuilder.Core.Interfaces
{
    public interface IUserService
    {
        Task<UserInfoDto?> GetMeAsync(ClaimsPrincipal user);
    }
}
