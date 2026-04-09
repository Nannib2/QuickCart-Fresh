
using DO;
using Helpers;

namespace BO;

/// <summary>
/// Represents a concise view of an Order, designed for display in lists.
/// This class is used for read-only purposes and supports selection by the courier.
/// </summary>
public class OpenOrderInList
{
    /// <summary>
    /// The ID of the courier responsible for selecting this order.
    /// This field is mandatory (cannot be null) and is taken from the DO.Order entity.
    /// </summary>
    public int? CourierId { get; init; }

    /// <summary>
    /// The running identification number of the order.
    /// </summary>
    public int OrderId { get; init; }

    /// <summary>
    /// The status of the order (e.g., Created, Scheduled, Delivered).
    /// </summary>
    public OrderRequirements OrderType { get; init; }

    // Item Attributes  - Example of required attributes
    /// <summary>
    /// Attributes of the item, FreeShippingEligibility, AmountItems ,Updatable by manager.
    /// </summary>
    public bool FreeShippingEligibility { get; init; }
    public int AmountItems { get; init; }

    // Full Address 
    /// <summary>
    /// The full, valid, and real address of the order location. Updatable by manager.
    /// The logical layer calculates Latitude/Longitude and validates the address existence.
    /// </summary>
    public string FullAddress { get; init; }

    /// <summary>
    /// The air distance between the delivery location and the courier's current location.
    /// This is a calculated field.
    /// This field is nullable (can be null) if the courier's location is unknown.
    /// </summary>
    public double AirDistance { get; init; }

    /// <summary>
    /// The actual distance between the delivery location and the courier's current location.
    /// This is a calculated field.
    /// This field is nullable (can be null) if the courier's location is unknown.
    /// </summary>
    public double? ActualDistance { get; init; }

    /// <summary>
///assuming delivery time
    /// </summary>
    public TimeSpan? ActualDeliveryTimeSpan { get; init; }

    /// <summary>
    /// The current schedule status of the delivery related to the order.
    /// </summary>
    public ScheduleStatus ScheduleStatus { get; init; }

    /// <summary>
    /// The required time left to finish the order, based on the earliest scheduled pick-up time.
    /// This is a calculated field.
    /// This field is mandatory (not nullable).
    /// </summary>
    public TimeSpan TimeToFinish { get; init; }

    /// <summary>
    /// The estimated time of arrival (ETA) for collection or delivery, based on the courier's location.
    /// This is a calculated field.
    /// This field is mandatory (not nullable).
    /// </summary>
    public DateTime MaximumDeliveryTime { get; init; }
    public override string ToString() => Tools.ToStringProperty<OpenOrderInList>(this, "CourierId");
}
