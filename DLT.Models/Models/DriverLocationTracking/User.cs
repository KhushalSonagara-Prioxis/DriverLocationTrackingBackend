using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DLT.Models.Models.DriverLocationTracking;

[Index("UserEmail", Name = "UQ__Users__08638DF8D11950B1", IsUnique = true)]
[Index("UserSid", Name = "UQ__Users__2B6A7E55B4EE2A91", IsUnique = true)]
[Index("PhoneNumber", Name = "UQ__Users__85FB4E38579D69F9", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("UserID")]
    public int UserId { get; set; }

    [Column("UserSID")]
    [StringLength(50)]
    [Unicode(false)]
    public string UserSid { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string UserName { get; set; } = null!;

    [StringLength(10)]
    [Unicode(false)]
    public string PhoneNumber { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string UserEmail { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string PasswordHash { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastModifiedDate { get; set; }

    public int Status { get; set; }

    public int Role { get; set; }

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Trip> TripCreatedByNavigations { get; set; } = new List<Trip>();

    [InverseProperty("Driver")]
    public virtual ICollection<Trip> TripDrivers { get; set; } = new List<Trip>();

    [InverseProperty("LastModifiedByNavigation")]
    public virtual ICollection<Trip> TripLastModifiedByNavigations { get; set; } = new List<Trip>();

    [InverseProperty("Driver")]
    public virtual ICollection<TripUpdate> TripUpdates { get; set; } = new List<TripUpdate>();
}
