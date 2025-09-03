using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DLT.Models.Models.DriverLocationTracking;

public partial class DriverLocationTrackingDbContext : DbContext
{
    public DriverLocationTrackingDbContext(DbContextOptions<DriverLocationTrackingDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DriverCurrentLocation> DriverCurrentLocations { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<Trip> Trips { get; set; }

    public virtual DbSet<TripUpdate> TripUpdates { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DriverCurrentLocation>(entity =>
        {
            entity.HasKey(e => e.DriverCurrentLocationId).HasName("PK__DriverCu__5190FB688597CBE3");

            entity.Property(e => e.LastUpdate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Trip).WithMany(p => p.DriverCurrentLocations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DriverCurrentLocation_Trip");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.LocationId).HasName("PK__Location__E7FEA477238601DD");
        });

        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasKey(e => e.TripId).HasName("PK__Trips__51DC711ED95605D7");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TripCreatedByNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Trips_CreatedBy");

            entity.HasOne(d => d.Driver).WithMany(p => p.TripDrivers).HasConstraintName("FK_Trips_Driver");

            entity.HasOne(d => d.LastModifiedByNavigation).WithMany(p => p.TripLastModifiedByNavigations).HasConstraintName("FK_Trips_LastModifiedBy");

            entity.HasOne(d => d.StartLocationNavigation).WithMany(p => p.TripStartLocationNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Trips_StartLocation");

            entity.HasOne(d => d.ToLocationNavigation).WithMany(p => p.TripToLocationNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Trips_ToLocation");
        });

        modelBuilder.Entity<TripUpdate>(entity =>
        {
            entity.HasKey(e => e.TripUpdatesId).HasName("PK__TripUpda__BB9599DBD27BE19B");

            entity.Property(e => e.TimeStamp).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Driver).WithMany(p => p.TripUpdates)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TripUpdates_Driver");

            entity.HasOne(d => d.Trip).WithMany(p => p.TripUpdates)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TripUpdates_Trip");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC1A4016E7");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("(getdate())");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
