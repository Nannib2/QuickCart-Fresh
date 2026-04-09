using BO;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers;

internal static class DeliveryManager
{
    
    private static readonly IDal s_dal = Factory.Get;
    internal static ObserverManager Observers = new(); //stage 5 
    private static readonly AsyncMutex s_periodicMutex = new(); //stage 7

    /// <summary>
    /// Retrieves a specific delivery item as a simplified list object tailored for display within an order.
    /// It fetches the Delivery entity from the DAL using the provided ID and attempts to read the associated Courier entity.
    /// Returns a BO.DeliveryPerOrderInList object, mapping data and handling potential null values for the courier or completion status.
    /// </summary>
    /// <param name="idDelivery"></param>
    /// <returns></returns>
    /// <exception cref="BO.BlDoesNotExistException"></exception>
    internal static BO.DeliveryPerOrderInList GetDeliveryPerOrderInList(int idDelivery)
    {
        DO.Delivery doDelivery;
        DO.Courier? doCourier;
        lock (AdminManager.BlMutex) //stage 7
        {
            doDelivery = s_dal.Delivery.Read(idDelivery) ?? throw new BO.BlDoesNotExistException($"Delivery with ID={idDelivery} does Not exist");
            doCourier = s_dal.Courier.Read(doDelivery.CourierId);
        }
        return new BO.DeliveryPerOrderInList()
        {
            DeliveryId = doDelivery.Id,
            CourierId = doDelivery.CourierId,
            CourierName = doCourier != null ? doCourier.NameCourier : null,
            DeliveryType = (BO.DeliveryTypeMethods)doDelivery.DeliveryType,
            StartTime = doDelivery.OrderStartDateTime,
            FinishType = doDelivery.DeliveryTypeEnding != null ? (BO.DeliveryCompletionType)doDelivery.DeliveryTypeEnding : null,
            FinishTime = doDelivery.OrderEndDateTime
        };
    }

    /// <summary>
    /// Retrieves essential summary details for a delivery that has been successfully completed.
    /// It fetches the specific Delivery and its corresponding Order from the Data Layer using their IDs.
    /// Returns a BO.ClosedDeliveryInList object containing key completion data like the actual distance, total time taken, and final completion status.
    /// </summary>
    /// <param name="idDelivery"></param>
    /// <returns></returns>
    /// <exception cref="BO.BlDoesNotExistException"></exception>
    internal static BO.ClosedDeliveryInList GetClosedDeliveryInList(int idDelivery)
    {
        try
        {
            DO.Delivery doDelivery;
            DO.Order doOrder;
            lock (AdminManager.BlMutex) //stage 7
            {
                doDelivery = s_dal.Delivery.Read(idDelivery) ?? throw new BO.BlDoesNotExistException($"Delivery with ID={idDelivery} does Not exist");
                doOrder = s_dal.Order.Read(doDelivery.OrderId) ?? throw new BO.BlDoesNotExistException($"Order with ID={doDelivery.OrderId} does Not exist");
            }
            return new BO.ClosedDeliveryInList()
            {
                DeliveryId = doDelivery.Id,
                OrderId = doDelivery.OrderId,
                OrderType = (BO.OrderRequirements)doOrder.OrderType,
                FullAddress = doOrder.OrderAddress,
                DeliveryType = (BO.DeliveryTypeMethods)doDelivery.DeliveryType,
                ActualDistance = doDelivery.DeliveryDistanceKm,
                TotalCompletionTime = doDelivery.OrderEndDateTime.HasValue ? doDelivery.OrderEndDateTime.Value - doDelivery.OrderStartDateTime : TimeSpan.Zero,
                DeliveryCompletionType = doDelivery.DeliveryTypeEnding != null ? (BO.DeliveryCompletionType)doDelivery.DeliveryTypeEnding : null
            };
        }
        catch (BO.BlNullPropertyException ex)
        {
            throw new BO.BlDoesNotExistException("Unexpected error in DeliveryManager.GetClosedDeliveryInList"+ex.Message, ex);
        }
    }

    /// <summary>
    /// Performs periodic updates for open deliveries based on time progression.
    /// The method detects deliveries whose expected or maximum delivery times
    /// were crossed between <paramref name="oldClock"/> and <paramref name="newClock"/>,
    /// and notifies the relevant couriers accordingly.
    /// </summary>
    /// <param name="oldClock">
    /// The previous system time before the update.
    /// </param>
    /// <param name="newClock">
    /// The current system time after the update.
    /// </param>
    /// <remarks>
    /// Thread safety:
    /// All accesses to the DAL are protected using <see cref="AdminManager.BlMutex"/>
    /// in order to preserve data consistency. Locks are kept as short as possible
    /// and never include asynchronous operations.
    /// 
    /// Asynchronous behavior:
    /// Notifications and time calculations are performed asynchronously and
    /// strictly outside of any lock blocks, as required.
    /// </remarks>
    /// <exception cref="BO.BlDoesNotExistException">
    /// Thrown when an unexpected DAL-related error occurs.
    /// </exception>
    /// <exception cref="BO.BlNullPropertyException">
    /// Thrown when an argument or LINQ-related error is detected.
    /// </exception>
    internal static async Task PeriodicUpdates(DateTime oldClock, DateTime newClock)
    {
        // Optional protection against concurrent executions of this periodic method
        // (if a dedicated periodic mutex was introduced in previous stages).
        // if (s_periodicMutex.CheckAndSetInProgress()) return;

        try
        {
            if (s_periodicMutex.CheckAndSetInProgress())
                return;
            List<DO.Delivery> openDeliveries;

            // Step 1:
            // Retrieve all open deliveries (deliveries without an ending type).
            // This is a DAL read operation and must be protected by a lock.
            lock (AdminManager.BlMutex)
            {
                openDeliveries = s_dal.Delivery
                    .ReadAll(d => d.DeliveryTypeEnding == null)
                    .ToList(); // Materialize immediately to release the lock early
            }

            // Step 2:
            // Create an asynchronous task for each open delivery.
            var tasks = openDeliveries.Select(async d =>
            {
                DO.Order? order;

                // Step 3:
                // Retrieve the associated order from the DAL (protected by a lock).
                lock (AdminManager.BlMutex)
                {
                    order = s_dal.Order.Read(d.OrderId);
                }

                // If the order does not exist, skip this delivery.
                if (order == null)
                    return;

                // --- Asynchronous section (no locks allowed beyond this point) ---

                // Calculate the expected delivery time asynchronously.
                var expected = await OrderManager.GetExpectedDeliveryTime(order);

                // Calculate the maximum allowed delivery time (synchronous BL logic).
                var max = OrderManager.GetMaximumDeliveryTime(order);

                // Check whether the expected delivery time was crossed in the given interval.
                if (expected.HasValue &&
                    oldClock <= expected.Value &&
                    expected.Value <= newClock)
                {
                    DO.Courier? courier;

                    // Retrieve the courier from the DAL (protected by a lock).
                    lock (AdminManager.BlMutex)
                    {
                        courier = s_dal.Courier.Read(d.CourierId);
                    }

                    // Notify the courier asynchronously (outside the lock).
                    if (courier != null)
                    {
                        await Tools.NotifyCouriersOfDeliveryTimeChange(
                            d,
                            courier,
                            $"Delivery {d.Id} crossed expected delivery time."
                        );
                    }
                }

                // Check whether the maximum delivery time was crossed in the given interval.
                if (oldClock <= max && max <= newClock)
                {
                    DO.Courier? courier;

                    // Retrieve the courier from the DAL (protected by a lock).
                    lock (AdminManager.BlMutex)
                    {
                        courier = s_dal.Courier.Read(d.CourierId);
                    }

                    // Notify the courier asynchronously (outside the lock).
                    if (courier != null)
                    {
                        await Tools.NotifyCouriersOfDeliveryTimeChange(
                            d,
                            courier,
                            $"Delivery {d.Id} passed maximum delivery time (late)."
                        );
                    }
                }
            });

            // Step 3:
            // Await completion of all delivery update tasks.
            await Task.WhenAll(tasks);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            // Wrap DAL exceptions with a BL-level exception.
            throw new BO.BlDoesNotExistException(
                "Unexpected error in DeliveryManager.PeriodicUpdates",
                ex
            );
        }
        catch (ArgumentException ex)
        {
            // Handle LINQ or argument-related errors.
            throw new BO.BlNullPropertyException(
                "Handling LINQ error: " + ex.Message
            );
        }
        finally
        {
            s_periodicMutex.UnsetInProgress();
        }
    }

}
