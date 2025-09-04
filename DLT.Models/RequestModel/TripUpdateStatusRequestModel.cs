namespace Models.RequestModel;

public class TripUpdateStatusRequestModel
{
    public int TripUpdateStatus { get; set; }
    public decimal TripUpdatedLatitude { get; set; }
    public decimal TripUpdatedLongitude { get; set; }
    public string Note { get; set; }
}