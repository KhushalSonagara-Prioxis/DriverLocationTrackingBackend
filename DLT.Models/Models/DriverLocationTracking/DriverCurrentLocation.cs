using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DLT.Models.Models.DriverLocationTracking;

[Table("DriverCurrentLocation")]
[Index("DriverCurrentLocationSid", Name = "UQ__DriverCu__849F8A06FB092BCF", IsUnique = true)]
public partial class DriverCurrentLocation
{
    [Key]
    [Column("DriverCurrentLocationID")]
    public int DriverCurrentLocationId { get; set; }

    [Column("DriverCurrentLocationSID")]
    [StringLength(50)]
    [Unicode(false)]
    public string DriverCurrentLocationSid { get; set; } = null!;

    [Column("TripID")]
    public int TripId { get; set; }

    [Column(TypeName = "decimal(10, 7)")]
    public decimal Latitude { get; set; }

    [Column(TypeName = "decimal(10, 7)")]
    public decimal Longitude { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastUpdate { get; set; }

    [ForeignKey("TripId")]
    [InverseProperty("DriverCurrentLocations")]
    public virtual Trip Trip { get; set; } = null!;
}
