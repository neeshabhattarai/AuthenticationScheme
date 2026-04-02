using System.Net;
using System.Net.Mail;
using AuthenticationScheme.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace AuthenticationScheme.Controller;
[ApiController]
[Route("api/[controller]")]
public class UserController(UserManager<IdentityUser> manager,IEmailService emailService):ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] User users)
    {
        var user = new IdentityUser
        {
            UserName = users.Email,
            Email = users.Email,
        };
        
        var result = await manager.CreateAsync(user, users.Password);
        if (result.Succeeded)
        {
            var token = await manager.GenerateEmailConfirmationTokenAsync(user);
            var ConfirmationEmail = Url.Action("EmailConfirmation", "User", new { userId = user.Id, tokenId = token },
                Request.Scheme);
            try
            {
                await emailService.SendEmail("nishabhattarai778@gmail.com", "Signup Confiramtion",
                    $"Confirm Email {ConfirmationEmail}");
            }
            catch (Exception e)
            {
                throw new BadHttpRequestException(e.Message);

            }
        }

        return Ok("Check your email box");
    }

    [HttpGet]
    [Route("emailConfirmation")]
    public async Task<IActionResult> EmailConfirmation([FromQuery]string userId,[FromQuery]string tokenId)
    {
        var user=await manager.FindByIdAsync(userId);
        if(user==null)
            throw new Exception("User not found");

        await manager.ConfirmEmailAsync(user, tokenId);
        return Ok("Success");

    }
    public class  User
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}