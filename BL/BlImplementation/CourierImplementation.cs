

namespace BlImplementation;
using BLApi;
using BO;
using Helpers;

internal class CourierImplementation : ICourier
{
    public void Create(BO.Courier boCourier, int requesterId)
    {
        AdminManager.ThrowOnSimulatorIsRunning();
        CourierManager.GetCreateCourier( boCourier, requesterId);
    }

    public void Delete(int courierId, int requesterId)
    {
        AdminManager.ThrowOnSimulatorIsRunning();
        CourierManager.DeleteCourier(courierId, requesterId);
    }

    public string EnterSystem(int id, string password)
    {
       return CourierManager.EnterSystem(id, password);
    }

    public Task< Courier> Read(int requesterId, int courierId)
    {
         return CourierManager.ReadCourier(courierId, requesterId);
    }

    public IEnumerable<CourierInList> ReadAll(int requesterId, bool? active, CourierInListProperties? sortBy, CourierInListProperties? filterBy, Object? innerFilterBy)
    {
        return CourierManager.ReadAllCouriers(requesterId, active, sortBy, filterBy , innerFilterBy );
    }

    public void Update(BO.Courier boCourier, int requesterId)
    {
        AdminManager.ThrowOnSimulatorIsRunning();
        CourierManager.UpdateCourier(boCourier, requesterId);
    }
    public void AddObserver(Action listObserver) =>
        CourierManager.Observers.AddListObserver(listObserver); //stage 5
    public void AddObserver(int id, Action observer) =>
        CourierManager.Observers.AddObserver(id, observer); //stage 5
    public void RemoveObserver(Action listObserver) =>
        CourierManager.Observers.RemoveListObserver(listObserver); //stage 5
    public void RemoveObserver(int id, Action observer) =>
        CourierManager.Observers.RemoveObserver(id, observer); //stage 5


}
