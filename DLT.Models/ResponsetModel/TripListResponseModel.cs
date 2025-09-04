namespace Models.ResponsetModel;

public class TripListResponseModel
{
    public string TripSID { get; set; }
    public string StartLocationName { get; set; }
    public string ToLocationName { get; set; }
    public string TripStatusName { get; set; }
    public int TripStatus { get; set; }
    public string DriverName { get; set; }
    public DateTime LastModifiedDate { get; set; }
}