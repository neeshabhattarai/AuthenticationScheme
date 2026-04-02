
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationScheme.Application;

public static class TokenGenerator
{
   public static string CreateToken()
   {
      var cliamsIdentity = GenerateToken(new List<Claim>
      {
new Claim(ClaimTypes.NameIdentifier, "admin123@gmail.com"),
new Claim(ClaimTypes.Name, "admin"),
new Claim(ClaimTypes.Role, "admin"),
// new Claim(ClaimTypes.Role, "user"),
new Claim(ClaimTypes.Email,"adbc@gmail.com")
      });
      return cliamsIdentity;
   }

   public static string GenerateToken(List<Claim> cliams)
   {
      var claimsIdentity = new Dictionary<string,object>();
      foreach (var cliam in cliams)
      {
         claimsIdentity.Add(cliam.Type,cliam.Value);
      }

      var CreateToken = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor()
      {
         SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("MySecretKeyASutoehntbohf;hf;oihoishf;oihedioufhuisdhfui")),
            SecurityAlgorithms.HmacSha256Signature),
         Claims = claimsIdentity,
      });
      return CreateToken;
   }
}