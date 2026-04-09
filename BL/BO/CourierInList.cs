
using Helpers;

namespace BO;

public class CourierInList
{
    /// <summary>
    /// Unique identifier of the courier (from DO.Courier).
    /// Read-only logical entity (no validation).
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Full name of the courier (first + last name).
    /// Loaded from DO.Courier.
    /// </summary>
    public string NameCourier { get; init; }

    /// <summary>
    /// Indicates whether the courier is active.
    /// Loaded from DO.Courier.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Courier's delivery type.
    /// Loaded from DO.Courier.
    /// </summary>
    public DeliveryTypeMethods DeliveryType { get; init; }

    /// <summary>
    /// Time the courier began working in the company.
    /// Loaded from DO.Courier.
    /// </summary>
    public DateTime WorkStartTime { get; init; }

    /// <summary>
    /// Number of deliveries that met the expected delivery time.
    /// Calculated in the logical layer.
    /// </summary>
    public int OnTimeDeliveries { get; init; }

    /// <summary>
    /// Number of deliveries that were late.
    /// Calculated in the logical layer.
    /// </summary>
    public int LateDeliveries { get; init; }

    /// <summary>
    /// ID of the active delivery currently assigned to the courier.
    /// Nullable – only exists when a delivery is in progress.
    /// </summary>
    public int? ActiveDeliveryId { get; init; }
    public override string ToString() => Tools.ToStringProperty<CourierInList>(this);
}