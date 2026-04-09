
using Helpers;

namespace BO;

public class OrderInProgress
{
    /// <summary>
    /// Unique identifier of the delivery entity (from DO.Delivery).
    /// Read-only in the logical layer.
    /// </summary>
    public int DeliveryId { get; init; }

    /// <summary>
    /// Unique identifier of the order entity (from DO.Order).
    /// Read-only in the logical layer.
    /// </summary>
    public int OrderId { get; init; }

    /// <summary>
    /// Type of the order. Loaded from DO.Order.
    /// </summary>
    public OrderRequirements OrderType { get; init; }

    /// <summary>
    /// Optional textual description of the order (nullable).
    /// Loaded from DO.Order.
    /// </summary>
    public string? ShortOrderDescription { get; init; }
  
    /// <summary>
    /// Full address of the order destination.
    /// Loaded from DO.Order.
    /// </summary>
    public string OrderAddress { get; init; }

    /// <summary>
    /// Air distance between the company and the order address.
    /// Calculated in the logical layer.
    /// </summary>
    public double AirDistance { get; init; }

    /// <summary>
    /// Actual travel distance (nullable).
    /// Taken from DO.Delivery.
    /// </summary>
    public double? ActualDistance { get; init; }

    /// <summary>
    /// Full name of the customer who placed the order.
    /// Loaded from DO.Order.
    /// </summary>
    public string CustomerName { get; init; }

    /// <summary>
    /// Phone number of the customer.
    /// Loaded from DO.Order.
    /// </summary>
    public string CustomerPhone { get; init; }

    /// <summary>
    /// The date and time when the order was created.
    /// Loaded from DO.Order.
    /// </summary>
    public DateTime OrderCreatedTime { get; init; }

    /// <summary>
    /// The date and time when the delivery started.
    /// Loaded from DO.Delivery.
    /// </summary>
    public DateTime DeliveryStartTime { get; init; }

    /// <summary>
    /// Expected delivery time, calculated in the logical layer.
    /// Based on delivery start time, order distance, and delivery type.
    /// </summary>
    public DateTime ExpectedDeliveryTime { get; init; }

    /// <summary>
    /// Maximum allowed delivery time.
    /// Calculated in the logical layer from the configuration.
    /// </summary>
    public DateTime MaxDeliveryTime { get; init; }

    /// <summary>
    /// Status of the order based on the latest delivery data.
    /// Calculated in the logical layer.
    /// </summary>
    public OrderStatus OrderStatus { get; init; }

    /// <summary>
    /// Delivery schedule status indicating whether the order is on-time or late.
    /// Calculated in the logical layer.
    /// </summary>
    public ScheduleStatus ScheduleStatus { get; init; }

    /// <summary>
    /// Remaining time until the order’s maximum delivery time is reached.
    /// Calculated as MaxDeliveryTime minus the current system time.
    /// </summary>
    public TimeSpan RemainingTime { get; init; }
    public override string ToString() => Tools.ToStringProperty<OrderInProgress>(this, "DeliveryId");
}