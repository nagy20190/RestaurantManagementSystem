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
    public class TableConfiguration:IEntityTypeConfiguration<Table>
    {
        public void Configure(EntityTypeBuilder<Table> builder)
        {
            // Configure the primary key
            builder.HasKey(t => t.ID);
            // Configure properties
            builder.Property(t => t.Capacity)
                .IsRequired()
                .HasMaxLength(12);

            builder.Property(t => t.IsAvailable)
                .IsRequired();

            builder.Property(t => t.ResturantID)
                .IsRequired();

            // Configure relationships
            builder.HasOne(t => t.Resturant)
                .WithMany(r => r.Tables)
                .HasForeignKey(t => t.ResturantID)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete if Resturant is deleted

        }
    }
}
