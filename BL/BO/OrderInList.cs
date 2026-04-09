
using Helpers;

namespace BO;

public class OrderInList
{
    // 1. Delivery ID 
    /// <summary>
    /// The ID of the latest or current Delivery entity for this order. Read-only.
    /// Nullable, as no delivery might have been initiated yet (DO.Delivery).
    /// Not intended for display.
    /// </summary>
    public int? DeliveryId { get; init; }

    // 2. Order ID
    /// <summary>
    /// The unique identifier for the Order entity. Read-only.
    /// Retrieved from DO.Order.
    /// </summary>
    public int OrderId { get; init; }

    // 3. Order Type 
    /// <summary>
    /// The type of the order. Read-only.
    /// Retrieved from DO.Order.
    /// </summary>
    public OrderRequirements OrderType { get; init; }

    // 4. Air Distance 
    /// <summary>
    /// The straight-line (air) distance from the company's location to the order's destination. Read-only.
    /// Calculated in the logical layer.
    /// </summary>
    public double AirDistance { get; init; }

    // 5. Order Status 
    /// <summary>
    /// The overall status of the order (based on the latest delivery status). Read-only.
    /// Calculated in the logical layer.
    /// </summary>
    public OrderStatus Status { get; init; }

    // 6. Schedule Status 
    /// <summary>
    /// The status regarding adherence to deadlines (e.g., OnTime, Delayed). Read-only.
    /// Calculated in the logical layer.
    /// </summary>
    public ScheduleStatus ScheduleStatus { get; init; }

    // 7. Time Remaining 
    /// <summary>
    /// The total time remaining until the maximum delivery time. Read-only.
    /// If the order is closed, this is TimeSpan.Zero (00:00:00).
    /// </summary>
    public TimeSpan TimeRemaining { get; init; }

    // 8. Total Completion Time 
    /// <summary>
    /// The total duration from order opening to final delivery completion. Read-only.
    /// Calculated as (Latest Delivery End Time - Order Opening Time).
    /// If the order is open, this is TimeSpan.Zero (00:00:00).
    /// </summary>
    public TimeSpan TotalCompletionTime { get; init; }

    // 9. Total Deliveries 
    /// <summary>
    /// The total number of deliveries attempted/completed for this order (including current and previous attempts). Read-only.
    /// 0 if no delivery has been initiated yet.
    /// </summary>
    public int TotalDeliveries { get; init; }
    public override string ToString() => Tools.ToStringProperty<OrderInList>(this, "DeliveryId");
}