using System.Text;
using ConverterApi.Data;
using ConverterApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var frontendUrl = builder.Configuration["FRONTEND_URL"] ?? "https://file-converter-phi-nine.vercel.app";

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendOnly", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var cs = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(cs));

var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var db = services.GetRequiredService<AppDbContext>();
    
    try 
    { 
        var connectionString = app.Configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            // Mask password for safety in logs
            var maskedCs = System.Text.RegularExpressions.Regex.Replace(connectionString, "Password=[^;]+", "Password=***");
            logger.LogInformation("Attempting migration with connection: {ConnectionString}", maskedCs);
        }
        
        db.Database.Migrate(); 
        logger.LogInformation("Database migration completed successfully.");
    } 
    catch (Exception ex) 
    { 
        logger.LogError(ex, "An error occurred during database migration. Check Azure Firewall and Connection String.");
    }
}

// Swagger'ı prod'da da görmek istersen bunu kaldırma:
app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseCors("FrontendOnly");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/ip", (HttpContext ctx) => Results.Ok(new { ip = ctx.Connection.RemoteIpAddress?.ToString() }));

app.Run();
