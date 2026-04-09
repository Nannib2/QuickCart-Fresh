using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dal;
using DalApi;
sealed internal class DalList : IDal

{
    ///<summary>
    /// Thread-safe lazy initialization of the singleton instance.
    /// Uses System.Lazy to ensure the instance is created only when first accessed 
    /// and is safe to use across multiple threads without manual locking.
    /// </summary>
    private static readonly Lazy<DalList> lazyInstance = new Lazy<DalList>(() => new DalList());
    public static DalList Instance => lazyInstance.Value;// Public accessor for the singleton instance
    private DalList() { }// Private constructor to prevent external instantiation
    public ICourier Courier { get; } = new CourierImplementation();
    public IOrder Order { get; } = new OrderImplementation();
    public IDelivery Delivery { get; } = new DeliveryImplementation();
    public IConfig Config { get; } = new ConfigImplementation();

    public void ResetDB()
    {
        Courier.DeleteAll();
        Order.DeleteAll();
        Delivery.DeleteAll();
        Config.Reset();
    }
}
