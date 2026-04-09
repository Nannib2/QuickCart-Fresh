using BO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PL.Enums;

/// <summary>
/// supports binding DeliveryTypeMethods enum to WPF controls
/// </summary>
public class DeliveryTypeMethodsCollection : IEnumerable
{
    static readonly IEnumerable<BO.DeliveryTypeMethods> s_enums =
(Enum.GetValues(typeof(BO.DeliveryTypeMethods)) as IEnumerable<BO.DeliveryTypeMethods>)!;

    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}

/// <summary>
/// delivery completion type enum for binding to WPF controls
/// </summary>
public class DeliveryCompletionTypeCollection : IEnumerable
{
    // convert enum to IEnumerable
    static readonly IEnumerable<BO.DeliveryCompletionType> s_enums =
        (Enum.GetValues(typeof(BO.DeliveryCompletionType)) as IEnumerable<BO.DeliveryCompletionType>)!;

    // to support foreach
    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}


/// <summary>
/// Provides a collection of OrderRequirements enum values for XAML data binding.
/// This class enables the UI to populate filtering ComboBoxes directly from the enum definitions.
/// </summary>
public class OrderRequirementsCollection : IEnumerable
{
    /// <summary>
    /// Static collection of enum values to avoid redundant allocations.
    /// </summary>
    static readonly IEnumerable<BO.OrderRequirements> s_enums =
        (Enum.GetValues(typeof(BO.OrderRequirements)) as IEnumerable<BO.OrderRequirements>)!;

    /// <summary>
    /// Returns an enumerator that iterates through the OrderRequirements collection.
    /// Used by WPF ItemsControl (like ComboBox) to display the list of requirements.
    /// </summary>
    /// <returns>An enumerator for the enum values.</returns>
    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}

/// <summary>
/// Provides a collection of ClosedDeliveryInListProperties enum values for XAML data binding.
/// Used to populate sorting ComboBoxes in the Delivery History screen.
/// </summary>
public class ClosedDeliveryInListPropertiesCollection : IEnumerable
{
    /// <summary>
    /// Static collection of property names used for sorting operations.
    /// </summary>
    static readonly IEnumerable<BO.ClosedDeliveryInListProperties> s_enums =
        (Enum.GetValues(typeof(BO.ClosedDeliveryInListProperties)) as IEnumerable <BO.ClosedDeliveryInListProperties>)!;

    /// <summary>
    /// Returns an enumerator that iterates through the sorting properties collection.
    /// </summary>
    /// <returns>An enumerator for the sortable properties.</returns>
    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}

/// <summary>
/// Represents a collection of properties used for sorting open orders in a list.
/// </summary>
/// <remarks>This collection provides an enumeration of all values in the <see
/// cref="BO.OpenOrderInListProperties"/> enum. It is designed to support WPF data binding scenarios by enabling
/// iteration over the collection.</remarks>
public class OpenOrderInListPropertiesCollection : IEnumerable
{
    // defines a static collection of enum values for sorting open orders
    static readonly IEnumerable<BO.OpenOrderInListProperties> s_enums =
        (Enum.GetValues(typeof(BO.OpenOrderInListProperties)) as IEnumerable<BO.OpenOrderInListProperties>)!.Where(prop => prop != OpenOrderInListProperties.CourierId);

    // for foreach support in WPF binding
    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}

/// <summary>
/// Supports binding OrderInListProperties enum to WPF controls
/// (e.g., ComboBox for filtering or sorting orders).
/// </summary>
public class OrderInListPropertiesCollection : IEnumerable
{
    /// <summary>
    /// Cached collection of all OrderInListProperties enum values.
    /// </summary>
    static readonly IEnumerable<OrderInListProperties> s_enums =
        (Enum.GetValues(typeof(OrderInListProperties)) as IEnumerable<OrderInListProperties>)!;

    /// <summary>
    /// Returns an enumerator that iterates through the enum values.
    /// </summary>
    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}

/// <summary>
/// Supports binding OrderStatus enum to WPF controls
/// (e.g., status filter, visual triggers, or DataGrid columns).
/// </summary>
public class OrderStatusCollection : IEnumerable
{
    /// <summary>
    /// Cached collection of all OrderStatus enum values.
    /// </summary>
    static readonly IEnumerable<OrderStatus> s_enums =
        (Enum.GetValues(typeof(OrderStatus)) as IEnumerable<OrderStatus>)!;

    /// <summary>
    /// Returns an enumerator that iterates through the enum values.
    /// </summary>
    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}

/// <summary>
/// supports binding ScheduleStatus enum to WPF controls
/// </summary>
public class ScheduleStatusCollection : IEnumerable
{
    /// <summary>
    /// Cached collection of all OrderStatus enum values.
    /// </summary>
    static readonly IEnumerable<ScheduleStatus> s_enums =
        (Enum.GetValues(typeof(ScheduleStatus)) as IEnumerable<ScheduleStatus>)!;

    /// <summary>
    /// Returns an enumerator that iterates through the enum values.
    /// </summary>
    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}

/// <summary>
/// collection for binding CourierInListProperties enum to WPF controls
/// </summary>
public class CourierInListPropertiesCollection : IEnumerable
{
    /// <summary>
    /// Cached collection of all OrderStatus enum values.
    /// </summary>
    static readonly IEnumerable<CourierInListProperties> s_enums =
        (Enum.GetValues(typeof(CourierInListProperties)) as IEnumerable<CourierInListProperties>)!.Where(prop => prop != CourierInListProperties.Id && prop != CourierInListProperties.NameCourier);

    /// <summary>
    /// Returns an enumerator that iterates through the enum values.
    /// </summary>
    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}
