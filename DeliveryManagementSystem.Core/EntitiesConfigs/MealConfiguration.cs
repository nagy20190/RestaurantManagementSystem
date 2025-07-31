using DeliveryManagementSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryManagementSystem.Core.EntitiesConfigs
{
    public class MealConfiguration : IEntityTypeConfiguration<Meal>
    {
        public void Configure(EntityTypeBuilder<Meal> builder)
        {
            builder.HasKey(p => p.ID);

            builder.Property(p => p.ResturantID).IsRequired();

            builder.Property(p => p.Price).IsRequired().HasMaxLength(100000);

            builder.Property(p => p.Description)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(200);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.HasOne(p => p.Resturant)
                .WithMany(c => c.Meals)
                .HasForeignKey(e => e.ResturantID)
                .OnDelete(DeleteBehavior.Cascade);


            builder.HasMany(p => p.OrderItems)
                .WithOne(c => c.Meal)
                .HasForeignKey(v => v.MealId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Inventories)
                .WithOne(c => c.Meal)
                .HasForeignKey(v => v.MealID)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.RestaurantMenuCategory)
                .WithMany(c => c.Meals)
                .HasForeignKey(e => e.RestaurantMenuCategoryID)
                .OnDelete(DeleteBehavior.NoAction);


        }
    }
}
