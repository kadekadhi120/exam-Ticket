using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Ticket.Entites;

public partial class AccelokaDbContext : DbContext
{

    public AccelokaDbContext(DbContextOptions<AccelokaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BookedTiket> BookedTikets { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Data source=.\\SQLEXPRESS;initial catalog=AccelokaDB;trusted_connection=true;TrustServerCertificate=True");
        }
    }

      

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BookedTiket>(entity =>
        {
            entity.HasKey(e => e.BookedTicketId).HasName("PK__BookedTi__9110472FED015B6C");

            entity.ToTable("BookedTiket");

            entity.Property(e => e.BookedTicketId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.TanggalBooking)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Ticket).WithMany(p => p.BookedTikets)
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("FK__BookedTik__Ticke__7C4F7684");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Category__19093A0B9E221BA4");

            entity.ToTable("Category");

            entity.HasIndex(e => e.CategoryName, "UQ__Category__8517B2E0849E3098").IsUnique();

            entity.Property(e => e.CategoryId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CategoryName).HasMaxLength(255);
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("PK__Ticket__712CC60749FB6771");

            entity.ToTable("Ticket");

            entity.HasIndex(e => e.TicketCode, "UQ__Ticket__598CF7A300447DF7").IsUnique();

            entity.Property(e => e.TicketId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TicketCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TicketName).HasMaxLength(255);

            entity.HasOne(d => d.Category).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Ticket__Category__76969D2E");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
