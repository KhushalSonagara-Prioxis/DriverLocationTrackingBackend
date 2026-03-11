using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DLT.Models.Models.DriverLocationTracking;

[Index("LocationSid", Name = "UQ__Location__EF33EBE21FD9F2B8", IsUnique = true)]
public partial class Location
{
    [Key]
    [Column("LocationID")]
    public int LocationId { get; set; }

    [Column("LocationSID")]
    [StringLength(50)]
    [Unicode(false)]
    public string LocationSid { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string LocationName { get; set; } = null!;

    [Column(TypeName = "decimal(9, 6)")]
    public decimal? Latitude { get; set; }

    [Column(TypeName = "decimal(9, 6)")]
    public decimal? Longitude { get; set; }

    [InverseProperty("StartLocationNavigation")]
    public virtual ICollection<Trip> TripStartLocationNavigations { get; set; } = new List<Trip>();

    [InverseProperty("ToLocationNavigation")]
    public virtual ICollection<Trip> TripToLocationNavigations { get; set; } = new List<Trip>();
}
