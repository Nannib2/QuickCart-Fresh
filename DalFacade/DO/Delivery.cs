using System.Xml.Linq;
namespace DO;
/// <summary>
/// Represents a delivery entity containing all the necessary information 
/// about a specific delivery process — including the order, courier, 
/// delivery type, timing, and distance details.
/// </summary>
/// <param name="Id">Unique identifier of the delivery.</param>
/// <param name="OrderId">Identifier of the order being delivered.</param>
/// <param name="CourierId">Identifier of the courier responsible for the delivery.</param>
/// <param name="DeliveryType">Specifies the type of delivery (motorcycle, car, bike, foot).</param>
/// <param name="OrderStartDateTime">Date and time when the delivery started.</param>
/// <param name="DeliveryDistanceKm">actual distance (in kilometers) for this delivery.</param>
/// <param name="DeliveryTypeEnding">Indicates the completion mode, if applicable.</param>
/// <param name="OrderEndDateTime">Date and time when the delivery was completed, if applicable.</param>
public record Delivery
(
    /// <summary>
    /// Unique identifier of the delivery.
    /// <summary>
    int Id,

    /// <summary>
    /// the identifier of the order being delivered.
   /// <summary>
    int OrderId,
    /// <summary>
    /// courier identifier responsible for the delivery.
    /// <summary>
    int CourierId,
    /// <summary>
    /// delivery type (motorcycle, car, bike, foot).
    /// <summary>
    DeliveryTypeMethods DeliveryType,
    /// <summary>
    /// date and time when the delivery started.
    /// <summary>
    DateTime OrderStartDateTime,
    /// <summary>
    /// delivery distance (in kilometers) for this delivery.(“Calculation in the logical layer”)
    double? DeliveryDistanceKm=null,
    /// <summary>
    /// delivery type ending (if applicable) supplied etc.
    /// <summary>
    DeliveryCompletionType? DeliveryTypeEnding=null,
    /// <summary>
    /// order end date and time (if applicable).
    /// <summary>
    DateTime? OrderEndDateTime=null

)
/// <summary>
/// Default constructor for stage 3
/// </summary>

{
    public Delivery() : this(
             0,              
             0,              
             0,              
             default!,
             DateTime.Now,           
             null,           
             null            
         )
    { }     
}
