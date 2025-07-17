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
    public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
    {
        public void Configure(EntityTypeBuilder<Reservation> builder)
        {
            builder.HasKey(r => r.ID);

            builder.Property(r => r.ReservationDate)
                .IsRequired();

            builder.Property(r => r.TableID)
                .IsRequired();

            builder.Property(r => r.UserID)
                .IsRequired();

            builder.Property(r => r.NumberOfPeople)
                .IsRequired();

            builder.Property(r => r.QRCode)
                .IsRequired()
                .HasMaxLength(5000);

            builder.Property(r => r.Status)
                .IsRequired();

            builder.Property(r => r.StartDate)
                .IsRequired();

            builder.Property(r => r.EndDate)
                .IsRequired();

            // Relationships
            builder.HasOne(r => r.Table)
                .WithMany(t => t.Reservations)
                .HasForeignKey(r => r.TableID)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
