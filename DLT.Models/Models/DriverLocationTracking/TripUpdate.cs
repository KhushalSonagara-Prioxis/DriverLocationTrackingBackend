using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DLT.Models.Models.DriverLocationTracking;

[Index("TripUpdatesSid", Name = "UQ__TripUpda__F59F0F74A88FA374", IsUnique = true)]
public partial class TripUpdate
{
    [Key]
    [Column("TripUpdatesID")]
    public int TripUpdatesId { get; set; }

    [Column("TripUpdatesSID")]
    [StringLength(50)]
    [Unicode(false)]
    public string TripUpdatesSid { get; set; } = null!;

    [Column("TripID")]
    public int TripId { get; set; }

    [Column("DriverID")]
    public int DriverId { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string TripUpdatesStatus { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string? Note { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? TimeStamp { get; set; }

    [ForeignKey("DriverId")]
    [InverseProperty("TripUpdates")]
    public virtual User Driver { get; set; } = null!;

    [ForeignKey("TripId")]
    [InverseProperty("TripUpdates")]
    public virtual Trip Trip { get; set; } = null!;
}
