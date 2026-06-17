using Microsoft.EntityFrameworkCore;
using InfoGiovani_Back.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//configurazione JWT
var jwtConfig = builder.Configuration.GetSection("Jwt");
var key = new SymmetricSecurityKey(
    Convert.FromBase64String(jwtConfig["Key"]!)
);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },

        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtConfig["Issuer"],
        ValidAudience = jwtConfig["Audience"],
        IssuerSigningKey = key,

        ClockSkew = TimeSpan.FromSeconds(30)
    };
});

builder.Services.AddAuthorization();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHttpClient();

builder.Services.AddCors(options =>
{
    options.AddPolicy("LanPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            //.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); //per permettere l'invio dei cookie, necessario per il refresh token
    });
});

builder.Services.AddScoped<TokenService>();

//test da eliminare
builder.Services.AddSingleton<CookieOptions>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    return new CookieOptions
    {
        HttpOnly = true,
        Secure = !env.IsDevelopment(), // false in dev, true in prod
        SameSite = SameSiteMode.None,
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("LanPolicy");       
app.UseAuthentication();       
app.UseAuthorization();
app.MapControllers();           
app.Run();
