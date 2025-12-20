using APICat.Domain.Entities; // Asegúrate de ajustar este namespace donde tengas la entidad Breed
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APICat.Infrastructure.Configurations
{
    public class BreedConfiguration : IEntityTypeConfiguration<Breed>
    {
        public void Configure(EntityTypeBuilder<Breed> builder)
        {
            builder.ToTable("Breeds", "dbo");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Origin)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Temperament)
                .HasMaxLength(300)
                .IsRequired();

            builder.Property(x => x.Wikipedia_url)
                .HasMaxLength(100)
                .IsRequired();
        }
    }
}