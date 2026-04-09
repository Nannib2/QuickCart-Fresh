using System.Xml.Linq;
namespace DO
{
    /// <summary>
    /// Represents an order entity containing all essential details about 
    /// a customer order — including location, customer information, 
    /// and package characteristics.
    /// </summary>
    /// <param name="Id">Unique identifier of the order.</param>
    /// <param name="OpenOrderDateTime">Date and time when the order was created or opened.</param>
    /// param name="OrderType">Specifies the type of order (e.g., standard, express, same-day).</param>
    /// param name="ShortOrderDescription">A brief description of the order contents or special instructions.</param>
    /// param name="OrderAddress">The delivery address for the order.</param>
    /// param name="Latitude">The latitude coordinate of the delivery address.</param>
    /// param name="Longitude">The longitude coordinate of the delivery address.</param>
    /// param name="CustomerFullName">Full name of the customer placing the order.</param>
    /// param name="CustomerPhone">Phone number of the customer.</param>
    /// param name="FreeShippingEligibility">Indicates whether the order qualifies for free shipping.</param>
    /// param name="AmountItems">The total number of items included in the order.</param>
    public record Order
    (
        int Id,
        DateTime OpenOrderDateTime,
        OrderRequirements OrderType,
        string? ShortOrderDescription,
        string OrderAddress,
        double Latitude,
        double Longitude,
        string CustomerFullName,
        string CustomerPhone,
        bool FreeShippingEligibility,
        int AmountItems = 0
    )
    {
        

        public Order() : this(
                 0,
                 DateTime.Now,
                 OrderRequirements.Mixed,
                 null,
                 string.Empty,
                 0.0,
                 0.0,
                 string.Empty,
                 string.Empty,
                 false,
                 0
                 )
        { }
    }
}
