using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Teste.ScoreAPI.Domain.Entities;

namespace Teste.ScoreAPI.Infrastructure.Configurations;

public sealed class CustomerEntityTypeConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.HasIndex(c => c.Cpf)
            .IsUnique()
            .HasDatabaseName("IX_Customers_Cpf");

        builder.HasIndex(c => c.Email)
            .HasDatabaseName("IX_Customers_Email");

        builder.Property(c => c.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(c => c.Name)
            .HasMaxLength(200);

        builder.Property(c => c.Email)
            .HasMaxLength(200);

        builder.Property(c => c.BirthDate)
            .IsRequired();

        builder.Property(c => c.Cpf)
            .HasMaxLength(11)
            .IsRequired();

        builder.Property(c => c.AnnualIncome)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.OwnsOne(c => c.Phone, phone =>
        {
            phone.Property(p => p.Ddd)
                .HasColumnName("Phone_Ddd")
                .HasMaxLength(2)
                .IsRequired();

            phone.Property(p => p.Number)
                .HasColumnName("Phone_Number")
                .HasMaxLength(9)
                .IsRequired();
        });

        builder.OwnsOne(c => c.Address, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("Address_Street")
                .HasMaxLength(200);

            address.Property(a => a.Number)
                .HasColumnName("Address_Number")
                .HasMaxLength(20);

            address.Property(a => a.Complement)
                .HasColumnName("Address_Complement")
                .HasMaxLength(100);

            address.Property(a => a.ZipCode)
                .HasColumnName("Address_ZipCode")
                .HasMaxLength(9);

            address.Property(a => a.State)
                .HasColumnName("Address_State")
                .HasMaxLength(2)
                .IsRequired();
        });
    }
}