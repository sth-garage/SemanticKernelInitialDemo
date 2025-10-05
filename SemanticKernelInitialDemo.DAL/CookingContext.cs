using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SemanticKernelInitialDemo.DAL;

public partial class CookingContext : DbContext
{
    public CookingContext()
    {
    }

    public CookingContext(DbContextOptions<CookingContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CustomRecipe> CustomRecipes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlServer("Data Source=127.0.0.1;Initial Catalog=Cooking;User Id=cookingAdmin;Password=Testing777!!;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CustomRecipe>(entity =>
        {
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FilePath)
                .HasMaxLength(1000)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
