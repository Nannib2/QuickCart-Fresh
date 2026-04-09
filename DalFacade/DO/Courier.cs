
using DalApi;
using System.Xml.Linq;

namespace DO;

/// <summary>
/// Represents a courier entity containing all essential details about 
/// the delivery personnel, including contact information, activity status, 
/// delivery preferences, and employment data.
/// </summary>
/// <param name="Id">Unique identifier of the courier.</param>
/// <param name="EmploymentStartDateTime">Date and time when the courier started working for the company.</param>
/// param name="NameCourier">Full name of the courier.</param>
/// param name="PhoneNumber">Phone number of the courier.</param>
/// param name="EmailCourier">Email address of the courier.</param>
/// param name="PasswordCourier">Password used by the courier to access the system.</param>
/// param name="Active">Indicates whether the courier is currently active in the system.</param>
/// param name="PersonalMaxAirDistance">The personal maximum air distance (in kilometers) that the courier is willing to deliver to. If null, there is no personal distance limit.</param>
/// param name="CourierDeliveryType">Specifies the courier's delivery type (, car, bike, foot, ).</param>
/// </summary>
public record Courier
(
    int Id,
    DateTime EmploymentStartDateTime,
    string NameCourier,
    string PhoneNumber,
    string EmailCourier,
    string PasswordCourier,
    bool Active,
    double? PersonalMaxAirDistance,
    DeliveryTypeMethods CourierDeliveryType

)
{

    
    public Courier() : this(
             0,
             DateTime.Now,
             string.Empty,
             string.Empty,
             string.Empty,
             string.Empty,
             true,
             null,
             DeliveryTypeMethods.Car
             )
    { }
}