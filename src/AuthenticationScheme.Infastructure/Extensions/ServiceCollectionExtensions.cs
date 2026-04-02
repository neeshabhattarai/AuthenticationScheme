using AuthenticationScheme.Infastructure.Entities;
using AuthenticationScheme.Infastructure.Persistent;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.Extensions.DependencyInjection;

namespace AuthenticationScheme.Infastructure;

public static class ServiceCollectionExtensions
{
  public static void AddInfastructureService(this IServiceCollection collection,IConfiguration configuration)
  {
      collection.AddIdentity<User,IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
      collection.AddDbContext<ApplicationDbContext>(options =>
      {
options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
      });
  }  
}