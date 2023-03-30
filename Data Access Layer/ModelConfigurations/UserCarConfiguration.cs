using Data_Access_Layer.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data_Access_Layer.ModelConfigurations
{
    internal class UserCarConfiguration : IEntityTypeConfiguration<UserCarDB>
    {
        public void Configure(EntityTypeBuilder<UserCarDB> builder)
        {
            builder.ToTable("UserCar");

            builder.HasKey(p => p.Id);

            builder.Property(x => x.UserId).IsRequired();
        }
    }
}
