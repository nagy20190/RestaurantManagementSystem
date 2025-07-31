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
    public class ResturantConfiguration : IEntityTypeConfiguration<Restaurant>
    {
        public void Configure(EntityTypeBuilder<Restaurant> builder)
        {
            builder.HasKey(r => r.ID);

            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(r => r.Address).
                IsRequired().
                HasMaxLength(200);

            builder.Property(r => r.Phone)
                .IsRequired()
                .HasMaxLength(15);

            builder.Property(r => r.URLPhoto)
                .IsRequired(false)
                .HasMaxLength(200);

            builder.Property(r => r.AverageRating)
                .IsRequired();


            builder.Property(r => r.DeliveryFee)
              .HasColumnType("decimal(8, 2)")
              .IsRequired();

            builder.Property(r => r.MinimumOrderAmount)
               .IsRequired();

            builder.Property(r => r.PreparationTime)
            .IsRequired();


            builder.Property(r => r.OpeningTime)
              .HasColumnType("time")
              .IsRequired();

            // ClosingTime
            builder.Property(r => r.ClosingTime)
                   .HasColumnType("time")
                   .IsRequired();

            builder.HasOne(r => r.Category)
                .WithMany(c => c.Resturants)
                .HasForeignKey(r => r.CategoryID)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.Tables)
                .WithOne(t => t.Resturant)
                .HasForeignKey(t => t.ResturantID)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.Reviews)
                .WithOne(rv => rv.Resturant)
                .HasForeignKey(rv => rv.ResturantID)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.Meals)
                .WithOne(m => m.Resturant)
                .HasForeignKey(m => m.ResturantID)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.Orders)
                .WithOne(o => o.Resturant)
                .HasForeignKey(o => o.ResturantID)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(r => r.ResturantCategories)
                .WithOne(rc => rc.Resturant)
                .HasForeignKey(rc => rc.ResturantID)
                .OnDelete(DeleteBehavior.Cascade);


        }

    }
}
