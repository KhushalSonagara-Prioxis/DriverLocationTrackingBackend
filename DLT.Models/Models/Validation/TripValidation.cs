using FluentValidation;
using Models.RequestModel;

namespace Models.Models.Validation;

public class TripValidation : AbstractValidator<TripRequestModel>
{
    public TripValidation()
    {
        RuleFor(x=>x.StartLatitude).NotNull().NotEmpty().WithMessage("Start Latitude is required.").GreaterThan(
            -1).WithMessage("Start Latitude must be greater than or equal to 0.");
        RuleFor(x=>x.StartLongitude).NotNull().NotEmpty().WithMessage("Start Longitude is required.").GreaterThan(
            -1).WithMessage("Start Longitude must be greater than or equal to 0.");;
        RuleFor(x=>x.ToLatitude).NotNull().NotEmpty().WithMessage("To Latitude is required.").GreaterThan(
            -1).WithMessage("To Latitude must be greater than or equal to 0.");;
        RuleFor(x=>x.ToLongitude).NotNull().NotEmpty().WithMessage("To Longitude is required.").GreaterThan(
            -1).WithMessage("To Longitude must be greater than or equal to 0.");;;
        RuleFor(x => x.StartLocationSID).NotNull().NotEmpty().WithMessage("Start Location is required.");
        RuleFor(x => x.ToLocationSID).NotNull().NotEmpty().WithMessage("To Location is required.");
        RuleFor(x => x.DriverSID).NotNull().NotEmpty().WithMessage("Driver is required.");
        
    }
}