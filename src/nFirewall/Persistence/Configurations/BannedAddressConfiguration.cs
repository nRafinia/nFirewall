using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using nFirewall.Domain.Entities;

namespace nFirewall.Persistence.Configurations;

public class BannedAddressConfiguration:IEntityTypeConfiguration<BannedAddress>
{
    public void Configure(EntityTypeBuilder<BannedAddress> builder)
    {
        builder.ToTable("BannedAddresses");
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Id)
            .ValueGeneratedOnAdd()
            .IsRequired();
        
        builder.HasIndex(c => c.ExpireDate);

        builder.Property(c => c.Ip).IsRequired();
        builder.Property(c => c.Permanent).IsRequired();
        builder.Property(c => c.ExpireDate).IsRequired(false);
    }
}