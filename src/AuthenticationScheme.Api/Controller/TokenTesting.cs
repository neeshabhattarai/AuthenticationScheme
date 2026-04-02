using AuthenticationScheme.Application;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationScheme.Controller;
[ApiController]
[Route("/api/[controller]")]
public class TokenTesting:ControllerBase
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TokenTesting(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    [HttpGet]
    public IActionResult GetToken()
    {
        var token = TokenGenerator.CreateToken();
        _httpContextAccessor.HttpContext.Session.SetString("token", token);
        _httpContextAccessor.HttpContext.Session.SetString("time", DateTime.UtcNow.ToString());
        return Ok(new
        {
            access_token = token,
            expires_in = DateTime.UtcNow.AddMinutes(10)
        });
    }
[HttpGet]
[Route("[action]")]
    public IActionResult CheckFlow()
    {
        var context = _httpContextAccessor.HttpContext.Session.GetString("token");
        var timeInSession=DateTime.Parse(_httpContextAccessor.HttpContext.Session.GetString("time") ??  DateTime.UtcNow.ToString());
        var timeNow = DateTime.UtcNow;
        if (context == null || string.IsNullOrWhiteSpace(context) || timeNow<timeInSession)

    {
            return Unauthorized();
        }
        return Ok("You are now logged in as: ");
    }
    
}