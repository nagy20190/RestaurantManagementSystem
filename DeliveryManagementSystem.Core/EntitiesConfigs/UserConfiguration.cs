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
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(a => a.ID);

            builder.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(100);


            builder.Property(a => a.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.PasswordHash)
                .IsRequired()
                .HasMaxLength(256);

            builder.HasIndex(e => e.Email).IsUnique();

            builder.Property(a => a.Phone)
                .IsRequired()
                .HasMaxLength(15);

            builder.Property(a => a.Address)
                .IsRequired()
                .HasMaxLength(200);


            builder.Property(a => a.CreditCardNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(a => a.Role)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(15);

            builder.Property(a => a.ProfileImageUrl)
                .IsRequired(false)
                .HasMaxLength(100);


        }
    }
}
