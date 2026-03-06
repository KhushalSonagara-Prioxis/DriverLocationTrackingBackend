namespace Models.ResponsetModel;

public class DashboadResponseModel
{
    public int TotalNumberOfTrips { get; set; }
    public int InProgressTrips { get; set; }
    public int CompletedTrips { get; set; }
    public int PendingTrips  { get; set; }
    public int NumberOfDriver { get; set; }
}