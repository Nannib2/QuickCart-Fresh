using BLApi;
using BO;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Helpers;

internal static class OrderManager
{
    private static readonly IDal s_dal = DalApi.Factory.Get;
    internal static ObserverManager Observers = new(); //stage 5 


    /// <summary>
    /// Retrieves the full, detailed Business Object (BO) representation of a specific order.
    /// It fetches the Data Object (DO) from the DAL and calculates numerous derived properties, including air distance, various time estimates, and current statuses.
    /// The returned object provides a comprehensive view, including the delivery history and calculated time remaining until the maximum delivery window closes.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="BO.BlDoesNotExistException"></exception>
    internal static async Task< BO.Order> GetOrder(int id)
    {
        try
        {
            DO.Order doOrder;
            double? configLat, configLon;

            // Step 1: Fetch synchronous data inside lock
            lock (AdminManager.BlMutex)
            {
                doOrder = s_dal.Order.Read(id) ?? throw new BO.BlDoesNotExistException($"Order with ID={id} does Not exist");
                configLat = s_dal.Config.Latitude;
                configLon = s_dal.Config.Longitude;
            }
           
            return new BO.Order()
            {
                Id = doOrder.Id,
                OrderType = (BO.OrderRequirements)doOrder.OrderType,
                ShortOrderDescription = doOrder.ShortOrderDescription,
                FullAddress = doOrder.OrderAddress,
                Latitude = doOrder.Latitude,
                Longitude = doOrder.Longitude,
                AirDistance = CalculateAirDistanceKm(doOrder.Latitude, doOrder.Longitude, configLat, configLon),
                CustomerFullName = doOrder.CustomerFullName,
                CustomerPhone = doOrder.CustomerPhone,
                FreeShippingEligibility = doOrder.FreeShippingEligibility,
                AmountItems = doOrder.AmountItems,
                OrderOpeningTime = doOrder.OpenOrderDateTime,
                ExpectedDeliveryTime = await GetExpectedDeliveryTime(doOrder),
                MaximumDeliveryTime = GetMaximumDeliveryTime(doOrder),
                OrderStatus = GetOrderStatus(id),
                ScheduleStatus = GetScheduleStatus(id),
                TimeRemaining = GetTimeRemaining(doOrder),
                DeliveryHistory = GetDeliveryHistory(doOrder),

            };
        }
        catch (BlNullPropertyException ex)
        {
            throw new BO.BlNullPropertyException($"errror getting an order" + ex.Message);
        }
        catch (BlDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Order with ID={id} does Not exist" + ex.Message);
        }
    }

    /// <summary>
    /// Retrieves a simplified summary of a specific order suitable for display in a general list view.
    /// It fetches the Order DO from the DAL and finds the most recent delivery associated with it to determine the latest status and delivery ID.
    /// The function calculates and includes key metrics like air distance, current status, time remaining, and total deliveries associated with the order.
    /// </summary>
    /// <param name="idOrder"></param>
    /// <returns></returns>
    /// <exception cref="BO.BlDoesNotExistException"></exception>
    internal static BO.OrderInList? GetOrderInList(int idOrder)
    {
        try
        {
            DO.Order doOrder;
            DO.Delivery? doDelivery;
            int totalDeliveriesCount;
            double? configLat, configLon;

            lock (AdminManager.BlMutex)
            {
                doOrder = s_dal.Order.Read(idOrder) ?? throw new BO.BlDoesNotExistException($"Order with ID={idOrder} does Not exist");
                var doDeliverys = s_dal.Delivery.ReadAll(delivery => delivery.OrderId == idOrder); //get all deliveries for this order
                doDelivery = doDeliverys.OrderByDescending(d => d.OrderStartDateTime).FirstOrDefault(); //get the latest delivery
                totalDeliveriesCount = s_dal.Delivery.ReadAll(d => d.OrderId == doOrder.Id).Count();
                configLat = s_dal.Config.Latitude;
                configLon = s_dal.Config.Longitude;
            }
            return new BO.OrderInList()
            {
                DeliveryId = (doDelivery != null) ? doDelivery.Id : null,  //nullable if no delivery exists yet  
                OrderId = idOrder,
                OrderType = (BO.OrderRequirements)doOrder.OrderType,
                AirDistance = CalculateAirDistanceKm(doOrder.Latitude, doOrder.Longitude, configLat, configLon),
                Status = GetOrderStatus(idOrder),
                ScheduleStatus = GetScheduleStatus(idOrder),
                TimeRemaining = GetTimeRemaining(doOrder),
                TotalCompletionTime = GetTotalCompletionTime(doOrder),
                TotalDeliveries = totalDeliveriesCount,
            };
        }
        catch (BlDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"error getting order in list" + ex.Message);
        }
    }

    /// <summary>
    /// Retrieves the specific details of the order currently being handled by a courier.
    /// It finds the single active delivery assigned to the given courier ID and then fetches the corresponding Order DO.
    /// The function populates a BO.OrderInProgress object with delivery and order details, including expected and maximum delivery times, statuses, and time remaining.
    /// </summary>
    /// <param name="idCourier"></param>
    /// <returns></returns>
    /// <exception cref="BO.BlDoesNotExistException"></exception>
    /// <exception cref="BlDoesNotExistException"></exception>
    internal static async Task<BO.OrderInProgress> GetOrderInProgress(int idCourier)
    {
        try
        {
            DO.Order doOrder;
            DO.Delivery doDelivery;
            double? configLat, configLon;

            // Step 1: Read data inside lock
            lock (AdminManager.BlMutex)
            {
                doDelivery = s_dal.Delivery.Read(delivery => delivery.CourierId == idCourier && delivery.DeliveryTypeEnding == null)
                    ?? throw new BO.BlDoesNotExistException($"Courier with ID={idCourier} for open delivery does Not exist"); //only one active delivery per courier
                doOrder = s_dal.Order.Read(doDelivery.OrderId)!;
                configLat = s_dal.Config.Latitude;
                configLon = s_dal.Config.Longitude;
            }
            return new BO.OrderInProgress()
            {
                DeliveryId = doDelivery.Id,
                OrderId = doDelivery.OrderId,
                OrderType = (BO.OrderRequirements)doOrder.OrderType,
                ShortOrderDescription = doOrder.ShortOrderDescription,
                OrderAddress = doOrder.OrderAddress,
                AirDistance = CalculateAirDistanceKm(doOrder.Latitude, doOrder.Longitude, configLat,configLon),
                ActualDistance = doDelivery.DeliveryDistanceKm,
                CustomerName = doOrder.CustomerFullName,
                CustomerPhone = doOrder.CustomerPhone,
                OrderCreatedTime = doOrder.OpenOrderDateTime,
                DeliveryStartTime = doDelivery.OrderStartDateTime,
                ExpectedDeliveryTime =  await GetExpectedDeliveryTime(doOrder) ?? throw new BlDoesNotExistException($"Delivery does Not exist"),//it will never be null here
                MaxDeliveryTime = GetMaximumDeliveryTime(doOrder),
                OrderStatus = GetOrderStatus(doDelivery.OrderId),
                ScheduleStatus = GetScheduleStatus(doDelivery.OrderId),
                RemainingTime = GetTimeRemaining(doOrder),
            };
        }
        catch (BlDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"error getting order in progress" + ex.Message);
        }
        catch (BlNullPropertyException ex)
        {
            throw new BO.BlNullPropertyException($"error getting order in progress" + ex.Message);
        }
    }

    /// <summary>
    /// Retrieves a simplified list view of an order only if its current status is 'Open'.
    /// It first fetches the Order DO from the DAL and checks the calculated OrderStatus; if not 'Open', it returns null.
    /// The returned BO.OpenOrderInList includes key attributes like air distance, item count, and the time remaining until the maximum delivery time.
    /// </summary>
    /// <param name="idOrder"></param>
    /// <returns></returns>
    /// <exception cref="BO.BlDoesNotExistException"></exception>
    internal static BO.OpenOrderInList? GetOpenOrderInList(int idOrder)
    {
        try
        {
            DO.Order doOrder;
            double? configLat, configLon;
            DateTime now;

            lock (AdminManager.BlMutex)
            {
                doOrder = s_dal.Order.Read(idOrder) ?? throw new BO.BlDoesNotExistException($"Order with ID={idOrder} does Not exist");
                configLat = s_dal.Config.Latitude;
                configLon = s_dal.Config.Longitude;
                now = AdminManager.Now;
            }

            if (GetOrderStatus(idOrder) != BO.OrderStatus.Open)
                return null;
            return new BO.OpenOrderInList()
            {
                CourierId = null,
                OrderId = doOrder.Id,
                OrderType = (BO.OrderRequirements)doOrder.OrderType,
                FreeShippingEligibility = doOrder.FreeShippingEligibility,
                AmountItems = doOrder.AmountItems,
                FullAddress = doOrder.OrderAddress,
                AirDistance = CalculateAirDistanceKm(doOrder.Latitude, doOrder.Longitude, configLat, configLon),
                ActualDistance = null,
                ActualDeliveryTimeSpan = null,
                ScheduleStatus = GetScheduleStatus(idOrder),
                TimeToFinish = GetMaximumDeliveryTime(doOrder) - now,
                MaximumDeliveryTime = GetMaximumDeliveryTime(doOrder),
            };
        }
        catch (BlDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"error getting open order in list" + ex.Message);
        }
    }

    /// <summary>
    /// Calculates the geographical distance in kilometers between two points 
    /// using the Haversine formula.
    /// </summary>
    /// <param name="lat1">Latitude of the first point (in degrees).</param>
    /// <param name="lon1">Longitude of the first point (in degrees).</param>
    /// <param name="lat2">Latitude of the second point (in degrees).</param>
    /// <param name="lon2">Longitude of the second point (in degrees).</param>
    /// <returns>The distance in kilometers.</returns>
    /// using AI assistance
    /// https://gemini.google.com/share/2e5958cd9c69
    internal static double CalculateAirDistanceKm(double lat1, double lon1, double? lat2, double? lon2)
    {
        // Check for null values
        if (lat2 == null || lon2 == null)
            return 0;

        // Earth's radius in kilometers
        const double R = 6371.0;

        // Convert nullable to non-nullable
        double lat2Value = lat2.Value;
        double lon2Value = lon2.Value;

        // Convert degrees to radians
        var lat1Rad = ToRadians(lat1);
        var lon1Rad = ToRadians(lon1);
        var lat2Rad = ToRadians(lat2Value);
        var lon2Rad = ToRadians(lon2Value);

        // Calculate the difference in coordinates
        var deltaLat = lat2Rad - lat1Rad;
        var deltaLon = lon2Rad - lon1Rad;

        // Haversine formula
        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        // Distance = R * c
        var distance = R * c;
        return distance;
    }

    /// <summary>
    /// Helper function to convert degrees to radians.
    /// </summary>
    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }

    /// <summary>
    /// calculates the status of an order based on its delivery information.
    /// Determines the current logical status of an order (Open, InProgress, Delivered, Rejected, or Canceled).
    /// It fetches the order and its latest associated delivery from the DAL to establish the status hierarchy.
    /// The status is based on whether a delivery exists, whether it is currently active (InProgress), or the specific type of completion recorded.
    /// </summary>
    /// <param name="idOrder"></param>
    /// <returns></returns>
    /// <exception cref="BO.BlDoesNotExistException"></exception>
    internal static BO.OrderStatus GetOrderStatus(int idOrder)  //to check
    {
        lock (AdminManager.BlMutex)
        {
            DO.Order doOrder;
            doOrder = s_dal.Order.Read(idOrder) ?? throw new BO.BlDoesNotExistException($"Order with ID={idOrder} does Not exist");
            var doDeliverys = s_dal.Delivery.ReadAll(delivery => delivery.OrderId == idOrder).ToList(); // .ToList() inside lock!
            var doDelivery = doDeliverys.OrderByDescending(d => d.OrderStartDateTime).FirstOrDefault();

            if (doDelivery == null)//no delivery yet
            {
                return BO.OrderStatus.Open;
            }
            else if (doDelivery.DeliveryTypeEnding == null)//delivery in progress
            {
                return BO.OrderStatus.InProgress;
            }
            else if (doDelivery.DeliveryTypeEnding == DO.DeliveryCompletionType.Supplied)//delivered successfully
            {
                return BO.OrderStatus.Delivered;
            }
            else
            {
                if (doDelivery.DeliveryTypeEnding == DO.DeliveryCompletionType.CustomerRefused)//rejected by customer
                {
                    return BO.OrderStatus.Rejected;
                }
                else
                {
                    return BO.OrderStatus.Canceled;
                }
            }
        }
    }
    

    /// <summary>
    /// Determines the schedule status (OnTime, InRisk, or Late) of an order relative to its maximum allowed delivery time.
    /// For open/in-progress orders, it compares the time remaining until the maximum deadline against the defined RiskTimeRange.
    /// For completed orders, it compares the actual delivery end time against the calculated maximum delivery time based on the order open time.
    /// </summary>
    /// <param name="idOrder"></param>
    /// <returns></returns>
    /// <exception cref="BO.BlDoesNotExistException"></exception>
    internal static BO.ScheduleStatus GetScheduleStatus(int idOrder)
    {
        try
        {
            DO.Order doOrder;
            DO.Delivery? doDelivery;
            TimeSpan maxDeliveryTimeRange;
            TimeSpan riskTimeRange;
            DateTime now;

            lock (AdminManager.BlMutex)
            {
                doOrder = s_dal.Order.Read(idOrder) ?? throw new BO.BlDoesNotExistException($"Order with ID={idOrder} does Not exist");
                var doDeliverys = s_dal.Delivery.ReadAll(delivery => delivery.OrderId == idOrder).ToList();
                doDelivery = doDeliverys.OrderByDescending(d => d.OrderStartDateTime).FirstOrDefault();
                maxDeliveryTimeRange = s_dal.Config.MaxDeliveryTimeRange;
                riskTimeRange = s_dal.Config.RiskTimeRange;
                now = AdminManager.Now;
            }
            if (GetOrderStatus(idOrder) == BO.OrderStatus.Open || GetOrderStatus(idOrder) == BO.OrderStatus.InProgress)//order not yet delivered or rejected etc
            {
                if ((doOrder.OpenOrderDateTime + maxDeliveryTimeRange - now) > riskTimeRange
                    && (now - doOrder.OpenOrderDateTime) <= maxDeliveryTimeRange)
                {
                    return BO.ScheduleStatus.OnTime;
                }
                else
                {
                    if ((doOrder.OpenOrderDateTime + maxDeliveryTimeRange - now) <= riskTimeRange
                        && (now - doOrder.OpenOrderDateTime) <= maxDeliveryTimeRange)
                        return BO.ScheduleStatus.InRisk;
                    return BO.ScheduleStatus.Late;
                }

            }
            else
            {
                if (doDelivery != null && doDelivery.OrderEndDateTime <= doOrder.OpenOrderDateTime + maxDeliveryTimeRange)
                {
                    return BO.ScheduleStatus.OnTime;
                }
                return BO.ScheduleStatus.Late;
            }
        }
        catch (BlDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Error calculates Schedule Status " + ex.Message);
        }
    }

    /// <summary>
    /// Calculates the estimated time of delivery (ETD) for an order if it is currently in progress.
    /// It finds the latest (active) delivery for the order and determines the courier's transportation type (e.g., Car, Motorcycle).
    /// Using the configured average speed for that transportation type and the calculated travel distance from the hub to the destination,
    /// it calculates the expected duration and adds it to the order's opening time to estimate the arrival time.
    /// </summary>
    /// <param name="doOrder"></param>
    /// <returns></returns>
    /// <exception cref="BlNullPropertyException"></exception>
    internal static async Task<DateTime?> GetExpectedDeliveryTime(DO.Order doOrder)
    {
        try
        {
            DO.Delivery? doDelivery;
            double? latConfig, lonConfig;
            double speed;
            BO.DeliveryPerOrderInList? boDelivery = null;

            // Step 1: Read all DAL-related data under lock
            lock (AdminManager.BlMutex)
            {
                var doDeliverys = s_dal.Delivery
                    .ReadAll(delivery => delivery.OrderId == doOrder.Id)
                    .ToList();

                doDelivery = doDeliverys
                    .OrderByDescending(d => d.OrderStartDateTime)
                    .FirstOrDefault();

                latConfig = s_dal.Config.Latitude;
                lonConfig = s_dal.Config.Longitude;

                if (doDelivery != null && doDelivery.DeliveryTypeEnding == null)
                {
                    boDelivery = DeliveryManager.GetDeliveryPerOrderInList(doDelivery.Id);
                }
            }

            // Step 2: Business logic and async calculations outside lock
            if (boDelivery != null)
            {
                double averageSpeedKmPerHour = 1;

                // Read speed configuration under a short, focused lock
                lock (AdminManager.BlMutex)
                {
                    switch (boDelivery.DeliveryType)
                    {
                        case BO.DeliveryTypeMethods.Motorcycle:
                            averageSpeedKmPerHour = s_dal.Config.AverageMotorcycleSpeedKmh;
                            break;
                        case BO.DeliveryTypeMethods.Car:
                            averageSpeedKmPerHour = s_dal.Config.AverageCarSpeedKmh;
                            break;
                        case BO.DeliveryTypeMethods.Bike:
                            averageSpeedKmPerHour = s_dal.Config.AverageBicycleSpeedKmh;
                            break;
                        case BO.DeliveryTypeMethods.Foot:
                            averageSpeedKmPerHour = s_dal.Config.AverageWalkingSpeedKmh;
                            break;
                    }
                }

                speed = averageSpeedKmPerHour;

                double distance = await Tools.CalculateDistance(
                    doOrder.Latitude,
                    doOrder.Longitude,
                    latConfig,
                    lonConfig,
                    boDelivery.DeliveryType
                );

                TimeSpan expectedTimeSpan = TimeSpan.FromHours(distance / speed);
                return doOrder.OpenOrderDateTime + expectedTimeSpan;
            }

            return null;
        }
        catch (BlDoesNotExistException ex)
        {
            // Keep exception behavior identical to the original implementation
            throw new BO.BlDoesNotExistException(
                "Error calculating expected delivery time" + ex.Message
            );
        }
        catch (BlExternalServiceException ex)
        {
            throw new BlExternalServiceException("Error calculating expected delivery time" + ex.Message);
        }
        catch (BlInvalidInputException ex)
        {
            throw new BlInvalidInputException("Error calculating expected delivery time" + ex.Message);
        }
    }


    /// <summary>
    /// Calculates the expected duration (TimeSpan) required to complete a delivery for a given Order and Courier.
    /// It first determines the courier's transportation type from their Business Object (BO) details.
    /// Then, it calculates the travel distance from the main hub to the order's location, divides this distance by the system's 
    /// configured average speed for that transportation type, and returns the result as a TimeSpan.
    /// </summary>
    /// <param name="doCourier"></param>
    /// <param name="doOrder"></param>
    /// <returns></returns>
    /// <exception cref="BlNullPropertyException"></exception>
    internal static  async Task <TimeSpan> GetActualDeliveryTimeSpan(DO.Courier doCourier, DO.Order doOrder)
    {
        try
        {
            BO.Courier boCourier = await CourierManager.GetCourier(doCourier.Id) ?? throw new BlNullPropertyException("DO.Courier is null");
            double averageSpeedKmPerHour = 1;
            double? configLat, configLon;

            lock (AdminManager.BlMutex)
            {
                configLat = s_dal.Config.Latitude;
                configLon = s_dal.Config.Longitude;

                switch (boCourier.DeliveryType)
                {
                    case BO.DeliveryTypeMethods.Motorcycle:
                        averageSpeedKmPerHour = s_dal.Config.AverageMotorcycleSpeedKmh;
                        break;
                    case BO.DeliveryTypeMethods.Car:
                        averageSpeedKmPerHour = s_dal.Config.AverageCarSpeedKmh;
                        break;
                    case BO.DeliveryTypeMethods.Bike:
                        averageSpeedKmPerHour = s_dal.Config.AverageBicycleSpeedKmh;
                        break;
                    case BO.DeliveryTypeMethods.Foot:
                        averageSpeedKmPerHour = s_dal.Config.AverageWalkingSpeedKmh;
                        break;
                }
            }

            double distance = await Tools.CalculateDistance(doOrder.Latitude, doOrder.Longitude, configLat, configLon, boCourier.DeliveryType);
            TimeSpan expectedTimeSpan = TimeSpan.FromHours(distance / averageSpeedKmPerHour);
            return expectedTimeSpan;
        }
        catch (BlDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Error calculating actual delivery time span" + ex.Message);
        }
        catch (BlExternalServiceException ex)
        {
            throw new BlExternalServiceException("Error calculating expected delivery time" + ex.Message);
        }
        catch (BlInvalidInputException ex)
        {
            throw new BlInvalidInputException("Error calculating expected delivery time" + ex.Message);
        }
    }

    /// <summary>
    /// Calculates the time remaining until the maximum allowed delivery deadline for an order.
    /// This is only calculated if the order is currently in an 'Open' or 'InProgress' status; otherwise, it returns TimeSpan.Zero.
    /// The result is derived by subtracting the current system time (AdminManager.Now) from the order's MaximumDeliveryTime.
    /// </summary>
    /// <param name="doOrder"></param>
    /// <returns></returns>
    internal static TimeSpan GetTimeRemaining(DO.Order doOrder)
    {
        try
        {
            if (GetOrderStatus(doOrder.Id) == BO.OrderStatus.Open || GetOrderStatus(doOrder.Id) == BO.OrderStatus.InProgress)
            {
                if (GetScheduleStatus(doOrder.Id) == BO.ScheduleStatus.Late)
                {
                    return TimeSpan.Zero;
                }

                DateTime now;
                lock (AdminManager.BlMutex)
                {
                    now = AdminManager.Now;
                }

                TimeSpan timeRemaining = GetMaximumDeliveryTime(doOrder) - now;
                return timeRemaining;
            }

            return TimeSpan.Zero;
        }
        catch (BO.BlDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Error calculating time remaining" + ex.Message);
        }
    }


    /// <summary>
    /// Calculates the absolute deadline for the delivery of a specific order.
    /// This deadline is determined by adding the system's configured maximum allowed delivery duration (MaxDeliveryTimeRange) to the order's initial opening time.
    /// The returned value is a fixed DateTime representing the final time the delivery should be completed to be considered on time.
    /// </summary>
    /// <param name="doOrder"></param>
    /// <returns></returns>
    internal static DateTime GetMaximumDeliveryTime(DO.Order doOrder)
    {
        lock (AdminManager.BlMutex)
        {
            return doOrder.OpenOrderDateTime + s_dal.Config.MaxDeliveryTimeRange;
        }
    }

    /// <summary>
    /// Retrieves the complete historical record of all delivery attempts associated with a specific order.
    /// It queries the DAL for all Delivery Data Objects (DO) linked to the order ID.
    /// The function then maps each DO.Delivery into a simplified Business Object list view (BO.DeliveryPerOrderInList) and returns the list.
    /// </summary>
    /// <param name="doOrder"></param>
    /// <returns></returns>
    internal static List<BO.DeliveryPerOrderInList> GetDeliveryHistory(DO.Order doOrder)
    {
        lock (AdminManager.BlMutex)
        {
            return s_dal.Delivery.ReadAll(d => d.OrderId == doOrder.Id)
                                 .Select(d => DeliveryManager.GetDeliveryPerOrderInList(d.Id))
                                 .ToList();
        }
    }

    /// <summary>
    /// Calculates the total duration from the order's opening time to its final completion time.
    /// It searches for the latest completed delivery (where DeliveryTypeEnding is not null) associated with the order.
    /// If a completed delivery is found, the TimeSpan between the order's OpenOrderDateTime and the last successful OrderEndDateTime is returned; otherwise, it returns TimeSpan.Zero.
    /// </summary>
    /// <param name="doOrder"></param>
    /// <returns></returns>
    internal static TimeSpan GetTotalCompletionTime(DO.Order doOrder)
    {
        try
        {
            lock (AdminManager.BlMutex)
            {
                DO.Delivery? doDelivery;
                doDelivery = (
                        from delivery in s_dal.Delivery.ReadAll(d => d.OrderId == doOrder.Id)
                        where delivery.DeliveryTypeEnding != null
                        orderby delivery.OrderEndDateTime descending
                        select delivery)
                        .FirstOrDefault();

                if (doDelivery != null)
                {
                    return doDelivery.OrderEndDateTime - doOrder.OpenOrderDateTime ?? TimeSpan.Zero;
                }
                return TimeSpan.Zero;
            }
        }
        catch (ArgumentException ex)
        {
            throw new BO.BlNullPropertyException("handaling LINQ error" + ex.Message);
        }
    }

    /// <summary>
    /// Generates a summarized array of order quantities, categorized by the unique combination of their OrderStatus and ScheduleStatus.
    /// Access is restricted to the manager (requesterId must match s_dal.Config.ManagerId).
    /// The resulting integer array is sized to hold a count for every possible pair of statuses, mapped to a linear index using the formula: Index = (OrderStatus index * ScheduleStatus count) + ScheduleStatus index.
    /// </summary>
    /// <param name="requesterId"></param>
    /// <returns></returns>
    /// <exception cref="BO.BlInvalidOperationException"></exception>
    internal static int[] GetOrderQuantitiesByStatus(int requesterId)
    {
        try
        {
            // Permission check – must be done outside the lock
            if (requesterId != s_dal.Config.ManagerId)
            {
                throw new BO.BlInvalidOperationException(
                    "Only the manager can access order summaries.");
            }

            // Calculate enum sizes (pure logic, no DAL access)
            int orderStatusCount = Enum.GetValues(typeof(BO.OrderStatus)).Length;
            int scheduleStatusCount = Enum.GetValues(typeof(BO.ScheduleStatus)).Length;
            int totalStatuses = orderStatusCount * scheduleStatusCount;

            // Will hold all orders fetched from DAL
            List<DO.Order> allOrders;

            // DAL access must be protected by BlMutex
            lock (AdminManager.BlMutex) // stage 7
            {
                // Single DAL transaction: read all orders
                allOrders = s_dal.Order.ReadAll().ToList();
            }

            // Group orders by the combination of OrderStatus and ScheduleStatus
            var groupedQuantities = allOrders
                .Select(doOrder => new
                {
                    OrderStatus = GetOrderStatus(doOrder.Id),
                    ScheduleStatus = GetScheduleStatus(doOrder.Id)
                })
                .GroupBy(order => new
                {
                    OrderState = order.OrderStatus,
                    TimingState = order.ScheduleStatus
                })
                .Select(g => new
                {
                    // Linearized index based on both enum values
                    Index = ((int)g.Key.OrderState * scheduleStatusCount)
                            + (int)g.Key.TimingState,
                    Count = g.Count()
                })
                .ToDictionary(g => g.Index, g => g.Count);

            // Build the result array, defaulting missing combinations to 0
            int[] orderQuantitiesByStatus = Enumerable.Range(0, totalStatuses)
                .Select(i =>
                    groupedQuantities.TryGetValue(i, out int count)
                        ? count
                        : 0)
                .ToArray();

            return orderQuantitiesByStatus;
        }
        catch (BO.BlDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException(
                "Error getting order quantities by status. " + ex.Message);
        }
    }


    /// <summary>
    /// Retrieves the full, detailed Business Object (BO) representation of a specific order.
    /// Access is strictly controlled, permitting only the system manager (requesterId matching s_dal.Config.ManagerId).
    /// It calls the internal GetOrder function to build the full object and handles exceptions if the specified order ID does not exist.
    /// </summary>
    /// <param name="idOrder"></param>
    /// <param name="requesterId"></param>
    /// <returns></returns>
    /// <exception cref="BO.BlInvalidOperationException"></exception>
    /// <exception cref="BO.BlDoesNotExistException"></exception>
    internal static  async Task<BO.Order> ReadOrder(int idOrder, int requesterId)
    {
        try
        {
            lock (AdminManager.BlMutex)
            {
                // Permission Check: Ensure only the defined Manager ID can access order details.
                if (requesterId != s_dal.Config.ManagerId)
                {
                    throw new BO.BlInvalidOperationException("Only the manager can access order details.");
                }
            }
            // Retrieve and return the detailed order information.
            return await GetOrder(idOrder);
        }
        catch (BlDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Order with ID={idOrder} does Not exist" + ex.Message);
        }
    }
    /// <summary>
    /// Retrieves a filtered and sorted list of orders in a summarized format.
    /// </summary>
    /// <remarks>This method is intended for use by the manager to retrieve a customizable view of orders.
    /// Filters and sorting options can be applied to tailor the results to specific requirements.</remarks>
    /// <param name="requesterId">The ID of the user requesting the list. Must be the manager's ID; otherwise, access is denied.</param>
    /// <param name="statusFilter">An optional filter to include only orders with the specified status.</param>
    /// <param name="typeFilter">An optional filter to include only orders of the specified type.</param>
    /// <param name="scheduleFilter">An optional filter to include only orders with the specified schedule status.</param>
    /// <param name="sortBy">An optional parameter specifying the property by which to sort the orders. Defaults to sorting by status if not
    /// provided.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="BO.OrderInList"/> objects representing the filtered and sorted
    /// orders.</returns>
    /// <exception cref="BO.BlInvalidOperationException">Thrown if the <paramref name="requesterId"/> does not match the manager's ID.</exception>
    internal static IEnumerable<BO.OrderInList> ReadAllOrdersInList(int requesterId, BO.OrderStatus? statusFilter = null, BO.OrderRequirements? typeFilter = null, BO.ScheduleStatus? scheduleFilter = null, BO.OrderInListProperties? sortBy = null)
    {
        IEnumerable<BO.OrderInList> query;
        lock (AdminManager.BlMutex)
        {
            if (requesterId != s_dal.Config.ManagerId)
            {
                throw new BO.BlInvalidOperationException("Access denied. Only manager can view this list.");
            }

            // Materialize the list inside lock to prevent DAL access outside lock
            query = s_dal.Order.ReadAll().Select(doOrder => GetOrderInList(doOrder.Id)!).ToList();
        }

        //filter by status
        if (statusFilter.HasValue)
        {
            query = query.Where(o => o.Status == statusFilter.Value);
        }

        ///filter by order type
        if (typeFilter.HasValue)
        {
            query = query.Where(o => o.OrderType == typeFilter.Value);
        }

        //filter by schedule status
        if (scheduleFilter.HasValue)
        {
            query = query.Where(o => o.ScheduleStatus == scheduleFilter.Value);
        }

        //sorting logic
        // sort by specified property or default to Status
        if (sortBy.HasValue)
        {
            switch (sortBy)
            {

                case BO.OrderInListProperties.OrderId:
                    query = query.OrderBy(o => o.OrderId);
                    break;

                case BO.OrderInListProperties.OrderType:
                    query = query.OrderBy(o => o.OrderType);
                    break;

                case BO.OrderInListProperties.AirDistance:
                    query = query.OrderBy(o => o.AirDistance);
                    break;

                case BO.OrderInListProperties.Status:
                    query = query.OrderBy(o => o.Status);
                    break;

                case BO.OrderInListProperties.ScheduleStatus:
                    query = query.OrderBy(o => o.ScheduleStatus);
                    break;

                case BO.OrderInListProperties.TimeRemaining:
                    query = query.OrderBy(o => o.TimeRemaining);
                    break;

                case BO.OrderInListProperties.TotalCompletionTime:
                    query = query.OrderBy(o => o.TotalCompletionTime);
                    break;

                case BO.OrderInListProperties.TotalDeliveries:
                    query = query.OrderBy(o => o.TotalDeliveries);
                    break;


                //default sorting by Status
                default:
                    query = query.OrderBy(o => o.Status);
                    break;
            }
        }
        else
        {
            //default sorting by Status
            query = query.OrderBy(o => o.Status);
        }

        return query;
    }
    

        /// <summary>
        /// Updates the modifiable fields of an existing order with new data, ensuring data integrity and authorization.
        /// Access is restricted to the system manager. It validates that the order exists and that unchangeable fields (ID, OpenOrderDateTime) remain constant.
        /// The function validates the customer's phone number, calculates new geographic coordinates for the address, determines free shipping eligibility based on item amount, and persists the changes to the DAL.
        /// </summary>
        /// <param name="order"></param>
        /// <param name="requesterId"></param>
        /// <exception cref="BO.BlInvalidOperationException"></exception>
        /// <exception cref="BO.BlDoesNotExistException"></exception>
        /// <exception cref="BO.BlInvalidInputException"></exception>
    internal static async Task UpdateOrder(BO.Order order, int requesterId)
    {
        try
        {
          

            DO.Order doOrder;

            // Step 1: Read and Check
            lock (AdminManager.BlMutex)
            {
                if (requesterId != s_dal.Config.ManagerId)
                    throw new BO.BlInvalidOperationException("Only the manager can access order summaries.");

                doOrder = s_dal.Order.Read(order.Id) ?? throw new BO.BlDoesNotExistException($"Order with ID={order.Id} does Not exist");

                if (doOrder.Id != order.Id)
                    throw new BO.BlInvalidOperationException("Cannot change the ID of an existing order.");
                if (doOrder.OpenOrderDateTime != order.OrderOpeningTime)
                    throw new BO.BlInvalidOperationException("Cannot change the opening time of an existing order.");
            }
            double[] Coordinates;

            // Step 2: Async Work
            if (order.FullAddress != doOrder.OrderAddress)
            {
                Coordinates = await Tools.CalculateCoordinate(order.FullAddress);
            }
            else
            { 
                Coordinates = new double[] { doOrder.Latitude, doOrder.Longitude };
            }

                string phone = Tools.IsValidMobileNumber(order.CustomerPhone) ? order.CustomerPhone : throw new BO.BlInvalidInputException("Invalid Phone Number format.");

            // Step 3: Write
            lock (AdminManager.BlMutex)
            {
                DO.Order updatedDoOrder = doOrder with
                {
                    OrderType = (DO.OrderRequirements)order.OrderType,
                    ShortOrderDescription = order.ShortOrderDescription,
                    OrderAddress = order.FullAddress,
                    Latitude = Coordinates[0],
                    Longitude = Coordinates[1],
                    CustomerFullName = order.CustomerFullName,
                    CustomerPhone = phone,
                    FreeShippingEligibility = order.AmountItems >= 20 ? true : false,
                    AmountItems = order.AmountItems,
                };
                s_dal.Order.Update(updatedDoOrder);
            }

            // Notify outside lock
            Observers.NotifyItemUpdated(order.Id);
        }
        catch (BlDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"error updating order" + ex.Message);
        }
        catch (BlInvalidInputException ex)

        {
            lock (AdminManager.BlMutex)
            {
                if (ex.Message == "Address not found.")
                {


                    DO.Order doOrder = s_dal.Order.Read(order.Id)!;

                    var delivery = new DO.Delivery
                    {
                        OrderId = doOrder.Id,
                        OrderStartDateTime = AdminManager.Now,
                        OrderEndDateTime = AdminManager.Now,
                        DeliveryTypeEnding = DO.DeliveryCompletionType.Failed,
                        CourierId = 0,
                    };
                    s_dal.Delivery.Create(delivery);
                }
            }

            throw new BO.BlInvalidInputException($"error updating order" + ex.Message);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"error updating order" + ex.Message);
        }
    }

    /// <summary>
    /// Cancels an order, updating the system status and notifying the courier if the delivery was in progress.
    /// Access is restricted to the system manager. The order must be either 'Open' or 'InProgress'.
    /// If 'Open', a new Canceled delivery record is immediately created; if 'InProgress', the active delivery record is updated to Canceled, and the assigned courier is notified.
    /// </summary>
    /// <param name="idOrder"></param>
    /// <param name="requesterId"></param>
    /// <exception cref="BO.BlInvalidOperationException"></exception>
    /// <exception cref="BO.BlDoesNotExistException"></exception>
    internal static async Task CancelOrder(int idOrder, int requesterId)
    {
        try
        {
            

            DO.Order doOrder;
            BO.Order boOrder;
            DO.Delivery? activeDelivery = null;
            DO.Delivery? updatedDelivery = null;
            bool isInProgress = false;

            // Step 1: Read and Validation
            lock (AdminManager.BlMutex)
            {
                if (requesterId != s_dal.Config.ManagerId)
                    throw new BO.BlInvalidOperationException("Only the manager can access order summaries.");

                doOrder = s_dal.Order.Read(idOrder) ?? throw new BO.BlDoesNotExistException($"Order with ID={idOrder} does Not exist");
                // Calling GetOrder is async, so we pause lock, but GetOrder handles its own locks internally.
                // However, we need to know status for logic.
                // We'll call GetOrder OUTSIDE the lock to be safe with async.
            }

            boOrder = await GetOrder(idOrder);

            if (boOrder.OrderStatus != OrderStatus.Open && boOrder.OrderStatus != OrderStatus.InProgress)
                throw new BO.BlInvalidOperationException("Only open or in-progress orders can be canceled.");

            // Step 2: Perform Updates
            lock (AdminManager.BlMutex)
            {
                if (boOrder.OrderStatus == OrderStatus.Open)
                {
                    var delivery = new DO.Delivery
                    {
                        OrderId = idOrder,
                        OrderStartDateTime = AdminManager.Now,
                        OrderEndDateTime = AdminManager.Now,
                        DeliveryTypeEnding = DO.DeliveryCompletionType.Canceled,
                        CourierId = 0,
                    };
                    s_dal.Delivery.Create(delivery);
                }

                if (boOrder.OrderStatus == OrderStatus.InProgress)
                {
                    activeDelivery = s_dal.Delivery.Read(delivery => (delivery.OrderId == idOrder && delivery.OrderEndDateTime == null && delivery.DeliveryTypeEnding == null))!;
                    updatedDelivery = activeDelivery with
                    {
                        DeliveryTypeEnding = DO.DeliveryCompletionType.Canceled,
                        OrderEndDateTime = AdminManager.Now,
                    };
                    s_dal.Delivery.Update(updatedDelivery);
                    isInProgress = true;
                }
            }

            // Step 3: Notifications (Outside Lock)
            if (isInProgress && activeDelivery != null)
            {
                // We need to read courier for email. Lock brief read.
                DO.Courier courier;
                lock (AdminManager.BlMutex) { courier = s_dal.Courier.Read(activeDelivery.CourierId)!; }

                await Tools.NotifyCourierOfCancellation(boOrder, courier);
                Observers.NotifyListUpdated();
                Observers.NotifyItemUpdated(activeDelivery.CourierId);
               //CourierManager.Observers.NotifyItemUpdated(activeDelivery.CourierId);
                Helpers.CourierManager.Observers.NotifyItemUpdated(activeDelivery.CourierId);
            }
        }
        catch (BlDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Order with ID={idOrder} does Not exist" + ex.Message);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Order with ID={idOrder} does Not exist" + ex.Message);
        }
    }
    /// <summary>
    /// Prevents the deletion of any order, enforcing a policy that orders must be retained for historical and auditing purposes.
    /// This function unconditionally throws a BlInvalidOperationException, as deleting orders is not permitted within the system.
    /// </summary>
    /// <param name="idOrder"></param>
    /// <param name="requesterId"></param>
    /// <exception cref="BlInvalidOperationException"></exception>
    internal static void DeleteOrder(int idOrder, int requesterId)
    {
        throw new BlInvalidOperationException("It is not possible to delete orders in the system at all.");
    }

    /// <summary>
    /// Creates a new order in the system, validating the input and notifying capable couriers.
    /// Access is restricted to the system manager. It validates the customer's phone number and calculates coordinates from the address.
    /// After creation, it retrieves the new order's ID, identifies couriers within the order's distance limit, and sends them a notification email about the new open order.
    /// </summary>
    /// <param name="order"></param>
    /// <param name="requesterId"></param>
    /// <exception cref="BO.BlInvalidOperationException"></exception>
    /// <exception cref="BO.BlInvalidInputException"></exception>
    internal static async Task CreateOrder(BO.Order order, int requesterId)
    {
        try
        {
           

            
            string validatedPhone = Tools.IsValidMobileNumber(order.CustomerPhone) ? order.CustomerPhone : throw new BO.BlInvalidInputException("Invalid Phone Number format.");
            double[] Coordinates = await Tools.CalculateCoordinate(order.FullAddress);

            DO.Order newDoOrder;
            List<string> courierEmails;
            string companyAddress;
            DO.Order createdOrderWithId;

            lock (AdminManager.BlMutex)
            {
                if (requesterId != s_dal.Config.ManagerId)
                    throw new BO.BlInvalidOperationException("Only the manager can create an order.");

                newDoOrder = new DO.Order()
                {
                    Id = 0,
                    OrderType = (DO.OrderRequirements)order.OrderType,
                    ShortOrderDescription = order.ShortOrderDescription,
                    OrderAddress = order.FullAddress,
                    Latitude = Coordinates[0],
                    Longitude = Coordinates[1],
                    CustomerFullName = order.CustomerFullName,
                    CustomerPhone = validatedPhone,
                    FreeShippingEligibility = order.AmountItems >= 20 ? true : false,
                    AmountItems = order.AmountItems,
                    OpenOrderDateTime = AdminManager.Now,
                };

                s_dal.Order.Create(newDoOrder);

                // Fetch necessary data for notification inside lock
                double? configLat = s_dal.Config.Latitude;
                double? configLon = s_dal.Config.Longitude;
                companyAddress = s_dal.Config.AddressCompany??throw new BO.BlNullPropertyException("Error creating order: The company address has not yet been initialized. ");

                // Logic to find couriers
                courierEmails = s_dal.Courier.ReadAll(c => c.PersonalMaxAirDistance <= CalculateAirDistanceKm(newDoOrder.Latitude, newDoOrder.Longitude, configLat, configLon))
                                             .Select(c => c.EmailCourier).ToList();

                // Get ID
                createdOrderWithId = s_dal.Order.ReadAll().Last();
            }

            Observers.NotifyListUpdated();
            await Tools.NotifyCouriersOfNewOrder(createdOrderWithId, courierEmails, companyAddress);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Error creating order:" + ex.Message);
        }
        catch (BlInvalidInputException ex)
        {
            lock (AdminManager.BlMutex)
            {
                if (ex.Message == "Address not found.")
                {

                    DO.Order newDoOrder = new DO.Order()
                    {
                        Id = 0,
                        OrderType = (DO.OrderRequirements)order.OrderType,
                        ShortOrderDescription = order.ShortOrderDescription,
                        OrderAddress = "",
                        Latitude = 0,
                        Longitude = 0,
                        CustomerFullName = order.CustomerFullName,
                        CustomerPhone = Tools.IsValidMobileNumber(order.CustomerPhone) ? order.CustomerPhone : throw new BO.BlInvalidInputException("Invalid Phone Number format."),
                        FreeShippingEligibility = order.AmountItems >= 20 ? true : false,
                        AmountItems = order.AmountItems,
                        OpenOrderDateTime = AdminManager.Now,
                    };

                    s_dal.Order.Create(newDoOrder);

                    var delivery = new DO.Delivery
                    {
                        OrderId = newDoOrder.Id,
                        OrderStartDateTime = AdminManager.Now,
                        OrderEndDateTime = AdminManager.Now,
                        DeliveryTypeEnding = DO.DeliveryCompletionType.Failed,
                        CourierId = 0,
                    };
                    s_dal.Delivery.Create(delivery);
                }
            }
            throw new BO.BlInvalidInputException("Error creating order: " + ex.Message);
        }
    }

    /// <summary>
    /// Marks a currently active delivery as successfully completed (Supplied) by the assigned courier.
    /// Access is restricted to the specific courier (requesterId must match courierId). It validates the existence of the active delivery.
    /// The function updates the delivery record in the DAL by setting the DeliveryTypeEnding to 'Supplied' and recording the OrderEndDateTime as the current time.
    /// </summary>
    /// <param name="requesterId"></param>
    /// <param name="courierId"></param>
    /// <param name="deliveryId"></param>
    /// <exception cref="BO.BlInvalidOperationException"></exception>
    /// <exception cref="BO.BlDoesNotExistException"></exception>
    internal static void FinishDeliveryHandling(int requesterId, int courierId, int deliveryId, DO.DeliveryCompletionType completionType)
    {
        try
        {
             

            DO.Delivery updateDelivery;

            lock (AdminManager.BlMutex)
            {
                if (requesterId != courierId)
                    throw new BO.BlInvalidOperationException("Couriers can only finish their own deliveries.");

                DO.Delivery doDelivery = s_dal.Delivery.Read(d => d.Id == deliveryId && d.CourierId == courierId && d.DeliveryTypeEnding == null)
                    ?? throw new BO.BlDoesNotExistException($"Delivery with ID={deliveryId} does Not exist");

                updateDelivery = doDelivery with
                {
                    DeliveryTypeEnding = completionType,
                    OrderEndDateTime = AdminManager.Now,
                };
                s_dal.Delivery.Update(updateDelivery);
            }

            //// Notifications outside lock
            //Observers.NotifyItemUpdated(updateDelivery.Id);
            //Observers.NotifyItemUpdated(updateDelivery.CourierId);
            //Observers.NotifyListUpdated();
            //CourierManager.Observers.NotifyListUpdated();
            //CourierManager.Observers.NotifyItemUpdated(updateDelivery.CourierId);
            Observers.NotifyItemUpdated(updateDelivery.Id);
            Observers.NotifyListUpdated();

            Helpers.CourierManager.Observers.NotifyItemUpdated(updateDelivery.CourierId);
            Helpers.CourierManager.Observers.NotifyListUpdated();

        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Error finishing delivery,errore at dal layer:" + ex.Message);
        }
    }

    /// <summary>
    /// Assigns an open order to a courier, starting a new delivery process.
    /// Access is restricted to the courier themselves (requesterId must match courierId). It validates that the courier is not currently handling another order and that the target order is 'Open'.
    /// It performs a capability check (distance/type) and, if successful, creates a new active Delivery record in the DAL, then sends an order confirmation email to the courier.
    /// </summary>
    /// <param name="requesterId"></param>
    /// <param name="courierId"></param>
    /// <param name="orderId"></param>
    /// <exception cref="BO.BlDoesNotExistException"></exception>
    /// <exception cref="BO.BlUnKnowErrorException"></exception>
    internal static async Task SelectOrderForHandling(int requesterId, int courierId, int orderId)
    {
        try
        {
           

            DO.Courier doCourier;
            BO.Order boOrder;
            BO.Courier boCourier;
            string companyAddress;

            // Step 1: Initial Reads and Validation
            lock (AdminManager.BlMutex)
            {
                if (requesterId != courierId)
                    throw new BO.BlInvalidOperationException("Couriers can only select their own orders for handling.");

                if (s_dal.Delivery.ReadAll(d => d.CourierId == courierId && d.DeliveryTypeEnding == null).Any())
                    throw new BO.BlInvalidOperationException("Courier is already handling another order.");

                doCourier = s_dal.Courier.Read(courierId) ?? throw new BO.BlDoesNotExistException($"Courier with ID={courierId} does Not exist");
                companyAddress = s_dal.Config.AddressCompany ?? throw new BO.BlDoesNotExistException($"Config data missing");
            }

            // Step 2: Async Data Fetching (GetOrder / GetCourier handle their own locks or need external handling if accessing DAL)
            // Assuming GetOrder and GetCourier are thread-safe (we fixed them above)
            boOrder = await GetOrder(orderId);
            boCourier = await CourierManager.GetCourier(courierId);

            if (boOrder.OrderStatus != BO.OrderStatus.Open)
                throw new BO.BlInvalidOperationException("Only open orders can be selected for handling.");

            if (await IsCourierCapableToDeliver(boCourier, boOrder) == false)
            {
                throw new BO.BlInvalidOperationException("Courier is not capable to deliver this order due to delivery type or distance.");
            }

            // Step 3: Async Distance Calculation
            double distance = await Tools.CalculateDistance(boOrder.Latitude, boOrder.Longitude, s_dal.Config.Latitude, s_dal.Config.Longitude, boCourier.DeliveryType);

            // Step 4: Create Delivery
            lock (AdminManager.BlMutex)
            {
                DO.Delivery newDoDelivery = new DO.Delivery()
                {
                    Id = 0,
                    OrderId = orderId,
                    CourierId = courierId,
                    DeliveryType = doCourier.CourierDeliveryType,
                    DeliveryDistanceKm = distance,
                    OrderStartDateTime = AdminManager.Now,
                    OrderEndDateTime = null,
                    DeliveryTypeEnding = null,
                };
                s_dal.Delivery.Create(newDoDelivery);
            }

            //// Notifications
            //Observers.NotifyListUpdated(); //orders list
            //CourierManager.Observers.NotifyListUpdated(); //couriers list
            //Observers.NotifyItemUpdated(courierId);  //courier item
            //CourierManager.Observers.NotifyItemUpdated(courierId);
            //await Tools.ConfirmOrderToCourier(boOrder, doCourier, companyAddress);
            Observers.NotifyListUpdated(); //update orders list

            //update couriers list  
            Helpers.CourierManager.Observers.NotifyListUpdated();
            Helpers.CourierManager.Observers.NotifyItemUpdated(courierId);

            await Tools.ConfirmOrderToCourier(boOrder, doCourier, companyAddress);
        }
        catch (BO.BlDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException(ex.Message);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Error selecting order:" + ex.Message);
        }
        catch (BO.BlNullPropertyException ex)
        {
            throw new BO.BlNullPropertyException("Error selecting order for handling:" + ex.Message);
        }
        catch (BlExternalServiceException ex)
        {
            throw new BlExternalServiceException("Error selecting order for handling:" + ex.Message);
        }
        catch (BlInvalidInputException ex)
        {
            throw new BlInvalidInputException("Error selecting order for handling:" + ex.Message);
        }
    }

    /// <summary>
    /// Retrieves a filtered and sorted list of a courier's completed (closed) deliveries.
    /// Access is restricted, requiring the requesterId to match the courierId. It filters deliveries to include only those with a recorded DeliveryTypeEnding.
    /// The list of closed deliveries is optionally filtered by OrderType and then sorted according to the specified sortByProperty or by DeliveryCompletionType by default.
    /// </summary>
    /// <param name="requesterId"></param>
    /// <param name="courierId"></param>
    /// <param name="filterByOrderType"></param>
    /// <param name="sortByProperty"></param>
    /// <returns></returns>
    /// <exception cref="BO.BlInvalidOperationException"></exception>
    /// <exception cref="BlInvalidInputException"></exception>
    internal static IEnumerable<BO.ClosedDeliveryInList> GetClosedDeliveriesList(int requesterId, int courierId, BO.OrderRequirements? filterByOrderType, ClosedDeliveryInListProperties? sortByProperty)
    {
        try
        {
            IEnumerable<BO.ClosedDeliveryInList> allClosedDeliveriesInList;

            lock (AdminManager.BlMutex)
            {
                if (requesterId != courierId)
                    throw new BO.BlInvalidOperationException("Couriers can only access their own closed deliveries.");

                var allDeliveries = s_dal.Delivery.ReadAll(d => d.CourierId == courierId && d.DeliveryTypeEnding != null).ToList();
                // We must materialize to list inside lock

                allClosedDeliveriesInList = allDeliveries.Select(doDelivery => DeliveryManager.GetClosedDeliveryInList(doDelivery.Id)!).ToList();
            }

            // Filter/Sort can happen on local list
            if (filterByOrderType.HasValue)
            {
                allClosedDeliveriesInList = allClosedDeliveriesInList.Where(delivery => delivery.OrderType == filterByOrderType.Value);
            }
            if (sortByProperty != null)
            {
                switch (sortByProperty)
                {
                    case ClosedDeliveryInListProperties.OrderId:
                        allClosedDeliveriesInList = allClosedDeliveriesInList.OrderBy(delivery => delivery.OrderId);
                        break;
                    case ClosedDeliveryInListProperties.OrderType:
                        allClosedDeliveriesInList = allClosedDeliveriesInList.OrderBy(delivery => delivery.OrderType);
                        break;
                    case ClosedDeliveryInListProperties.FullAddress:
                        allClosedDeliveriesInList = allClosedDeliveriesInList.OrderBy(delivery => delivery.FullAddress);
                        break;
                    case ClosedDeliveryInListProperties.DeliveryType:
                        allClosedDeliveriesInList = allClosedDeliveriesInList.OrderBy(delivery => delivery.DeliveryType);
                        break;
                    case ClosedDeliveryInListProperties.DeliveryCompletionType:
                        allClosedDeliveriesInList = allClosedDeliveriesInList.OrderBy(delivery => delivery.DeliveryCompletionType);
                        break;
                    case ClosedDeliveryInListProperties.ActualDistance:
                        allClosedDeliveriesInList = allClosedDeliveriesInList.OrderBy(delivery => delivery.ActualDistance);
                        break;
                    case ClosedDeliveryInListProperties.TotalCompletionTime:
                        allClosedDeliveriesInList = allClosedDeliveriesInList.OrderBy(delivery => delivery.TotalCompletionTime);
                        break;
                    default:
                        throw new BlInvalidInputException("Invalid sort property.");
                }
            }
            else
            {
                allClosedDeliveriesInList = allClosedDeliveriesInList.OrderBy(delivery => delivery.DeliveryCompletionType);
            }
            return allClosedDeliveriesInList;
        }
        catch (BO.BlDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("errore getting closed deliveries" + ex.Message);
        }
        catch (BO.BlNullPropertyException ex)
        {
            throw new BO.BlNullPropertyException("error getting closed deliveries" + ex.Message);
        }
        catch (ArgumentException ex)
        {
            throw new BO.BlNullPropertyException("handaling LINQ error" + ex.Message);
        }
    }

    /// <summary>
    /// Retrieves a filtered and sorted list of orders available for a specific courier to handle.
    /// Access is restricted to the courier themselves, and they must not already be handling an active order.
    /// The list is filtered to include only 'Open' orders that fall within the courier's maximum distance and 
    /// meet specific delivery type and distance requirements (e.g., Frozen, Chilled, Fragile goods restrictions).
    /// </summary>
    /// <param name="requesterId"></param>
    /// <param name="courierId"></param>
    /// <param name="filterByOrderType"></param>
    /// <param name="sortByProperty"></param>
    /// <returns></returns>
    /// <exception cref="BO.BlInvalidOperationException"></exception>
    /// <exception cref="BO.BlDoesNotExistException"></exception>
    /// <exception cref="BlInvalidInputException"></exception>
    internal static async Task<IEnumerable<BO.OpenOrderInList>> GetAvailableOrdersList(int requesterId, int courierId, BO.OrderRequirements? filterByOrderType, OpenOrderInListProperties? sortByProperty)
    {
        try
        {
            DO.Courier doCourier;
            BO.Courier boCourier;
            List<DO.Order> allOrders;
            double? configLat, configLon;

            // Step 1: Read snapshot of data
            lock (AdminManager.BlMutex)
            {
                if (requesterId != courierId)
                    throw new BO.BlInvalidOperationException("Couriers can only access their own available orders.");

                doCourier = s_dal.Courier.Read(courierId) ?? throw new BO.BlDoesNotExistException($"Courier with ID={courierId} does Not exist");
                allOrders = s_dal.Order.ReadAll().ToList(); // Snapshot
                configLat = s_dal.Config.Latitude;
                configLon = s_dal.Config.Longitude;
            }

            boCourier = await CourierManager.GetCourier(courierId);

            // Step 2: Process orders (contains async, so outside lock)
            var tasks = allOrders.Select(async doOrder =>
            {
                // GetOpenOrderInList handles its own lock, it's safe to call.
                var openOrder = GetOpenOrderInList(doOrder.Id);

                if (openOrder == null)
                    return null;

                // Use the local snapshot config values for distance calc to be thread safe without locking inside lambda
                if (boCourier.PersonalMaxAirDistance != null &&
                    CalculateAirDistanceKm(
                        doOrder.Latitude,
                        doOrder.Longitude,
                        configLat,
                        configLon) > boCourier.PersonalMaxAirDistance)
                    return null;

                //double actualDistance = await Tools.CalculateDistance(
                //    doOrder.Latitude,
                //    doOrder.Longitude,
                //    configLat,
                //    configLon,
                //    boCourier.DeliveryType);

                TimeSpan calculatedActualDeliveryTimeSpan =
                    await GetActualDeliveryTimeSpan(doCourier, doOrder);

                return new BO.OpenOrderInList
                {
                    CourierId = courierId,
                    OrderId = openOrder.OrderId,
                    OrderType = openOrder.OrderType,
                    FreeShippingEligibility = openOrder.FreeShippingEligibility,
                    AmountItems = openOrder.AmountItems,
                    FullAddress = openOrder.FullAddress,
                    AirDistance = openOrder.AirDistance,
                    ActualDistance = openOrder.AirDistance,
                    ActualDeliveryTimeSpan = calculatedActualDeliveryTimeSpan,
                    ScheduleStatus = openOrder.ScheduleStatus,
                    TimeToFinish = openOrder.TimeToFinish,
                    MaximumDeliveryTime = openOrder.MaximumDeliveryTime
                };
            });

            // wait for all tasks to complete and filter out null results
            IEnumerable<BO.OpenOrderInList> allOpenOrdersInList =
                (await Task.WhenAll(tasks))
                .Where(order => order != null)!;

            if (filterByOrderType.HasValue)
            {
                allOpenOrdersInList = allOpenOrdersInList.Where(order => order.OrderType == filterByOrderType.Value);
            }

            // Additional matching rules
            allOpenOrdersInList = allOpenOrdersInList.Where(order =>
            {
                if (order.OrderType == BO.OrderRequirements.Frozen)
                {
                    if (order.ActualDistance <= 5) return true;
                    return boCourier.DeliveryType == BO.DeliveryTypeMethods.Car || boCourier.DeliveryType == BO.DeliveryTypeMethods.Motorcycle;
                }
                if (order.OrderType == BO.OrderRequirements.Chilled)
                {
                    if (boCourier.DeliveryType == BO.DeliveryTypeMethods.Car || boCourier.DeliveryType == BO.DeliveryTypeMethods.Motorcycle) return true;
                    return order.ActualDistance <= 3;
                }
                if (order.OrderType == BO.OrderRequirements.Dry) return true;
                if (order.OrderType == BO.OrderRequirements.Fragile)
                {
                    if (boCourier.DeliveryType == BO.DeliveryTypeMethods.Car || boCourier.DeliveryType == BO.DeliveryTypeMethods.Motorcycle) return true;
                    return order.ActualDistance <= 2;
                }
                if (order.OrderType == BO.OrderRequirements.Mixed)
                {
                    if (boCourier.DeliveryType == BO.DeliveryTypeMethods.Car || boCourier.DeliveryType == BO.DeliveryTypeMethods.Motorcycle) return true;
                    return order.ActualDistance <= 4;
                }
                return true;
            });

            if (sortByProperty != null)
            {
                switch (sortByProperty)
                {
                    case OpenOrderInListProperties.CourierId:
                        allOpenOrdersInList = allOpenOrdersInList.OrderBy(order => order.CourierId);
                        break;
                    case OpenOrderInListProperties.OrderId:
                        allOpenOrdersInList = allOpenOrdersInList.OrderBy(order => order.OrderId);
                        break;
                    // ... (Keeping rest of sort logic) ...
                    case OpenOrderInListProperties.OrderType:
                        allOpenOrdersInList = allOpenOrdersInList.OrderBy(order => order.OrderType);
                        break;
                    case OpenOrderInListProperties.FreeShippingEligibility:
                        allOpenOrdersInList = allOpenOrdersInList.OrderBy(order => order.FreeShippingEligibility);
                        break;
                    case OpenOrderInListProperties.AmountItems:
                        allOpenOrdersInList = allOpenOrdersInList.OrderBy(order => order.AmountItems);
                        break;
                    case OpenOrderInListProperties.FullAddress:
                        allOpenOrdersInList = allOpenOrdersInList.OrderBy(order => order.FullAddress);
                        break;
                    case OpenOrderInListProperties.AirDistance:
                        allOpenOrdersInList = allOpenOrdersInList.OrderBy(order => order.AirDistance);
                        break;
                    case OpenOrderInListProperties.ActualDistance:
                        allOpenOrdersInList = allOpenOrdersInList.OrderBy(order => order.ActualDistance);
                        break;
                    case OpenOrderInListProperties.ActualDeliveryTimeSpan:
                        allOpenOrdersInList = allOpenOrdersInList.OrderBy(order => order.ActualDeliveryTimeSpan);
                        break;
                    case OpenOrderInListProperties.ScheduleStatus:
                        allOpenOrdersInList = allOpenOrdersInList.OrderBy(order => order.ScheduleStatus);
                        break;
                    case OpenOrderInListProperties.TimeToFinish:
                        allOpenOrdersInList = allOpenOrdersInList.OrderBy(order => order.TimeToFinish);
                        break;
                    case OpenOrderInListProperties.MaximumDeliveryTime:
                        allOpenOrdersInList = allOpenOrdersInList.OrderBy(order => order.MaximumDeliveryTime);
                        break;
                    default:
                        throw new BlInvalidInputException("Invalid sort property.");
                }
            }
            else
            {
                allOpenOrdersInList = allOpenOrdersInList.OrderBy(order => order.ScheduleStatus);
            }
            return allOpenOrdersInList;
        }
        catch (BO.BlDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("error getting available orders" + ex.Message);
        }
        catch (BO.BlNullPropertyException ex)
        {
            throw new BO.BlNullPropertyException("error getting available orders" + ex.Message);
        }
        catch (ArgumentException ex)
        {
            throw new BO.BlNullPropertyException("handaling LINQ error" + ex.Message);
        }
    }
    /// <summary>
    /// Determines if a specific courier is physically and logistically capable of delivering a particular order based on the order's distance, requirements (e.g., Frozen), and the courier's transportation type.
    /// The check first enforces the courier's personal maximum air distance limit. It then applies detailed rules for OrderType compatibility, often involving distance restrictions for slower or less-equipped delivery types (Bike/Foot).
    /// </summary>
    /// <param name="doCourier">The Data Object representation of the Courier.</param>
    /// <param name="doOrder">The Data Object representation of the Order.</param>
    /// <returns>True if the courier can perform the order, otherwise False.</returns>
    internal static async Task<bool> IsCourierCapableToDeliver(BO.Courier boCourier, BO.Order boOrder)
    {
        try
        {
            double? configLat, configLon;
            lock (AdminManager.BlMutex)
            {
                configLat = s_dal.Config.Latitude;
                configLon = s_dal.Config.Longitude;
            }

            double actualDistance = await Tools.CalculateDistance(
                boOrder.Latitude,
                boOrder.Longitude,
                configLat,
                configLon,
                boCourier.DeliveryType);

            if (boCourier.PersonalMaxAirDistance != null && boOrder.AirDistance > boCourier.PersonalMaxAirDistance)
                return false;

            return boOrder.OrderType switch
            {
                BO.OrderRequirements.Frozen =>
                    actualDistance <= 5 ||
                    boCourier.DeliveryType is BO.DeliveryTypeMethods.Car or BO.DeliveryTypeMethods.Motorcycle,

                BO.OrderRequirements.Chilled =>
                    boCourier.DeliveryType is BO.DeliveryTypeMethods.Car or BO.DeliveryTypeMethods.Motorcycle ||
                    actualDistance <= 3,

                BO.OrderRequirements.Dry => true,

                BO.OrderRequirements.Fragile =>
                     boCourier.DeliveryType is BO.DeliveryTypeMethods.Car or BO.DeliveryTypeMethods.Motorcycle ||
                    actualDistance <= 2,

                BO.OrderRequirements.Mixed =>
                     boCourier.DeliveryType is BO.DeliveryTypeMethods.Car or BO.DeliveryTypeMethods.Motorcycle || actualDistance <= 4,

                _ => true,
            };

        }
        catch (BO.BlNullPropertyException ex)
        {
            throw new BO.BlNullPropertyException("Error checking courier capability to deliver order: " + ex.Message);
        }
        catch (BlExternalServiceException ex)
        {
            throw new BlExternalServiceException("Error checking courier capability to deliver order:" + ex.Message);
        }
        catch (BlInvalidInputException ex)
        {
            throw new BlInvalidInputException("Error checking courier capability to deliver order:" + ex.Message);
        }
    }
}
