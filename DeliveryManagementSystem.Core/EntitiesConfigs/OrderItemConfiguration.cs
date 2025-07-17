using DeliveryManagementSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryManagementSystem.Core.EntitiesConfigs
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<OrderItem> builder)
        {
            // Configure primary key
            builder.HasKey(oi => oi.ID);
            // Configure properties
            builder.Property(oi => oi.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(oi => oi.UnitPrice)
                .IsRequired();

            builder.Property(oi => oi.Quantity)
                .IsRequired();
            // Configure relationships
            builder.HasOne(oi => oi.Meal)
                .WithMany(m => m.OrderItems)
                .HasForeignKey(oi => oi.MealId);

            builder.HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);
        }

    }
}
