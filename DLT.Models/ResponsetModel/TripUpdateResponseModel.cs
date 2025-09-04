namespace Models.ResponsetModel;

public class TripUpdateResponseModel
{
    public string TripUpdatesSID { get; set; }
    public string DriverName  { get; set; }
    public int TripUpdatesStatus { get; set; }
    public decimal TripUpdatedLatitude { get; set; }
    public decimal TripUpdatedLongitude { get; set; }
    public string Note { get; set; }
    public DateTime TimeStamp { get; set; }
}