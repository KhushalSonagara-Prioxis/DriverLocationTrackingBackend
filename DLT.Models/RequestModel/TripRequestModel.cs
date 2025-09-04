namespace Models.RequestModel;

public class TripRequestModel
{
    public decimal StartLatitude { get; set; }
    public decimal StartLongitude { get; set; }
    public decimal ToLatitude { get; set; }
    public decimal ToLongitude { get; set; }
    public string StartLocationSID { get; set; }
    public string ToLocationSID { get; set; }
    public string DriverSID { get; set; }
    public string UserSID { get; set; }
}