using DeliveryManagementSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryManagementSystem.Core.EntitiesConfigs
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(p => p.ID);

            builder.Property(p => p.Amount).IsRequired();

            builder.Property(p => p.PaymentDate).IsRequired();

            builder.Property(p => p.PaymentMethod)
                .HasConversion<string>()
                .IsRequired().HasMaxLength(50);

            builder.Property(p => p.Status).IsRequired().HasMaxLength(20);

            // Additional configurations can be added here as needed
            
            builder.Property(p => p.TransactionID)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasOne(p => p.Order)
                .WithOne(o => o.Payment)
                .HasForeignKey<Payment>(p => p.OrderID);


        }
    }
}
