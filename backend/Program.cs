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
        policy.WithOrigins(frontendUrl)
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

// Migration'ı arka planda çalıştırarak startup timeout'u engelle (Hata olsa bile uygulama açık kalsın)
_ = Task.Run(async () => {
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        var db = services.GetRequiredService<AppDbContext>();
        try 
        { 
            logger.LogInformation("Background migration starting...");
            await db.Database.MigrateAsync(); 
            logger.LogInformation("Database migration completed successfully.");
        } 
        catch (Exception ex) 
        { 
            logger.LogError(ex, "Background migration failed. Still waiting for Azure Firewall whitelist.");
        }
    }
});

// Swagger'ı prod'da da görmek istersen bunu kaldırma:
app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseCors("FrontendOnly");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Render'ın IP'sini öğrenmek için kritik endpoint:
app.MapGet("/ip", (HttpContext ctx) => Results.Ok(new { ip = ctx.Connection.RemoteIpAddress?.ToString() }));

app.Run();
