﻿using DataAccess.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Contexts
{
    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> modelBuilder)
        {
            modelBuilder
                .Property(c => c.CreatedAt)
                .HasDefaultValueSql("NOW()");

            modelBuilder
                .HasIndex(t => t.Name)
                .IsUnique();
        }
    }
}