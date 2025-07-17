using DeliveryManagementSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryManagementSystem.Core.EntitiesConfigs
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            // Primary Key
            builder.HasKey(o => o.ID);
            // Properties
            builder.Property(o => o.OrderNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(o => o.TotalAmount);

            builder.Property(o => o.Status)
                .IsRequired();
            builder.Property(o => o.OrderDate);

            builder.Property(o => o.DeliveryAddress)
                .IsRequired()
                .HasMaxLength(255);
            builder.Property(o => o.DeliveryFee);

            builder.Property(o => o.PaymentMethod)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(o => o.IsPaid)
                .IsRequired();
            builder.Property(o => o.CreatedAt);

            // Relationships
            builder.HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserID);

            builder.HasOne(o => o.Payment)
                .WithOne(p => p.Order)
                .HasForeignKey<Payment>(p => p.OrderID);

            builder.HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId);

        }
    }
}
