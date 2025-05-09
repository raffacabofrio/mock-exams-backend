﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain;

namespace MockExams.Infra.Database.Mapping;

public class UserMap
{
    public UserMap(EntityTypeBuilder<User> entityBuilder)
    {
        entityBuilder.HasKey(t => t.Id);

        entityBuilder.Property(t => t.Name)
            .HasColumnType("varchar(200)")
            .HasMaxLength(100)
            .IsRequired();

        entityBuilder.Property(t => t.Email)
            .HasColumnType("varchar(100)")
            .HasMaxLength(100)
            .IsRequired();

        entityBuilder.HasIndex(t => t.Email)
            .IsUnique();

        entityBuilder.Property(t => t.Linkedin)
            .HasColumnType("varchar(100)")
            .HasMaxLength(100);

        entityBuilder.Property(t => t.Phone)
            .HasColumnType("varchar(30)")
            .HasMaxLength(30);

        entityBuilder.HasIndex(t => t.Phone)
            .IsUnique();

        entityBuilder.Property(t => t.HashCodePassword)
            .HasColumnType("varchar(200)")
            .HasMaxLength(200);

        entityBuilder.Property(t => t.HashCodePasswordExpiryDate)
            .HasColumnType("datetime2(7)");

        entityBuilder.Property(t => t.Active)
            .HasDefaultValueSql("1");

        entityBuilder.Property(t => t.AllowSendingEmail)
            .ValueGeneratedNever()
            .HasDefaultValue(true);

        entityBuilder.Property(t => t.LastLogin)
            .HasDefaultValueSql("getdate()");

    }
}
