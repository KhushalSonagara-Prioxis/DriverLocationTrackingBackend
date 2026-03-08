namespace Models.ResponsetModel;

public class DashboardResponseModel
{
}

public class DashboardTileResponseModel
{
    public int TotalNumberOfTrips { get; set; }
    public int InProgressTrips { get; set; }
    public int CompletedTrips { get; set; }
    public int PendingTrips { get; set; }
    public int NumberOfDriver { get; set; }
}

public class AdminDashboardBarChartResponseModel
{
    public List<TripPerDay> TripsPerDayData { get; set; }
    public List<TripPerDriver> TripsPerDriverData { get; set; }
}

public class TripPerDay
{
    // The SP formats this as "YYYY-MM-DD"
    public string Date { get; set; } 
    public int TotalTrips { get; set; }
}

public class TripPerDriver
{
    public string DriverName { get; set; }
    public int AssignedTrips { get; set; }
}