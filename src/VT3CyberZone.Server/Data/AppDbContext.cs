using Microsoft.EntityFrameworkCore;
using VT3CyberZone.Server.Models;

namespace VT3CyberZone.Server.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<CafeShop> CafeShops => Set<CafeShop>();
    public DbSet<PlatformAdmin> PlatformAdmins => Set<PlatformAdmin>();
    public DbSet<SubscriptionPayment> SubscriptionPayments => Set<SubscriptionPayment>();
    public DbSet<ShopEvent> ShopEvents => Set<ShopEvent>();
    public DbSet<User> Users => Set<User>();
    public DbSet<MachineGroup> MachineGroups => Set<MachineGroup>();
    public DbSet<Workstation> Workstations => Set<Workstation>();
    public DbSet<UsageSession> UsageSessions => Set<UsageSession>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<ProductRecipe> ProductRecipes => Set<ProductRecipe>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CafeShop>(e =>
        {
            e.HasIndex(s => s.Email).IsUnique();
            e.HasIndex(s => s.LicenseKey).IsUnique();
            e.Property(s => s.Status).HasConversion<string>();
        });

        modelBuilder.Entity<PlatformAdmin>(e =>
        {
            e.HasIndex(a => a.Username).IsUnique();
        });

        modelBuilder.Entity<SubscriptionPayment>(e =>
        {
            e.Property(p => p.Amount).HasPrecision(12, 2);
            e.HasOne(p => p.CafeShop)
                .WithMany(s => s.SubscriptionPayments)
                .HasForeignKey(p => p.CafeShopId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ShopEvent>(e =>
        {
            e.Property(x => x.Type).HasConversion<string>();
            e.HasOne(x => x.CafeShop)
                .WithMany(s => s.Events)
                .HasForeignKey(x => x.CafeShopId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // User
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Username).IsUnique();
            e.Property(u => u.Role).HasConversion<string>();
            e.Property(u => u.Status).HasConversion<string>();
            e.HasOne(u => u.CafeShop)
                .WithMany(s => s.Users)
                .HasForeignKey(u => u.CafeShopId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MachineGroup>(e =>
        {
            e.HasOne(g => g.CafeShop)
                .WithMany()
                .HasForeignKey(g => g.CafeShopId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.HasOne(c => c.CafeShop)
                .WithMany()
                .HasForeignKey(c => c.CafeShopId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Ingredient>(e =>
        {
            e.HasOne(i => i.CafeShop)
                .WithMany()
                .HasForeignKey(i => i.CafeShopId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Workstation
        modelBuilder.Entity<Workstation>(e =>
        {
            e.HasIndex(w => w.MachineCode).IsUnique();
            e.Property(w => w.Status).HasConversion<string>();
            e.HasOne(w => w.Group)
             .WithMany(g => g.Workstations)
             .HasForeignKey(w => w.GroupId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // UsageSession — 2 FK về Users nên phải cấu hình tường minh
        modelBuilder.Entity<UsageSession>(e =>
        {
            e.Property(s => s.Status).HasConversion<string>();
            e.HasOne(s => s.User)
             .WithMany(u => u.Sessions)
             .HasForeignKey(s => s.UserId)
             .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(s => s.Workstation)
             .WithMany(w => w.Sessions)
             .HasForeignKey(s => s.WorkstationId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Order
        modelBuilder.Entity<Order>(e =>
        {
            e.HasIndex(o => o.OrderCode).IsUnique();
            e.Property(o => o.PaymentMethod).HasConversion<string>();
            e.Property(o => o.OrderStatus).HasConversion<string>();
            e.HasOne(o => o.User)
             .WithMany(u => u.Orders)
             .HasForeignKey(o => o.UserId)
             .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(o => o.Workstation)
             .WithMany(w => w.Orders)
             .HasForeignKey(o => o.WorkstationId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Transaction — có 2 FK về User (user + staff)
        modelBuilder.Entity<Transaction>(e =>
        {
            e.HasIndex(t => t.TransCode).IsUnique();
            e.Property(t => t.Type).HasConversion<string>();
            e.HasOne(t => t.CafeShop)
             .WithMany()
             .HasForeignKey(t => t.CafeShopId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.User)
             .WithMany()
             .HasForeignKey(t => t.UserId)
             .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(t => t.Staff)
             .WithMany()
             .HasForeignKey(t => t.StaffId)
             .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(t => t.Order)
             .WithMany(o => o.Transactions)
             .HasForeignKey(t => t.OrderId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ProductRecipe
        modelBuilder.Entity<ProductRecipe>(e =>
        {
            e.HasOne(r => r.Product)
             .WithMany(p => p.Recipes)
             .HasForeignKey(r => r.ProductId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.Ingredient)
             .WithMany(i => i.Recipes)
             .HasForeignKey(r => r.IngredientId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

