using BlApi;
using BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLApi;

public interface IOrder : IObservable //stage 5
{
    public int[] GetOrderSummary(int requesterId);
    //public IEnumerable<BO.OrderInList> ReadAll(int requesterId, OrderInListProperties? filterBy, Object? obj, OrderInListProperties? sortBy);
    public IEnumerable<BO.OrderInList> ReadAll( int requesterId, BO.OrderStatus? statusFilter = null,BO.OrderRequirements? typeFilter = null, BO.ScheduleStatus? scheduleFilter = null, BO.OrderInListProperties? sortBy = null);
    public Task< BO.Order> Read(int requesterId, int orderId);
    public Task Update(BO.Order order, int requesterId);
    public void Delete(int orderId, int requesterId);
    public Task Create(BO.Order order, int requesterId);
    public Task CancelOrder(int orderId, int requesterId);

    /// <summary>
    /// Closes a delivery by updating its status to 'Delivered' and setting the completion time.
    /// </summary>
    /// <param name="requesterId">The ID of the requester (manager or courier).</param>
    /// <param name="courierId">The ID of the courier reporting the delivery completion.</param>
    /// <param name="deliveryId">The unique ID of the delivery being closed.</param>
    /// <exception cref="InvalidOperationException">Thrown if the requester is not the assigned courier or the request is invalid.</exception>
    public void FinishDeliveryHandling(int requesterId, int courierId, int deliveryId, BO.DeliveryCompletionType? completionType);

    /// <summary>
    /// Initiates a new delivery process for a selected order by creating a new DO.Delivery entity.
    /// </summary>
    /// <param name="requesterId">The ID of the requester (manager or courier).</param>
    /// <param name="courierId">The ID of the courier who is taking the order for delivery.</param>
    /// <param name="orderId">The ID of the order selected for handling.</param>
    /// <exception cref="InvalidOperationException">Thrown if the order is not available or the request is invalid.</exception>
    public Task SelectOrderForHandling(int requesterId, int courierId, int orderId);

    /// <summary>
    /// Retrieves a list of closed orders (deliveries) handled by a specific courier, with optional filtering and sorting.
    /// </summary>
    /// <param name="requesterId">The ID of the requester.</param>
    /// <param name="courierId">The ID of the courier whose delivery history is requested.</param>
    /// <param name="filterByOrderType">Optional: Filters the list by a specific order type.</param>
    /// <param name="sortByProperty">Optional: Sorts the list by the specified property. Defaults to Schedule Status if null.</param>
    /// <returns>A sorted and filtered list of closed delivery summaries.</returns>
    public IEnumerable<BO.ClosedDeliveryInList> GetClosedDeliveriesByCourier(int requesterId, int courierId, BO.OrderRequirements? filterByOrderType, ClosedDeliveryInListProperties? sortByProperty);

    /// <summary>
    /// Retrieves a list of open orders available for selection by a specific courier, filtered by their personal max-distance, with optional sorting and filtering.
    /// </summary>
    /// <param name="requesterId">The ID of the requester.</param>
    /// <param name="courierId">The ID of the courier requesting the list (used to calculate distance and max-distance filter).</param>
    /// <param name="filterByOrderType">Optional: Filters the list by a specific order type.</param>
    /// <param name="sortByProperty">Optional: Sorts the list by the specified property. Defaults to Schedule Status if null.</param>
    /// <returns>A sorted and filtered list of open order summaries within the courier's range.</returns>
    public Task< IEnumerable<BO.OpenOrderInList>> GetAvailableOrdersForCourier(int requesterId, int courierId, BO.OrderRequirements? filterByOrderType, OpenOrderInListProperties? sortByProperty);
}
