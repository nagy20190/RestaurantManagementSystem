using DeliveryManagementSystem.Core.Entities;
using DeliveryManagementSystem.Core.EntitiesConfigs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryManagementSystem.DAL.Contexts
{
    public class ApplicationDbContext : IdentityDbContext<User, Roles, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
          : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Meal> Meals { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<RestaurantMenuCategory> RestaurantMenuCategories { get; set; }
        public DbSet<Restaurant> Resturants { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Roles> Roles { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // EntityConfigs classes 
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CategoryConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MealConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderItemConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReservationConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ResturantConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReviewConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TableConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserConfiguration).Assembly);
        }
    }
}
