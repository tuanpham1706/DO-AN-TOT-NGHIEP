using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VT3CyberZone.Server.Data;
using VT3CyberZone.Server.Hubs;
using VT3CyberZone.Server.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Missing ConnectionStrings:DefaultConnection. Configure it using user-secrets or an environment variable.");
}

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key must be configured with a secret of at least 32 bytes using user-secrets or an environment variable.");
}

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("Missing Jwt:Issuer configuration.");
var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("Missing Jwt:Audience configuration.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0)))
        .UseSnakeCaseNamingConvention());
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    await CafeShopSchemaInitializer.InitializeAsync(db);

    var legacyShop = await db.CafeShops.SingleAsync(s => s.Email == "legacy-shop@local.invalid");

    var defaultAdminUsername = builder.Configuration["App:DefaultAdminUsername"] ?? "admin";
    var defaultAdminPassword = builder.Configuration["App:DefaultAdminPassword"] ?? "Admin@123";

    var hasAdmin = await db.Users.AnyAsync(u => u.Role == UserRole.ADMIN);
    if (!hasAdmin)
    {
        db.Users.Add(new User
        {
            CafeShopId = legacyShop.Id,
            Username = defaultAdminUsername,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(defaultAdminPassword),
            FullName = "Quản trị hệ thống",
            Phone = "0900000000",
            Role = UserRole.ADMIN,
            Balance = 0m,
            Status = UserStatus.ACTIVE
        });

        await db.SaveChangesAsync();
    }

    var platformAdminUsername = builder.Configuration["PlatformAdmin:Username"];
    var platformAdminPassword = builder.Configuration["PlatformAdmin:Password"];
    if (string.IsNullOrWhiteSpace(platformAdminUsername) != string.IsNullOrWhiteSpace(platformAdminPassword))
    {
        throw new InvalidOperationException(
            "Configure both PlatformAdmin:Username and PlatformAdmin:Password, or leave both unset.");
    }

    if (!string.IsNullOrWhiteSpace(platformAdminUsername)
        && !await db.PlatformAdmins.AnyAsync())
    {
        db.PlatformAdmins.Add(new PlatformAdmin
        {
            Username = platformAdminUsername.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(platformAdminPassword!)
        });
        await db.SaveChangesAsync();
    }
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", async (AppDbContext db) =>
    await db.Database.CanConnectAsync()
        ? Results.Ok(new { status = "ok" })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable));
app.MapGet("/admin", () => Results.Redirect("/admin.html"));
app.MapGet("/shop", () => Results.Redirect("/shop.html"));

app.MapControllers();
app.MapHub<CyberHub>("/hubs/cyber").RequireAuthorization();

app.Run();
