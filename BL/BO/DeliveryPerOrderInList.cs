
using Helpers;

namespace BO;

/// <summary>
/// Represents delivery details for display in a list of orders (view model).
/// This class is designed for viewing purposes only.
/// </summary>
public class DeliveryPerOrderInList
{
    /// <summary>
    /// The running identification number of the delivery entity (DeliveryId).
    /// This field is read-only and cannot be updated.
    /// </summary>
    public int DeliveryId { get; init; }

    /// <summary>
    /// The identification number of the courier (CourierId).
    /// This field is mandatory (not nullable).
    /// </summary>
    public int? CourierId { get; init; }

    /// <summary>
    /// The name of the courier.
    /// This field is mandatory (not nullable).
    /// </summary>
    public string? CourierName { get; set; }    

    /// <summary>
    /// The type of shipping (ENUM).
    /// </summary>
    public DeliveryTypeMethods DeliveryType { get; init; }

    /// <summary>
    /// The start time of the delivery (DateTime).
    /// This field is mandatory (not nullable).
    /// </summary>
    public DateTime StartTime { get; init; }

    /// <summary>
    /// The type of delivery completion (ENUM).
    /// This field is optional (nullable) as the delivery may not be finished yet.
    /// </summary>
    public DeliveryCompletionType? FinishType { get; init; }

    /// <summary>
    /// The time the delivery was completed (DateTime).
    /// This field is optional (nullable) as the delivery may not be finished yet.
    /// </summary>
    public DateTime? FinishTime { get; init; }
    public override string ToString() => Tools.ToStringProperty<DeliveryPerOrderInList>(this);
}