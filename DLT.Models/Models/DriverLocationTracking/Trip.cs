using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DLT.Models.Models.DriverLocationTracking;

[Index("TripSid", Name = "UQ__Trips__655B93F4A10BB6D1", IsUnique = true)]
public partial class Trip
{
    [Key]
    [Column("TripID")]
    public int TripId { get; set; }

    [Column("TripSID")]
    [StringLength(50)]
    [Unicode(false)]
    public string TripSid { get; set; } = null!;

    [Column(TypeName = "decimal(10, 7)")]
    public decimal StartLatitude { get; set; }

    [Column(TypeName = "decimal(10, 7)")]
    public decimal StartLongitude { get; set; }

    [Column(TypeName = "decimal(10, 7)")]
    public decimal ToLatitude { get; set; }

    [Column(TypeName = "decimal(10, 7)")]
    public decimal ToLongitude { get; set; }

    public int StartLocation { get; set; }

    public int ToLocation { get; set; }

    [Column("DriverID")]
    public int? DriverId { get; set; }

    public int CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    public int? LastModifiedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastModifiedDate { get; set; }

    public int TripStatus { get; set; }

    public int Status { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("TripCreatedByNavigations")]
    public virtual User CreatedByNavigation { get; set; } = null!;

    [ForeignKey("DriverId")]
    [InverseProperty("TripDrivers")]
    public virtual User? Driver { get; set; }

    [InverseProperty("Trip")]
    public virtual ICollection<DriverCurrentLocation> DriverCurrentLocations { get; set; } = new List<DriverCurrentLocation>();

    [ForeignKey("LastModifiedBy")]
    [InverseProperty("TripLastModifiedByNavigations")]
    public virtual User? LastModifiedByNavigation { get; set; }

    [ForeignKey("StartLocation")]
    [InverseProperty("TripStartLocationNavigations")]
    public virtual Location StartLocationNavigation { get; set; } = null!;

    [ForeignKey("ToLocation")]
    [InverseProperty("TripToLocationNavigations")]
    public virtual Location ToLocationNavigation { get; set; } = null!;

    [InverseProperty("Trip")]
    public virtual ICollection<TripUpdate> TripUpdates { get; set; } = new List<TripUpdate>();
}
