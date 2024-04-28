using Microsoft.EntityFrameworkCore;
using Neo4jClient.DataAnnotations.Cypher.Functions;
using web_project_BE.Models;

namespace web_project_BE.Data
{

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Size> Sizes { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Cart_Detail> Cart_details { get; set; }
        public DbSet<Order_Detail> Order_details { get; set; }
        public DbSet<Roles> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>().ToTable("Accounts");
            modelBuilder.Entity<Cart>().ToTable("Carts");
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<Category>().ToTable("Categories");
            modelBuilder.Entity<Roles>().ToTable("Roles");
            modelBuilder.Entity<Supplier>().ToTable("Suppliers");
            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<Order_Detail>().ToTable("Order_Details");
            modelBuilder.Entity<Cart_Detail>().ToTable("Cart_Details");
            modelBuilder.Entity<Size>().ToTable("Sizes");
            modelBuilder.Entity<Image>().ToTable("Images");

            modelBuilder.Entity<Account>()
                .HasOne(m => m.role)
                .WithMany(u => u.accounts)
                .HasForeignKey(m => m.account_role_id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Account>()
                .HasOne(c => c.Cart)
                .WithOne(a => a.Account)
                .HasForeignKey<Cart>(a => a.account_id)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
