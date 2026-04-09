using DalApi;
using System.Diagnostics;

namespace Dal;

/// <summary>
/// Implements the <see cref="IDal"/> interface, serving as the main entry point to the Data Access Layer (DAL)
/// when using the XML file persistence mechanism.
/// <para>This class aggregates all specific data access implementations (Couriers, Orders, Deliveries, and Config)
/// and provides a central method for resetting the entire database state.</para>
/// </summary>
sealed internal class DalXml : IDal

{
     /// <summary>
    /// Thread-safe lazy initialization of the singleton instance.
    /// Uses System.Lazy to ensure the instance is created only when first accessed 
    /// and is safe to use across multiple threads without manual locking.
    /// </summary>
    private static readonly Lazy<DalXml> lazyInstance = new Lazy<DalXml>(() => new DalXml());
    public static DalXml Instance => lazyInstance.Value;
    private DalXml() { }
    /// <summary>
    /// Gets the data access logic for <see cref="ICourier"/> entities, implemented using XML files.
    /// </summary>
    public ICourier Courier { get; } = new CourierImplementation();

    /// <summary>
    /// Gets the data access logic for <see cref="IOrder"/> entities, implemented using XML files.
    /// </summary>
    public IOrder Order { get; } = new OrderImplementation();

    /// <summary>
    /// Gets the data access logic for <see cref="IDelivery"/> entities, implemented using XML files.
    /// </summary>
    public IDelivery Delivery { get; } = new DeliveryImplementation();

    /// <summary>
    /// Gets the data access logic for configuration settings, implemented using XML files.
    /// </summary>
    public IConfig Config { get; } = new ConfigImplementation();

    /// <summary>
    /// Resets the entire system database to its initial default state.
    /// <para>This operation clears all stored data (Couriers, Orders, Deliveries) and resets all configuration settings (including auto-increment IDs and company constants).</para>
    /// </summary>
    public void ResetDB()
    {
        // Deletes all entities from their respective XML files.
        Courier.DeleteAll();
        Order.DeleteAll();
        Delivery.DeleteAll();
        // Resets configuration values and auto-incrementing IDs in the data-config XML file.
        Config.Reset();
    }
}