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
using QRCoder;

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
    [HttpGet("QRGenerator")]
    [Authorize]
    public async Task<IActionResult> QrCode()
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
        var email="nishabhattarai778@gmail.com";
        var qrCodeUrl = $"otpauth://totp/MyApp:{email}?secret={token}&issuer=MyApp&digits=6";
 var qrCoderGenerator=new QRCodeGenerator();
 var qrCode=qrCoderGenerator.CreateQrCode(qrCodeUrl,QRCodeGenerator.ECCLevel.Q);
 var byteCode=new PngByteQRCode(qrCode);
 var graphics=byteCode.GetGraphic(20);
 return File(graphics,"image/png");
 

    }
    [HttpPost("MFA")]
    [Authorize]
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
    [HttpGet("QR")]
    [Authorize]
    public async Task<IActionResult> QRCodeChecker([FromQuery]string code)
    {
        var user=contextAccessor.HttpContext.User;
        var userTest = await manager.FindByIdAsync(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        var userClaims = user.Claims.ToList();
        if (user==null)
        {
            return BadRequest("User not found");
        }
        if(userTest==null){throw new Exception("User not found");}

        var userVerified = await manager.VerifyTwoFactorTokenAsync(userTest, manager.Options.Tokens.AuthenticatorTokenProvider, code);
        if (userVerified)
        {
            await manager.SetTwoFactorEnabledAsync(userTest, true);
            return Ok("Thank you for verifying your OTP");
        }
        return BadRequest("Invalid OTP");
    }

    [HttpPost("LoginWithAuthenticator")]
    public async Task<IActionResult> LoginWithAuthenticator([FromBody] User user,[FromQuery]string code)
    {
      var result=await signInManager.PasswordSignInAsync(user.Email, user.Password, false, false);
      if (result.Succeeded)
      {
          return Ok("Thank you for logging in");
      }
      else
      {
          if (result.RequiresTwoFactor)
          {
              var users = await manager.FindByEmailAsync(user.Email);
              var twoFactorResult = await manager.VerifyTwoFactorTokenAsync(users, manager.Options.Tokens.AuthenticatorTokenProvider, code);
              if (twoFactorResult)
              {
                  await manager.SetTwoFactorEnabledAsync(users, true);

                  return Ok("Thank you for logging in");
              } 
          }
      }
       return BadRequest("Invalid OTP"); 
    }

    [HttpPost("CheckMFA")]
    public async Task<IActionResult> CheckMFA(string code)
    {
        var userClaim=contextAccessor.HttpContext.User;
        var user=await manager.GetUserAsync(userClaim);
       var result= await manager.VerifyTwoFactorTokenAsync(user, manager.Options.Tokens.AuthenticatorTokenProvider, code);
       if (result)
       {
           return Ok("Thank you for verifying your OTP");
       }
       else
       {
           return BadRequest("Invalid OTP");
       }
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