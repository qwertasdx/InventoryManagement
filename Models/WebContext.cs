using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Models;

public partial class WebContext : DbContext
{
    public WebContext(DbContextOptions<WebContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Department> Department { get; set; }

    public virtual DbSet<ItemBasic> ItemBasic { get; set; }

    public virtual DbSet<ItemStock> ItemStock { get; set; }

    public virtual DbSet<ItemTrans> ItemTrans { get; set; }

    public virtual DbSet<ItemTrans2> ItemTrans2 { get; set; }

    public virtual DbSet<User> User { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.DepartmentName)
                .HasMaxLength(10)
                .IsFixedLength();
        });



        modelBuilder.Entity<ItemBasic>(entity =>
        {
            entity.HasKey(e => e.ItemCode);

            entity.Property(e => e.ItemCode)
                .HasMaxLength(8)
                .IsFixedLength();
            entity.Property(e => e.ItemName)
                .HasMaxLength(20)
                .IsFixedLength();
            entity.Property(e => e.Spec)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Status)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.SystemTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SystemUser)
                .HasMaxLength(10)
                .IsFixedLength();
        });

        modelBuilder.Entity<ItemStock>(entity =>
        {
            entity.HasKey(e => e.ItemCode);

            entity.Property(e => e.ItemCode)
                .HasMaxLength(8)
                .IsFixedLength();
            entity.Property(e => e.Status)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.SystemTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SystemUser)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Unit)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
        });

        modelBuilder.Entity<ItemTrans>(entity =>
        {
            entity.HasKey(e => e.TransNo);

            entity.Property(e => e.ItemCode)
                .HasMaxLength(8)
                .IsFixedLength();
            entity.Property(e => e.SystemTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SystemUser)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Type)
                .HasMaxLength(3)
                .IsFixedLength();
            entity.Property(e => e.Unit)
                .HasMaxLength(2)
                .IsFixedLength();
        });

        modelBuilder.Entity<ItemTrans2>(entity =>
        {
            entity.HasKey(e => e.TransNo);

            entity.Property(e => e.ItemCode).HasMaxLength(8);
            entity.Property(e => e.SystemTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SystemUser)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Type)
                .HasMaxLength(3)
                .IsFixedLength();
            entity.Property(e => e.Unit)
                .HasMaxLength(2)
                .IsFixedLength();
            entity.Property(e => e.Reason)
                .HasMaxLength(50)
                .IsFixedLength();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.EmployeeId);

            entity.Property(e => e.EmployeeId).HasMaxLength(10);
            entity.Property(e => e.Account).HasMaxLength(20);
            entity.Property(e => e.EmployeeName)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Password).HasMaxLength(10);
            entity.Property(e => e.Role)
                .HasMaxLength(10)
                .IsFixedLength();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
