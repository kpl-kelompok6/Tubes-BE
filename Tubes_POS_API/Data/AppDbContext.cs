using Microsoft.EntityFrameworkCore;
using Tubes_POS_API.Entities;
using Tubes_POS_API.Entities.Enums;

namespace Tubes_POS_API.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<TransactionItem> TransactionItems => Set<TransactionItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<TransactionHistory> TransactionHistories => Set<TransactionHistory>();
    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasIndex(m => m.Code).IsUnique();
            entity.HasIndex(m => m.Name);
            entity.HasIndex(m => m.Category);

            entity.HasData(
                new Menu { Id = 1, Code = "MENU-20250101-a1b2c3d4", Name = "Nasi Goreng Spesial", Price = 25000m, Category = "Makanan", IsAvailable = true, ImageUrl = "https://images.unsplash.com/photo-1512058564366-18510be2db19?w=400" },
                new Menu { Id = 2, Code = "MENU-20250101-e5f6g7h8", Name = "Es Teh Manis", Price = 5000m, Category = "Minuman", IsAvailable = true, ImageUrl = "https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=400" },
                new Menu { Id = 3, Code = "MENU-20250101-i9j0k1l2", Name = "Kopi Hitam", Price = 10000m, Category = "Minuman", IsAvailable = true, ImageUrl = "https://images.unsplash.com/photo-1509042239860-f550ce710b93?w=400" }
            );
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasIndex(t => t.TransactionCode).IsUnique();
            entity.HasIndex(t => t.Status);
            entity.HasIndex(t => t.CreatedAt);

            entity.HasOne(t => t.Cashier)
                  .WithMany(e => e.Transactions)
                  .HasForeignKey(t => t.CashierId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TransactionItem>(entity =>
        {
            entity.HasOne(ti => ti.Transaction)
                  .WithMany(t => t.Items)
                  .HasForeignKey(ti => ti.TransactionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ti => ti.Menu)
                  .WithMany()
                  .HasForeignKey(ti => ti.MenuId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasIndex(p => p.Code).IsUnique();
            entity.HasIndex(p => p.TransactionId).IsUnique();
            entity.HasOne(p => p.Transaction)
                  .WithOne(t => t.Payment)
                  .HasForeignKey<Payment>(p => p.TransactionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TransactionHistory>(entity =>
        {
            entity.HasIndex(h => h.Code).IsUnique();
            entity.HasIndex(h => h.TransactionDate);
        });

        modelBuilder.Entity<Employee>().HasData(
            new Employee
            {
                Id = 1,
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                DisplayName = "Admin Utama",
                Role = EmployeeRole.Admin,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
