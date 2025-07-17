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
    public class ResturantConfiguration : IEntityTypeConfiguration<Resturant>
    {
        public void Configure(EntityTypeBuilder<Resturant> builder)
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

            builder.HasOne(r => r.Category)
                .WithMany(c => c.Resturants)
                .HasForeignKey(r => r.CategoryID)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.ResturantCategories)
                .WithOne(rc => rc.Resturant)
                .HasForeignKey(rc => rc.ResturantID)
                .OnDelete(DeleteBehavior.Cascade);


        }

    }
}
