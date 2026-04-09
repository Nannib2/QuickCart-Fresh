
namespace BO;

/// <summary>
/// Represents the available types of delivery methods.
/// </summary>
public enum DeliveryTypeMethods
{
    Motorcycle,
    Car,
    Bike,
    Foot
}

public enum OrderRequirements
{
    /// <summary>
    /// Frozen goods order — requires full cold chain handling
    /// and insulated or refrigerated transport.
    /// </summary>
    Frozen,

    /// <summary>
    /// Chilled goods order — includes refrigerated items such as
    /// dairy, meat, or fish. Requires cooling but not freezing.
    /// </summary>
    Chilled,

    /// <summary>
    /// Regular dry goods order — includes non-perishable items
    /// like bread, canned food, or cleaning supplies.
    /// </summary>
    Dry,

    /// <summary>
    /// Fragile goods order — includes sensitive or breakable items
    /// (e.g., eggs, glass bottles) that require careful handling.
    /// </summary>
    Fragile,


    /// <summary>
    /// Mixed order — includes multiple categories (frozen, chilled,
    /// and dry items), requiring optimized sorting and transport.
    /// </summary>
    Mixed
}


/// <summary>
/// Represents the status of an order based on the last delivery status and business logic.
/// Calculated based on the data in DO.Order and DO.Delivery entities.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// The order is open - it is not currently being handled by any courier 
    /// and has not been canceled for any reason.
    /// (Equivalent to the state before the first attempt to schedule).
    /// </summary>
    Open,

    /// <summary>
    /// The order is currently being handled by a courier.
    /// (Equivalent to the state when the order is scheduled to be picked up or is in transit).
    /// </summary>
    InProgress,

    /// <summary>
    /// The order is closed and successfully delivered.
    /// This state is reached when the last delivery ends with a "Finished" status.
    /// </summary>
    Delivered,

    /// <summary>
    /// The order is closed because the delivery was rejected by the customer upon arrival.
    /// This state is reached when the last delivery ends with a "Rejected" status.
    /// </summary>
    Rejected,

    /// <summary>
    /// The order is canceled and closed.
    /// This state is reached when the last delivery ends with a "Canceled" status.
    /// </summary>
    Canceled
}

/// <summary>
/// Represents the timeliness status of an order's delivery based on time constraints.
/// Calculated based on the data in DO.Order and DO.Delivery entities, and the time allocated by the manager.
/// </summary>
public enum ScheduleStatus
{
    /// <summary>
    /// The order is on time.
    /// Conditions:
    /// - Order is open/in progress AND there is sufficient time left to perform and complete the delivery within the committed time frame (without risk time).
    /// - Order is closed AND finished, AND the delivery completion time was before the estimated arrival time (ETA).
    /// </summary>
    OnTime,

    /// <summary>
    /// The order is potentially at risk of being late.
    /// Condition:
    /// - Order is open/in progress AND there is less than the risk time left until the estimated arrival time (ETA).
    /// </summary>
    InRisk,

    /// <summary>
    /// The order is late.
    /// Conditions:
    /// - Order is open/in progress AND the estimated arrival time (ETA) has passed, but the order is not yet closed.
    /// - Order is closed AND finished, AND the delivery completion time was after the estimated arrival time (ETA).
    /// </summary>
    Late
}

/// <summary>
/// Represents the possible types of delivery completion statuses.
/// </summary>
public enum DeliveryCompletionType
{
    /// <summary>
    /// The order was successfully delivered and closed.
    /// </summary>
    Supplied,

    /// <summary>
    /// The customer refused to accept the order; it was returned and closed.
    /// </summary>
    CustomerRefused,

    /// <summary>
    /// The order was canceled by the manager or the customer after creation.
    /// </summary>
    Canceled,

    /// <summary>
    /// The courier arrived, but the customer was not found at the destination.
    /// </summary>
    CustomerNotFound,

    /// <summary>
    /// The delivery failed due to a technical issue (e.g., route calculation error).
    /// </summary>
    Failed
}
/// <summary>
/// Represents the different types of grocery delivery orders
/// based on the nature of their contents and delivery requirements.
/// </summary>


  public enum CourierInListProperties
    {
        Id,
        NameCourier,
        IsActive,
        DeliveryType,
        WorkStartTime,
        OnTimeDeliveries,
        LateDeliveries,
        ActiveDeliveryId
    }

    /// <summary>
    /// Represents the available properties for sorting or filtering a list of orders.
    /// </summary>
    public enum OrderInListProperties
    {
       // DeliveryId,
        OrderId,
        OrderType,
        AirDistance,
        Status,
        ScheduleStatus,
        TimeRemaining,
        TotalCompletionTime,
        TotalDeliveries
    }

    /// <summary>
    /// Enumeration of properties available for sorting or filtering a list of closed deliveries (ClosedDeliveryInList).
    /// </summary>
    public enum ClosedDeliveryInListProperties
    {
        //// 1. Delivery ID (used internally, not typically for user sorting)
        //DeliveryId,

        // 2. Order ID
        OrderId,

        // 3. Order Type (e.g., dry)
        OrderType,

        // 4. Full Address
        FullAddress,

        // 5. Shipping Type (e.g., Bike, Drone, Van)
        DeliveryType,

        // 6. Actual Distance
        ActualDistance,

        // 7. Total Completion Time
        TotalCompletionTime,

        // 8. Delivery Completion Type (The final outcome: Delivered, Returned, Canceled)
        DeliveryCompletionType
    }

    /// <summary>
    /// Enumeration of properties available for sorting or filtering a list of open orders (OpenOrderInList).
    /// </summary>
    public enum OpenOrderInListProperties
    {
        /// <summary>
        /// The ID of the courier responsible for selecting this order.
        /// </summary>
        CourierId,

        /// <summary>
        /// The running identification number of the order.
        /// </summary>
        OrderId,

        /// <summary>
        /// The type of the order (e.g., Standard, Express).
        /// </summary>
        OrderType,

        /// <summary>
        /// Eligibility for free shipping.
        /// </summary>
        FreeShippingEligibility,

        /// <summary>
        /// The number of items in the order.
        /// </summary>
        AmountItems,

        /// <summary>
        /// The full address of the order location.
        /// </summary>
        FullAddress,

        /// <summary>
        /// The straight-line distance (air distance) between the delivery location and the courier's current location.
        /// </summary>
        AirDistance,

        /// <summary>
        /// The estimated actual distance between the delivery location and the courier's current location (e.g., road distance).
        /// </summary>
        ActualDistance,

        /// <summary>
        /// The actual time it took for the delivery, calculated from start to completion.
        /// </summary>
        ActualDeliveryTimeSpan,

        /// <summary>
        /// The current schedule status of the delivery related to the order (e.g., OnTime, Delayed).
        /// </summary>
        ScheduleStatus,

        /// <summary>
        /// The required time left to finish the order.
        /// </summary>
        TimeToFinish,

        /// <summary>
        /// The maximum allowed time for the delivery.
        /// </summary>
        MaximumDeliveryTime
    }


    /// <summary>
    /// Represents the units of time used for advancing the system clock.
    /// </summary>
    public enum TimeUnit
    {
        /// <summary>
        /// A single minute. Used to advance the clock by 60 seconds.
        /// </summary>
        Minute,

        /// <summary>
        /// A single hour. Used to advance the clock by 60 minutes.
        /// </summary>
        Hour,

        /// <summary>
        /// A single day. Used to advance the clock by 24 hours.
        /// </summary>
        Day,

        /// <summary>
        /// A single month. Used to advance the clock by the corresponding number of days in the current month.
        /// </summary>
        Month,

        /// <summary>
        /// A single year. Used to advance the clock by 365 or 366 days.
        /// </summary>
        Year
    }
public enum Months
{
    January = 1,
    February,
    March,
    April,
    May,
    June,
    July,
    August,
    September,
    October,
    November,
    December
}
