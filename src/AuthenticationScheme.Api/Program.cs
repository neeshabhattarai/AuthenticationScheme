using System.Runtime.InteropServices.JavaScript;
using AuthenticationScheme.Application.Extension;
using AuthenticationScheme.Infastructure;
using AuthenticationScheme.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthentication().AddFacebook(opt =>
{
    opt.AppId = builder.Configuration.GetValue<string>("Application[AppId]");
    opt.AppSecret = builder.Configuration.GetValue<string>("Application[AppSecret]");
});
 builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Authorization",new OpenApiSecurityScheme
    {
        Scheme = "Bearer",
        Description =  "Please insert JWT with Bearer into field",
        Type = SecuritySchemeType.Http
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Authorization"
                },
            },
            new string[] {}
        }
    });
});
builder.Services.AddControllers();
// builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("MySecret"))};
});
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("IsAdmin",opt=>opt.RequireRole("admin"));
});
// builder.Services.AddApplication();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddInfastructureService(builder.Configuration);
builder.Services.Configure<SmtpSetting>(builder.Configuration.GetSection("Smtp"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

