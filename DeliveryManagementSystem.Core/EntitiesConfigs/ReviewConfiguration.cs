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
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            // Configure the primary key
            builder.HasKey(r => r.ID);
            // Configure properties
            builder.Property(r => r.Comment)
                .IsRequired()
                .HasMaxLength(500); // Assuming a maximum length for comments

            builder.Property(r => r.Rating)
                .IsRequired();

            builder.Property(r => r.CreatedAt)
                .IsRequired();

            // Configure relationships
            builder.HasOne(r => r.User)
                .WithMany(u => u.Reviews) // Assuming User has a collection of Reviews
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete if User is deleted

            builder.HasOne(r => r.Resturant)
                .WithMany(rest => rest.Reviews) // Assuming Resturant has a collection of Reviews
                .HasForeignKey(r => r.ResturantID)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete if Resturant is deleted
        }
    }
}
