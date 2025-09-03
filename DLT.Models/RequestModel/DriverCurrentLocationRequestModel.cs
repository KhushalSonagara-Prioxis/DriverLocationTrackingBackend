namespace Models.RequestModel;

public class DriverCurrentLocationRequestModel
{
    public string  TripSId { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}