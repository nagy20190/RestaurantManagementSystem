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
    public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
    {
        public void Configure(EntityTypeBuilder<Inventory> builder)
        {
            // Configure the primary key
            builder.HasKey(i => i.ID);
            // Configure properties
            builder.Property(i => i.Quantity)
                .IsRequired();
            builder.Property(i => i.LastUpdated)
                .IsRequired();

            // Configure relationships
            builder.HasOne(i => i.Meal)
                .WithMany(m => m.Inventories)
                .HasForeignKey(i => i.MealID)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
