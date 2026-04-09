

namespace BlImplementation;
using BLApi;
using BO;
using DO;
using Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

internal class  OrderImplementation : IOrder
{
    public Task CancelOrder(int orderId, int requesterId)
    {
        AdminManager.ThrowOnSimulatorIsRunning();
        return OrderManager.CancelOrder(orderId, requesterId);
    }

    public Task Create(BO.Order order, int requesterId)
    {
        AdminManager.ThrowOnSimulatorIsRunning();
        return OrderManager.CreateOrder(order, requesterId);
    }

    public void Delete(int orderId, int requesterId)
    {
        OrderManager.DeleteOrder(orderId, requesterId);
    }

    public void FinishDeliveryHandling(int requesterId, int courierId, int deliveryId, BO.DeliveryCompletionType? completionType)
    {
        AdminManager.ThrowOnSimulatorIsRunning();
        OrderManager.FinishDeliveryHandling(requesterId, courierId, deliveryId,(DO.DeliveryCompletionType)completionType!);
    }

    public Task<IEnumerable<BO.OpenOrderInList>> GetAvailableOrdersForCourier(int requesterId, int courierId, BO.OrderRequirements? filterByOrderType, OpenOrderInListProperties? sortByProperty)
    {
        return  OrderManager.GetAvailableOrdersList(requesterId, courierId, filterByOrderType, sortByProperty);
    }

    public IEnumerable<BO.ClosedDeliveryInList> GetClosedDeliveriesByCourier(int requesterId, int courierId, BO.OrderRequirements? filterByOrderType, ClosedDeliveryInListProperties? sortByProperty)
    {
        return OrderManager.GetClosedDeliveriesList(requesterId, courierId, filterByOrderType, sortByProperty);
    }

    public int[] GetOrderSummary(int requesterId)
    {
        return OrderManager.GetOrderQuantitiesByStatus(requesterId);
    }

    public Task<BO.Order> Read(int requesterId, int orderId)
    {
        return  OrderManager.ReadOrder(orderId, requesterId);
    }
    public IEnumerable<BO.OrderInList> ReadAll(int requesterId, BO.OrderStatus? statusFilter = null, BO.OrderRequirements? typeFilter = null, BO.ScheduleStatus? scheduleFilter = null, BO.OrderInListProperties? sortBy = null)
    {        
        return OrderManager.ReadAllOrdersInList(requesterId, statusFilter, typeFilter, scheduleFilter, sortBy);
    }

    public Task SelectOrderForHandling(int requesterId, int courierId, int orderId)
    {
        AdminManager.ThrowOnSimulatorIsRunning();
        return OrderManager.SelectOrderForHandling(requesterId, courierId, orderId);
    }

    public Task Update(BO.Order order, int requesterId)
    {
        AdminManager.ThrowOnSimulatorIsRunning();
        return  OrderManager.UpdateOrder(order, requesterId);
    }
    public void AddObserver(Action listObserver) =>
        OrderManager.Observers.AddListObserver(listObserver); //stage 5
    public void AddObserver(int id, Action observer) =>
        OrderManager.Observers.AddObserver(id, observer); //stage 5
    public void RemoveObserver(Action listObserver) =>
        OrderManager.Observers.RemoveListObserver(listObserver); //stage 5
    public void RemoveObserver(int id, Action observer) =>
        OrderManager.Observers.RemoveObserver(id, observer); //stage 5

}
