namespace CVBuilder.Api.Controllers
{
    [ApiController]
    [Microsoft.AspNetCore.Mvc.Route("api/user")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userInfo = await _userService.GetMeAsync(User);
            if (userInfo == null)
                return Unauthorized("User not found.");

            return Ok(userInfo);
        }
    }
}
