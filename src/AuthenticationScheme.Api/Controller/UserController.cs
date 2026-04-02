using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using AuthenticationScheme.Infastructure.Entities;
using AuthenticationScheme.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace AuthenticationScheme.Controller;
[ApiController]
[Route("api/[controller]")]
public class UserController(UserManager<User> manager,IEmailService emailService):ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] User users)
    {
        var user = new Infastructure.Entities.User
        {
            UserName = users.Email,
            Email = users.Email,
            Department=users.Department
        };
        
        var result = await manager.CreateAsync(user, users.Password);
        if (result.Succeeded)
        {
            var token = await manager.GenerateEmailConfirmationTokenAsync(user);
            await manager.AddClaimAsync(user,new Claim("Department",user.Department));
            var ConfirmationEmail = Url.Action("EmailConfirmation", "User", new { userId = user.Id, tokenId = token },
                Request.Scheme);
            try
            {
                await emailService.SendEmail("nishabhattarai778@gmail.com", "Signup Confiramtion",
                    $"Confirm Email {ConfirmationEmail}");
                return Ok("Check your email box");

            }
            catch (Exception e)
            {
                throw new BadHttpRequestException(e.Message);

            }
        }
return BadRequest(result.Errors);
    }

    [HttpGet]
    [Route("emailConfirmation")]
    public async Task<IActionResult> EmailConfirmation([FromQuery]string userId,[FromQuery]string tokenId)
    {
        var user=await manager.FindByIdAsync(userId);
        if(user==null)
            throw new Exception("User not found");

        await manager.ConfirmEmailAsync(user, tokenId);
        return Ok("Thank you for confirming your email box");

    }
    public class  User
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Department { get; set; }
    }
}