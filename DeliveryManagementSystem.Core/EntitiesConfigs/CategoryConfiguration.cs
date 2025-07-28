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
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {

        public void Configure(EntityTypeBuilder<Category> builder)
        {
            // 
            builder.HasKey(p => p.ID);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion<string>();

            builder.Property(p => p.ImageUrl)
                .IsRequired(false)
                .HasMaxLength(255)
                .IsUnicode(false); // optional: for URL optimization (if no special characters)


            builder.Property(p => p.Description)
                .IsRequired()
                .HasMaxLength(255);

            builder.HasMany(p => p.Resturants)
                .WithOne(c => c.Category)
                .HasForeignKey(x => x.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
