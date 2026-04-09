
namespace DO;
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



