namespace Models.ResponsetModel;


public class DriverDetailsResponseModel
{
    public string UserSID { get; set; }
    public string UserName { get; set; }
    public string PhoneNumber { get; set; }
    public string UserEmail { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } // Active or Inactive
    public DateTime? CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}

public class DriverDropDownResponseModel
{
    public string UserSID { get; set; }
    public string UserName { get; set; }
}