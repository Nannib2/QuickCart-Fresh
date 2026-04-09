namespace Dal;

/// <summary>
/// Represents the in-memory data source for the application.
/// </summary>
/// <remarks>
/// This static class simulates a persistent storage layer by holding collections
/// of data entities in memory. It contains separate lists for <see cref="DO.Courier"/>,
/// <see cref="DO.Order"/>, and <see cref="DO.Delivery"/> objects.
/// <para>
/// The data here is shared across all DAL implementations and remains
/// available throughout the application's lifetime.
/// </para>
/// </remarks>
internal static class DataSource
{
    /// <summary>
    /// Gets the list of all couriers in memory.
    /// </summary>
    internal static List<DO.Courier> Couriers { get; } = new();

    /// <summary>
    /// Gets the list of all orders  in memory.
    /// </summary>
    internal static List<DO.Order> Orders { get; } = new();

    /// <summary>
    /// Gets the list of all deliveries in memory.
    /// </summary>
    internal static List<DO.Delivery> Deliverys { get; } = new();
}
