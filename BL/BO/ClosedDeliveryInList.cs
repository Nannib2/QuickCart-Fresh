
using Helpers;

namespace BO;

public class ClosedDeliveryInList
{
    // 1. Delivery ID 
    /// <summary>
    /// The unique identifier for the Delivery entity. Read-only.
    /// Retrieved from DO.Delivery. Not intended for display.
    /// </summary>
    public int DeliveryId { get; init; }

    // 2. Order ID 
    /// <summary>
    /// The unique identifier for the related Order entity. Read-only.
    /// Retrieved from DO.Delivery.
    /// </summary>
    public int OrderId { get; init; }

    // 3. Order Type 
    /// <summary>
    /// The type of the order that was delivered. Read-only.
    /// Retrieved from DO.Order.
    /// </summary>
    public OrderRequirements OrderType { get; init; }

    // 4. Full Address 
    /// <summary>
    /// The full address of the order's destination. Read-only.
    /// Retrieved from DO.Order.
    /// </summary>
    public string FullAddress { get; init; }

    // 5. Shipping Type
    /// <summary>
    /// The type of shipping used for this delivery. Read-only.
    /// Retrieved from DO.Delivery.
    /// </summary>
    public DeliveryTypeMethods DeliveryType { get; init; }

    // 6. Actual Distance 
    /// <summary>
    /// The actual distance traveled for this delivery. Nullable (if distance wasn't logged). Read-only.
    /// Retrieved from DO.Delivery.
    /// </summary>
    public double? ActualDistance { get; init; }

    // 7. Total Completion Time 
    /// <summary>
    /// The total duration of the delivery process (End Time - Start Time). Read-only.
    /// Calculated as the time difference between delivery start and end times.
    /// </summary>
    public TimeSpan TotalCompletionTime { get; init; }

    // 8. Delivery Completion Type 
    /// <summary>
    /// The final outcome of the delivery (e.g., Delivered, Returned, Canceled). Nullable. Read-only.
    /// Retrieved from DO.Delivery.
    /// </summary>
    public DeliveryCompletionType? DeliveryCompletionType { get; init; }
    public override string ToString() => Tools.ToStringProperty<ClosedDeliveryInList>(this, "DeliveryId");
}