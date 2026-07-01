using Microsoft.EntityFrameworkCore;
using InfoGiovani_Back.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using InfoGiovani_Back.Middleware;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.StaticFiles;

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

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IAuthorizationHandler, PermessoAuthorizationHandler>();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
        policy.Requirements.Add(new PermessoRequirement(nameof(IdentitaUtente.CanCreateUser))));

    options.AddPolicy("Entity", policy =>
        policy.Requirements.Add(new PermessoRequirement(nameof(IdentitaUtente.CanCreateEntity))));

    options.AddPolicy("Private", policy =>
        policy.Requirements.Add(new PermessoRequirement(nameof(IdentitaUtente.CanViewCard))));
});


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

        /*policy.WithOrigins(
            "https://radici.orientarti.it"
        )*/
        policy.SetIsOriginAllowed(origin => true) // Permette qualsiasi origine
      .AllowAnyMethod()
      .AllowAnyHeader()
      .AllowCredentials(); // Ora questo è consentito

    });
});

builder.Services.AddScoped<TokenService>();

var app = builder.Build();

var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
// Insegna al server come servire i file dei font
provider.Mappings[".ttf"] = "application/x-font-truetype";
provider.Mappings[".woff"] = "application/font-woff";
provider.Mappings[".woff2"] = "application/font-woff2";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("LanPolicy");
app.UseAuthentication();
app.UseIdentitaUtente();
app.UseAuthorization();
app.MapControllers();

app.Run();
