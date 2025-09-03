namespace Models.ResponsetModel;

public class DriverCurrectLocationResponseModel
{
    public string TripSID { get; set; }
    public string StartLocationName { get; set; }
    public string ToLocationName { get; set; }
    public decimal StartLatitude { get; set; }
    public decimal StartLongitude { get; set; }
    public decimal ToLatitude { get; set; }
    public decimal ToLongitude { get; set; }
    public decimal DriverLatitude { get; set; }
    public decimal DriverLongitude { get; set; }
}