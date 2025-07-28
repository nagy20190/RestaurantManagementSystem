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
            builder.Property(a => a.Address)
                .IsRequired(false)
                .HasMaxLength(200);

            builder.Property(a => a.CreditCardNumber)
                .IsRequired(false)
                .HasMaxLength(20);

            builder.Property(a => a.ProfileImageUrl)
                .IsRequired(false)
                .HasMaxLength(100);


        }
    }
}
