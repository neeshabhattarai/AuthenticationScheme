using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using AuthenticationScheme.Application;
using AuthenticationScheme.Infastructure.Entities;
using AuthenticationScheme.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace AuthenticationScheme.Controller;
[ApiController]
[Route("api/[controller]")]
public class UserController(UserManager<User> manager,IEmailService emailService,SignInManager<User> signInManager,IHttpContextAccessor contextAccessor):ControllerBase
{
    [HttpPost("login/withtoken")]
    public async Task<IActionResult> LoginUser([FromBody] User user)
    {
        var userManage=await signInManager.PasswordSignInAsync(user.Email,user.Password,false,false);
        if (userManage.Succeeded)
        {
            var userTest = await manager.FindByEmailAsync(user.Email);
            var userClaims=new List<Claim>
            {
                new Claim(ClaimTypes.Email,userTest.Email),
                new Claim("Department",userTest.Department),
                new Claim(ClaimTypes.NameIdentifier,userTest.Id)
                
                
            };
            var token =  TokenGenerator.GenerateToken(userClaims);
            
 return Ok(token);
        }
        return BadRequest("Invalid user");
    }
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
            await manager.SetTwoFactorEnabledAsync(user, true);
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

    [HttpPost("login")]
    public async Task<IActionResult> LoginUser([FromBody] string email, string password)
    {
        var result = await signInManager.PasswordSignInAsync(email, password, false,false);
        if (result.Succeeded)
        {
            return Ok("Thank you for logging in");
        }
        else
        {
            if (result.RequiresTwoFactor)
            {
                var user = await manager.FindByEmailAsync(email);

                if (user == null)
                    throw new Exception("User not found");
                var token = await manager.GenerateTwoFactorTokenAsync(user,"Email");
                await emailService.SendEmail("nishabhattarai778@gmail.com", "Two -factor Authentication",
                    $"Confirm your otp-{token}");
            }
        

    }
        return Ok();
    }

    [HttpGet("/confirmationCode")]
    public async Task<IActionResult> CheckAuthenticationCode([FromQuery] string code)
    {
        var result =
            signInManager.TwoFactorSignInAsync("Email", code, false, false);
        if (result.Result.Succeeded)
        {
            return Ok("Thank you for confirming your OTP");
        }

        return BadRequest("Invalid OTP");
    }

    [HttpPost("MFA")]
    public async Task<IActionResult> MFA()
    {
        var user=contextAccessor.HttpContext.User;
        var userTest = await manager.FindByIdAsync(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        var userClaims = user.Claims.ToList();
        if (user==null)
        {
            return BadRequest("User not found");
        }
        if(userTest==null){throw new Exception("User not found");}
       var token= await manager.GetAuthenticatorKeyAsync(userTest);
       if (token == null)
       {
           await manager.ResetAuthenticatorKeyAsync(userTest);
           token=await manager.GetAuthenticatorKeyAsync(userTest);
       }
       return Ok(token);
    }

    [HttpGet("logout")]
     public async Task<IActionResult> LogoutUser()
     {
         await signInManager.SignOutAsync();
         return Ok();
    }
    public class  User
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Department { get; set; }
    }
}