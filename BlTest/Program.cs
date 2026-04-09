using BLApi;
using BO;
using System;
using System.Linq; // Added for parsing Enum values

namespace BlTest;

/// <summary>
/// The main program class for testing the Business Logic layer (BL).
/// </summary>
internal class Program
{
    // Static field to hold the BL implementation instance, initialized via Factory.
    // It's declared as readonly because it should be initialized only once.
    static readonly IBl s_bl = Factory.Get();

    // --- Main Program Entry Point ---
    /// <summary>
    /// main program
    /// </summary>
    /// <param name="args"></param>
    static void Main(string[] args)
    {
        Console.WriteLine("--- BL Test Program Started ---");

        int choice;
        do
        {
            // Display the main menu
            Console.WriteLine("\n--- Main Menu ---");
            Console.WriteLine("1. Order Management (IOrder)");
            Console.WriteLine("2. Courier Management (ICourier)");
            Console.WriteLine("3. Admin Operations (IAdmin)");
            Console.WriteLine("0. Exit");
            Console.Write("Enter your choice: ");

            // Input validation using TryParse
            if (!int.TryParse(Console.ReadLine(), out choice))
            {
                Console.WriteLine("Invalid input. Please enter a number.");
                continue;
            }

            // Handle the user's main menu choice
            switch (choice)
            {
                case 1:
                    OrderMenu(); // Handle IOrder operations
                    break;
                case 2:
                    CourierMenu(); // Handle ICourier operations
                    break;
                case 3:
                    AdminMenu(); // Handle IAdmin operations
                    break;
                case 0:
                    Console.WriteLine("Exiting BL Test Program. Goodbye!");
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please select from the menu options.");
                    break;
            }
        } while (choice != 0);
    }

    // ----------------------------------------------------------------------------------
    // --- Helper Methods for Input and Exception Handling ---
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// Reads an integer from the console, validates it, and returns the value.
    /// </summary>
    /// <param name="prompt">The message to display to the user.</param>
    /// <returns>The parsed integer value.</returns>
    /// <exception cref="BlInvalidInputException">Thrown if input is not a valid integer.</exception>
    private static int ReadIntInput(string message)
    {
        Console.Write(message);
        if (int.TryParse(Console.ReadLine(), out int value))
        {
            return value;
        }
        throw new BlInvalidInputException("Invalid input. Please enter a valid integer.");
    }

    private static TimeSpan ReadTimeSpanInput(string message)
    {
        Console.Write(message);
        if (TimeSpan.TryParse(Console.ReadLine(), out TimeSpan value))
            return value;

        throw new BlInvalidInputException("Invalid TimeSpan format (expected hh:mm:ss).");
    }


    private static bool ReadBoolInput(string message)
    {
        Console.Write(message);

        if (bool.TryParse(Console.ReadLine(), out bool value))
        {
            return value;
        }

        throw new BlInvalidInputException("Invalid input. Please enter 'true' or 'false'.");
    }

    private static double ReadDoubleInput(string message)
    {
        Console.Write(message);
        if (double.TryParse(Console.ReadLine(), out double value))
            return value;

        throw new BlInvalidInputException("Invalid input. Please enter a valid DOUBLE.");
    }


    private static double? ReadNullableDoubleInput(string message)
    {
        Console.Write(message);
        string? valueStr = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(valueStr))
        {
            return null; // Return null if input is empty
        }
        if (double.TryParse(valueStr, out double value))
        {
            return value;
        }
        throw new BlInvalidInputException("Invalid input. Please enter a valid double.");
    }

    /// <summary>
    /// Reads a string from the console.
    /// </summary>
    /// <param name="prompt">The message to display to the user.</param>
    /// <returns>The read string from user.</returns>
    private static string ReadStringInput(string message)
    {
        Console.Write(message);
        return Console.ReadLine();
    }

    private static string? ReadStringNullableInput(string message)
    {
        Console.Write(message);
        string? str= Console.ReadLine();
        if(string.IsNullOrWhiteSpace(str))
            return null;
        return str;
    }

    /// <summary>
    /// Reads a DateTime from the console, validates it, and returns the value.
    /// </summary>
    /// <param name="prompt">The message to display to the user.</param>
    /// <returns>The parsed DateTime value.</returns>
    /// <exception cref="BlInvalidInputException">Thrown if input is not a valid date/time.</exception>
    private static DateTime ReadDateTimeInput(string message)
    {
        Console.Write(message);
        if (DateTime.TryParse(Console.ReadLine(), out DateTime value))
        {
            return value;
        }
        throw new BlInvalidInputException("Invalid input. Please enter a valid date/time.");
    }

    /// <summary>
    /// A generic method to read and validate an Enum value from the console.
    /// </summary>
    /// <typeparam name="T">The Enum type.</typeparam>
    /// <param name="prompt">The message to display to the user.</param>
    /// <returns>The parsed Enum value.</returns>
    /// <exception cref="BlInvalidInputException">Thrown if input is not a valid Enum member.</exception>
    private static T ReadEnumInput<T>(string strEnum) where T : struct, Enum  //To check 
    {
        Console.Write($"{strEnum} ({string.Join(", ", Enum.GetNames(typeof(T)))}): ");
        string input = Console.ReadLine();

        if (Enum.TryParse(input, true, out T value))
        {
            return value;
        }

        // Also try parsing by number
        if (int.TryParse(input, out int intValue) && Enum.IsDefined(typeof(T), intValue))
        {
            return (T)Enum.ToObject(typeof(T), intValue);
        }

        throw new BlInvalidInputException($"Invalid input. Please enter a valid value for {typeof(T).Name}.");
    }
    private static T? ReadNullAblleEnumInput<T>(string strEnum)
    where T : struct ,Enum
    {
        Console.Write($"{strEnum} ({string.Join(", ", Enum.GetNames(typeof(T)))}): ");
        string? input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
            return null;

        if (Enum.TryParse(input, true, out T value))
            return value;

        // Parse by number
        if (int.TryParse(input, out int intValue) && Enum.IsDefined(typeof(T), intValue))
            return (T)Enum.ToObject(typeof(T), intValue);

        return null;
    }


    /// <summary>
    /// Handles the display of caught BO exceptions.
    /// </summary>
    /// <param name="ex">The exception to display.</param>
    private static void HandleBlException(Exception ex)
    {
        Console.WriteLine($"\n--- Caught BL Exception ---");
        Console.WriteLine($"Exception Type: {ex.GetType().Name}");
        Console.WriteLine($"Message: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner Exception: {ex.InnerException.GetType().Name}");
            Console.WriteLine($"Inner Message: {ex.InnerException.Message}");
        }
        Console.WriteLine("---------------------------");
    }

    /// <summary>
    /// Prints an enumerable collection of objects using a foreach loop and Console.WriteLine.
    /// </summary>
    /// <typeparam name="T">The type of the objects in the collection.</typeparam>
    /// <param name="collection">The collection to print.</param>
    private static void PrintCollection<T>(IEnumerable<T> collection)
    {
        if (collection == null || !collection.Any())
        {
            Console.WriteLine("Collection is empty or null.");
            return;
        }

        Console.WriteLine($"\n--- {typeof(T).Name} Collection ({collection.Count()} items) ---");
        foreach (var item in collection)
        {
            // Console.WriteLine calls the ToString() method implicitly
            Console.WriteLine(item);
            Console.WriteLine("---");
        }
    }

    // ----------------------------------------------------------------------------------
    // --- IOrder Menu and Operation Handlers ---
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// Manages the IOrder sub-menu and dispatches actions.
    /// </summary>
    private static void OrderMenu()
    {
        int choice;
        do
        {
            Console.WriteLine("\n--- Order Sub-Menu (IOrder) ---");
            Console.WriteLine("1. Create Order");
            Console.WriteLine("2. Read Order");
            Console.WriteLine("3. Update Order");
            Console.WriteLine("4. Delete Order");
            Console.WriteLine("5. Read All Orders");
            Console.WriteLine("6. Select Order For Handling");
            Console.WriteLine("7. Finish Delivery Handling");
            Console.WriteLine("8. Cancel Order");
            Console.WriteLine("9. Get Available Orders For Courier");
            Console.WriteLine("10. Get Closed Deliveries By Courier");
            Console.WriteLine("11. Get Order Summary");
            Console.WriteLine("0. Back to Main Menu");
            Console.Write("Enter your choice: ");

            if (!int.TryParse(Console.ReadLine(), out choice))
            {
                Console.WriteLine("Invalid input. Please enter a number.");
                continue;
            }

            try
            {
                switch (choice)
                {
                    case 1: CreateOrder(); break;
                    case 2: ReadOrder(); break;
                    case 3: UpdateOrder(); break;
                    case 4: DeleteOrder(); break;
                    case 5: ReadAllOrders(); break;
                    case 6: SelectOrderForHandling(); break;
                    case 7: FinishDeliveryHandling(); break;
                    case 8: CancelOrder(); break;
                    case 9: GetAvailableOrdersForCourier(); break;
                    case 10: GetClosedDeliveriesByCourier(); break;
                    case 11: GetOrderSummary(); break;
                    case 0: break;
                    default: Console.WriteLine("Invalid choice."); break;
                }
            }
            catch (Exception ex) when (ex is BlDoesNotExistException || ex is BlAlreadyExistsException ||
                                       ex is BlFailedToGenerate || ex is BlXMLFileLoadCreateException ||
                                       ex is BlNullPropertyException || ex is BlInvalidInputException ||
                                       ex is BlInvalidOperationException || ex is BlExternalServiceException)
            {
                HandleBlException(ex);
            }
        } while (choice != 0);
    }

    private static void CreateOrder()
    {
        Console.WriteLine("\n--- Create New Order ---");

        // Collect essential Order fields

        // ID is collected from the user, though often generated by the system (DAL/BL).
        //int id = ReadIntInput("Enter New Order ID: ");
        int requesterId = ReadIntInput("Enter Requester ID: "); // Used for BL permission checks

        // Customer Details
        string customerFullName = ReadStringInput("Enter Customer Full Name: ");
        string customerPhone = ReadStringInput("Enter Customer Phone: ");

        // Order Classification
        OrderRequirements orderType = ReadEnumInput<OrderRequirements>("Enter Order Type: ");

        // Location and Description
        string ? shortOrderDescription = ReadStringNullableInput("Enter Short Order Description (Optional): ");
        string fullAddress = ReadStringInput("Enter Full Order Address:(Format: Street, City, Country) "); // Critical for geo-calculation in BL

        // Item Attribute
        int amountItems = ReadIntInput("Enter Amount of Items: ");

        // --- Create the Business Object (BO) ---

        BO.Order newOrder = new BO.Order()
        {
            CustomerFullName = customerFullName,
            CustomerPhone = customerPhone,
            OrderType = orderType,
            ShortOrderDescription = shortOrderDescription,
            FullAddress = fullAddress,
            AmountItems = amountItems,
        };

        s_bl.Order.Create(newOrder, requesterId);
        Console.WriteLine("Order created successfully.");
    }

    private static async void ReadOrder()
    {
        Console.WriteLine("\n--- Read Order ---");
        int requesterId = ReadIntInput("Enter Requester ID: ");
        int orderId = ReadIntInput("Enter Order ID to Read: ");

        Order order = await s_bl.Order.Read(requesterId, orderId);
        Console.WriteLine("\n--- Retrieved Order ---");
        Console.WriteLine(order);
    }

    private static async void UpdateOrder()
    {
        Console.WriteLine("\n--- Update Order ---");

        int requesterId = ReadIntInput("Enter Requester ID: ");
        int orderId = ReadIntInput("Enter Order ID to Update: ");

  
        Order existing = await s_bl.Order.Read(requesterId, orderId);

        Console.WriteLine("\nCurrent Order:");
        Console.WriteLine(existing);

        bool done = false;

        while (!done)
        {
            Console.WriteLine("\nSelect field to update:");
            Console.WriteLine("1 - Customer Full Name");
            Console.WriteLine("2 - Customer Phone");
            Console.WriteLine("3 - Order Type");
            Console.WriteLine("4 - Short Order Description");
            Console.WriteLine("5 - Full Address");
            Console.WriteLine("6 - Amount of Items");
            Console.WriteLine("0 - Finish");

            int choice = ReadIntInput("Your choice: ");

            switch (choice)
            {
                case 1:
                    existing.CustomerFullName = ReadStringInput("Enter NEW Customer Full Name: ");
                    break;

                case 2:
                    existing.CustomerPhone = ReadStringInput("Enter NEW Customer Phone: ");
                    break;

                case 3:
                    existing.OrderType = ReadEnumInput<OrderRequirements>("Enter NEW Order Type: ");
                    break;

                case 4:
                    existing.ShortOrderDescription = ReadStringInput("Enter NEW Short Description: ");
                    break;

                case 5:
                    existing.FullAddress = ReadStringInput("Enter NEW Full Address: ");
                    break;

                case 6:
                    existing.AmountItems = ReadIntInput("Enter NEW Amount of Items: ");
                    break;

                case 0:
                    done = true;
                    break;

                default:
                    Console.WriteLine("Invalid option. Try again.");
                    break;
            }
        }
        BO.Order updatedOrder = new BO.Order()
        {
            Id = existing.Id,
            CustomerFullName = existing.CustomerFullName,
            CustomerPhone = existing.CustomerPhone,
            OrderType = existing.OrderType,
            ShortOrderDescription = existing.ShortOrderDescription,
            FullAddress = existing.FullAddress,
            AmountItems = existing.AmountItems,
            OrderOpeningTime = existing.OrderOpeningTime,
        };
        s_bl.Order.Update(updatedOrder, requesterId);

        Console.WriteLine("Order updated successfully.");
    }

    private static void DeleteOrder()
    {
        Console.WriteLine("\n--- Delete Order ---");
        int requesterId = ReadIntInput("Enter Requester ID: ");
        int orderId = ReadIntInput("Enter Order ID to Delete: ");

        s_bl.Order.Delete(orderId, requesterId);
        Console.WriteLine($"Order {orderId} deleted successfully.");
    }

    private static void ReadAllOrders()
    {
        Console.WriteLine("\n--- Read All Orders ---");
        int requesterId = ReadIntInput("Enter Requester ID: ");

        // For simplicity, we skip filtering and sorting inputs for now
        //IEnumerable<OrderInList> orders = s_bl.Order.ReadAll(requesterId, null, null, null);

        // Example of using filter and sort (requires BO enums to be defined)
        OrderRequirements? filterBy = ReadNullAblleEnumInput<OrderRequirements>("Enter Filter Property (or press Enter): ");
          //  ReadEnumInput<OrderInListProperties>("Enter Filter Property (or press Enter): ");
        object? filterValue = null;
        if (filterBy != null)
        {
            Console.Write($"Enter filter value for {filterBy.ToString()}: ");
            filterValue = Console.ReadLine();
        }

        OrderInListProperties? sortBy = ReadNullAblleEnumInput<OrderInListProperties>("Enter Sort Property (or press Enter): ");

        IEnumerable<OrderInList> orders = s_bl.Order.ReadAll(requesterId, null, filterBy, null,  sortBy);

        PrintCollection(orders);
    }

    private static async void SelectOrderForHandling()
    {
        Console.WriteLine("\n--- Select Order For Handling ---");
        int requesterId = ReadIntInput("Enter Requester ID: ");
        int courierId = ReadIntInput("Enter Courier ID: ");
        Console.WriteLine("\n--- Select Order From the list: ");
        IEnumerable<OpenOrderInList> orders = await s_bl.Order.GetAvailableOrdersForCourier(requesterId, courierId, null, null);
        PrintCollection(orders);
        int orderId = ReadIntInput("Enter Order ID to Select: ");
        s_bl.Order.SelectOrderForHandling(requesterId, courierId, orderId);
        Console.WriteLine($"Order {orderId} selected for handling by courier {courierId} successfully.");
    }

    private static void FinishDeliveryHandling()
    {
        Console.WriteLine("\n--- Finish Delivery Handling ---");
        int requesterId = ReadIntInput("Enter Requester ID: ");
        int courierId = ReadIntInput("Enter Courier ID: ");
        int deliveryId = ReadIntInput("Enter Delivery ID to Finish: ");
        BO.DeliveryCompletionType completionType = ReadEnumInput<BO.DeliveryCompletionType>("Enter Delivery Completion Type: ");

        s_bl.Order.FinishDeliveryHandling(requesterId, courierId, deliveryId, completionType);
        Console.WriteLine($"Delivery {deliveryId} finished successfully.");
    }

    private static void CancelOrder()
    {
        Console.WriteLine("\n--- Cancel Order ---");
        int requesterId = ReadIntInput("Enter Requester ID: ");
        int orderId = ReadIntInput("Enter Order ID to Cancel: ");

        s_bl.Order.CancelOrder(orderId, requesterId);
        Console.WriteLine($"Order {orderId} cancelled successfully.");
    }

    private static async void GetAvailableOrdersForCourier()
    {
        Console.WriteLine("\n--- Get Available Orders For Courier ---");
        int requesterId = ReadIntInput("Enter Requester ID: ");
        int courierId = ReadIntInput("Enter Courier ID: ");

        OrderRequirements? filterByOrderType = ReadNullAblleEnumInput<OrderRequirements>("Enter Filter by Order Type (or press Enter): ");
        OpenOrderInListProperties? sortByProperty = ReadNullAblleEnumInput<OpenOrderInListProperties>("Enter Sort Property (or press Enter): ");

        IEnumerable<OpenOrderInList> orders = await s_bl.Order.GetAvailableOrdersForCourier(requesterId, courierId, filterByOrderType, sortByProperty);
        PrintCollection(orders);
    }

    private static void GetClosedDeliveriesByCourier()
    {
        Console.WriteLine("\n--- Get Closed Deliveries By Courier ---");
        int requesterId = ReadIntInput("Enter Requester ID: ");
        int courierId = ReadIntInput("Enter Courier ID: ");

        OrderRequirements? filterByOrderType = ReadNullAblleEnumInput<OrderRequirements>("Enter Filter by Order Type (or press Enter): ");
        ClosedDeliveryInListProperties? sortByProperty = ReadNullAblleEnumInput<ClosedDeliveryInListProperties>("Enter Sort Property (or press Enter): ");

        IEnumerable<ClosedDeliveryInList> deliveries = s_bl.Order.GetClosedDeliveriesByCourier(requesterId, courierId, filterByOrderType, sortByProperty);
        PrintCollection(deliveries);
    }
        private static void GetOrderSummary()
    {
        Console.WriteLine("\n--- Get Order Summary ---");
        int requesterId = ReadIntInput("Enter Requester ID: ");

        int[] summary = s_bl.Order.GetOrderSummary(requesterId);

        var orderStatuses = (BO.OrderStatus[])Enum.GetValues(typeof(BO.OrderStatus));
        var scheduleStatuses = (BO.ScheduleStatus[])Enum.GetValues(typeof(BO.ScheduleStatus));

        int scheduleCount = scheduleStatuses.Length;
        int index = 0;

        Console.WriteLine("\n--- Order Summary (Status × Timing) ---");

        foreach (var orderStatus in orderStatuses)
        {
            foreach (var scheduleStatus in scheduleStatuses)
            {
                Console.WriteLine(
                    $"Index {index}: {orderStatus} and {scheduleStatus} : {summary[index]} orders"
                );

                index++;
            }
        }
    }
    // ----------------------------------------------------------------------------------
    // --- ICourier Menu and Operation Handlers ---
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// Manages the ICourier sub-menu and dispatches actions.
    /// </summary>
    private static void CourierMenu()
    {
        int choice;
        do
        {
            Console.WriteLine("\n--- Courier Sub-Menu (ICourier) ---");
            Console.WriteLine("1. Create Courier");
            Console.WriteLine("2. Read Courier");
            Console.WriteLine("3. Update Courier");
            Console.WriteLine("4. Delete Courier");
            Console.WriteLine("5. Read All Couriers");
            Console.WriteLine("6. Enter System (Login)");
            Console.WriteLine("0. Back to Main Menu");
            Console.Write("Enter your choice: ");

            if (!int.TryParse(Console.ReadLine(), out choice))
            {
                Console.WriteLine("Invalid input. Please enter a number.");
                continue;
            }

            try
            {
                switch (choice)
                {
                    case 1: CreateCourier(); break;
                    case 2: ReadCourier(); break;
                    case 3: UpdateCourier(); break;
                    case 4: DeleteCourier(); break;
                    case 5: ReadAllCouriers(); break;
                    case 6: EnterSystemCourier(); break;
                    case 0: break;
                    default: Console.WriteLine("Invalid choice."); break;
                }
            }
            catch (Exception ex) when (ex is BlDoesNotExistException || ex is BlAlreadyExistsException ||
                                       ex is BlFailedToGenerate || ex is BlXMLFileLoadCreateException ||
                                       ex is BlNullPropertyException || ex is BlInvalidInputException ||
                                       ex is BlInvalidOperationException || ex is BlExternalServiceException)
            {
                HandleBlException(ex);
            }
        } while (choice != 0);
    }

    private static void CreateCourier()
    {
        Console.WriteLine("\n--- Create Courier ---");
        int requesterId = ReadIntInput("Enter Requester ID: ");
        int id = ReadIntInput("Enter New Courier ID: ");
        string name = ReadStringInput("Enter Courier Name: ");
        string phone = ReadStringInput("Enter Courier Phone: ");
        string password = ReadStringInput("Enter Initial Password: ");
        string Email = ReadStringInput("Enter Initial  Email: ");
        double? PersonalMaxAirDistance = ReadNullableDoubleInput("Enter PersonalMaxAirDistance or press enter: ");
        DeliveryTypeMethods deliveryType = ReadEnumInput<DeliveryTypeMethods>("Enter delivery Type method: ");
        // Assuming other properties like Active and BaseStationLocation are also required
        bool active = true; // Default to active

        // Create the BO.Courier object
        BO.Courier newCourier = new BO.Courier()
        {
            Id = id,
            NameCourier = name,
            PhoneNumber = phone,
            EmailCourier = Email,
            PasswordCourier = password,
            IsActive = active,
            PersonalMaxAirDistance = PersonalMaxAirDistance,
            DeliveryType = deliveryType,

            // Initialize other properties if necessary
        };

        s_bl.Courier.Create(newCourier, requesterId);
        Console.WriteLine("Courier created successfully.");
    }

    private static async void ReadCourier()
    {
        Console.WriteLine("\n--- Read Courier ---");
        int requesterId = ReadIntInput("Enter Requester ID: ");
        int courierId = ReadIntInput("Enter Courier ID to Read: ");

        Courier courier = await s_bl.Courier.Read(requesterId, courierId);
        Console.WriteLine("\n--- Retrieved Courier ---");
        Console.WriteLine(courier);
    }

    private static async void UpdateCourier()
    {
        Console.WriteLine("\n--- Update Courier ---");

        int requesterId = ReadIntInput("Enter Requester ID: ");
        int courierId = ReadIntInput("Enter Courier ID to Update: ");

        // Load existing courier
        BO.Courier existing = await s_bl.Courier.Read(requesterId, courierId);

        Console.WriteLine("\nCurrent Courier Details:");
        Console.WriteLine(existing);

        bool done = false;

        while (!done)
        {
            Console.WriteLine("\nSelect field to update:");
            Console.WriteLine("1 - Full Name");
            Console.WriteLine("2 - Phone Number");
            Console.WriteLine("3 - Email");
            Console.WriteLine("4 - Password");
            Console.WriteLine("5 - Active Status   (Manager ONLY)");
            Console.WriteLine("6 - Personal Max Air Distance");
            Console.WriteLine("7 - Delivery Type");
            Console.WriteLine("0 - Finish");

            int choice = ReadIntInput("Your choice: ");

            switch (choice)
            {
                case 1:
                    existing.NameCourier = ReadStringInput("Enter NEW Full Name: ");
                    break;

                case 2:
                    existing.PhoneNumber = ReadStringInput("Enter NEW Phone Number: ");
                    break;

                case 3:
                    existing.EmailCourier = ReadStringInput("Enter NEW Email: ");
                    break;

                case 4:
                    existing.PasswordCourier = ReadStringInput("Enter NEW Password: ");
                    break;

                case 5:
                    // Manager check occurs in BL anyway; UI prompts normally.
                    existing.IsActive = ReadBoolInput("Is courier active? (true/false): ");
                    break;

                case 6:
                    existing.PersonalMaxAirDistance =
                        ReadNullableDoubleInput("Enter NEW Max Air Distance (or empty for NULL): ");
                    break;

                case 7:
                    existing.DeliveryType =
                        ReadEnumInput<DeliveryTypeMethods>("Enter NEW Delivery Type: ");
                    break;
                case 0:
                    done = true;
                    break;

                default:
                    Console.WriteLine("Invalid option. Try again.");
                    break;
            }
        }

        // Build new BO object for update
        BO.Courier updatedCourier = new BO.Courier
        {
            Id = existing.Id,
            NameCourier = existing.NameCourier,
            PhoneNumber = existing.PhoneNumber,
            EmailCourier = existing.EmailCourier,
            PasswordCourier = existing.PasswordCourier,
            IsActive = existing.IsActive,
            PersonalMaxAirDistance = existing.PersonalMaxAirDistance,
            DeliveryType = existing.DeliveryType,
            SalaryForCourier = existing.SalaryForCourier,

            // Read-only properties preserved as-is
            StartDate = existing.StartDate,
            TotalDeliveredOnTime = existing.TotalDeliveredOnTime,
            TotalLateDeliveries = existing.TotalLateDeliveries,
            OrderInProgress = existing.OrderInProgress
        };

        s_bl.Courier.Update(updatedCourier, requesterId);
        Console.WriteLine("Courier updated successfully.");
    }

    private static void DeleteCourier()
    {
        Console.WriteLine("\n--- Delete Courier ---");
        int requesterId = ReadIntInput("Enter Requester ID: ");
        int courierId = ReadIntInput("Enter Courier ID to Delete: ");

        s_bl.Courier.Delete(courierId, requesterId);
        Console.WriteLine($"Courier {courierId} deleted successfully.");
    }

    private static void ReadAllCouriers()
    {
        Console.WriteLine("\n--- Read All Couriers ---");
        int requesterId = ReadIntInput("Enter Requester ID: ");

        // For simplicity, we skip filtering and sorting inputs for now
        IEnumerable<CourierInList> couriers = s_bl.Courier.ReadAll(requesterId, null, null, null,null);
        PrintCollection(couriers);
    }

    private static void EnterSystemCourier()
    {
        Console.WriteLine("\n--- Enter System (Login) ---");
        int id = ReadIntInput("Enter your ID: ");
        string password = ReadStringInput("Enter your Password: ");

        string token = s_bl.Courier.EnterSystem(id, password);
        Console.WriteLine($"Login successful. your role: {token}");
    }


    // ----------------------------------------------------------------------------------
    // --- IAdmin Menu and Operation Handlers ---
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// Manages the IAdmin sub-menu and dispatches actions.
    /// </summary>
    private static void AdminMenu()
    {
        int choice;
        do
        {
            Console.WriteLine("\n--- Admin Sub-Menu (IAdmin) ---");
            Console.WriteLine("1. Get Current Clock");
            Console.WriteLine("2. Forward Clock");
            Console.WriteLine("3. Get Configuration");
            Console.WriteLine("4. Set Configuration");
            Console.WriteLine("5. Initialize Database");
            Console.WriteLine("6. Reset Database");
            Console.WriteLine("0. Back to Main Menu");
            Console.Write("Enter your choice: ");

            if (!int.TryParse(Console.ReadLine(), out choice))
            {
                Console.WriteLine("Invalid input. Please enter a number.");
                continue;
            }

            try
            {
                switch (choice)
                {
                    case 1: GetClock(); break;
                    case 2: ForwardClock(); break;
                    case 3: GetConfig(); break;
                    case 4: SetConfig(); break;
                    case 5: InitializeDB(); break;
                    case 6: ResetDB(); break;
                    case 0: break;
                    default: Console.WriteLine("Invalid choice."); break;
                }
            }
            catch (Exception ex) when (ex is BlDoesNotExistException || ex is BlAlreadyExistsException ||
                                       ex is BlFailedToGenerate || ex is BlXMLFileLoadCreateException ||
                                       ex is BlNullPropertyException || ex is BlInvalidInputException ||
                                       ex is BlInvalidOperationException || ex is BlExternalServiceException)
            {
                HandleBlException(ex);
            }
        } while (choice != 0);
    }

    private static void GetClock()
    {
        Console.WriteLine("\n--- Get Current Clock ---");
        DateTime clock = s_bl.Admin.GetClock();
        Console.WriteLine($"Current System Clock: {clock}");
    }

    private static void ForwardClock()
    {
        Console.WriteLine("\n--- Forward Clock ---");
        TimeUnit timeUnit = ReadEnumInput<TimeUnit>("Enter Time Unit to Forward by");
        s_bl.Admin.ForwardClock(timeUnit);
        Console.WriteLine($"Clock forwarded by one {timeUnit} Current Time: {s_bl.Admin.GetClock()}");
    }

    private static void GetConfig()
    {
        Console.WriteLine("\n--- Get Configuration ---");
        Config config = s_bl.Admin.GetConfig();
        Console.WriteLine("\n--- System Configuration ---");
        Console.WriteLine(config);
    }

    private static void SetConfig()
    {
        Console.WriteLine("\n--- Set Configuration ---");

        // Load current configuration
        BO.Config currentConfig = s_bl.Admin.GetConfig();

        Console.WriteLine("\nCurrent Config:");
        Console.WriteLine(currentConfig);

        bool done = false;

        while (!done)
        {
            Console.WriteLine("\nSelect field to update:");
            Console.WriteLine("1  - Clock");
            Console.WriteLine("2  - Manager ID");
            Console.WriteLine("3  - Manager Password");
            Console.WriteLine("4  - Company Address");
            Console.WriteLine("5  - Latitude");
            Console.WriteLine("6  - Longitude");
            Console.WriteLine("7  - Max Air Delivery Distance (Km)");
            Console.WriteLine("8  - Avg Motorcycle Speed (Kmh)");
            Console.WriteLine("9  - Avg Bicycle Speed (Kmh)");
            Console.WriteLine("10 - Avg Car Speed (Kmh)");
            Console.WriteLine("11 - Avg Walking Speed (Kmh)");
            Console.WriteLine("12 - Max Delivery Time Range");
            Console.WriteLine("13 - Risk Time Range");
            Console.WriteLine("14 - Inactivity Time Range");
            Console.WriteLine("15 - Base Salary Monthly");
            Console.WriteLine("16 - Delivery Rate Per Km");
            Console.WriteLine("0  - Finish");

            int choice = ReadIntInput("Your choice: ");

            switch (choice)
            {
                case 1:
                    currentConfig.Clock =
                        ReadDateTimeInput("Enter NEW Clock DateTime (yyyy-MM-dd HH:mm:ss): ");
                    break;

                case 2:
                    currentConfig.ManagerId = ReadIntInput("Enter NEW Manager ID: ");
                    break;

                case 3:
                    currentConfig.ManagerPassword = ReadStringInput("Enter NEW Manager Password: ");
                    break;

                case 4:
                    currentConfig.AddressCompany =
                        ReadStringInput("Enter NEW Company Address (or blank to clear): ");
                    if (string.IsNullOrWhiteSpace(currentConfig.AddressCompany))
                        currentConfig.AddressCompany = null;
                    break;

                case 5:
                    currentConfig.Latitude =
                        ReadNullableDoubleInput("Enter NEW Latitude (DOUBLE or NULL): ");
                    break;

                case 6:
                    currentConfig.Longitude =
                        ReadNullableDoubleInput("Enter NEW Longitude (DOUBLE or NULL): ");
                    break;

                case 7:
                    currentConfig.MaxAirDeliveryDistanceKm =
                        ReadNullableDoubleInput("Enter NEW Max Air Delivery Distance Km (DOUBLE or NULL): ");
                    break;

                case 8:
                    currentConfig.AverageMotorcycleSpeedKmh =
                        ReadDoubleInput("Enter NEW Avg Motorcycle Speed: ");
                    break;

                case 9:
                    currentConfig.AverageBicycleSpeedKmh =
                        ReadDoubleInput("Enter NEW Avg Bicycle Speed: ");
                    break;

                case 10:
                    currentConfig.AverageCarSpeedKmh =
                        ReadDoubleInput("Enter NEW Avg Car Speed: ");
                    break;

                case 11:
                    currentConfig.AverageWalkingSpeedKmh =
                        ReadDoubleInput("Enter NEW Avg Walking Speed: ");
                    break;

                case 12:
                    currentConfig.MaxDeliveryTimeRange =
                        ReadTimeSpanInput("Enter NEW Max Delivery Time Range (hh:mm:ss): ");
                    break;

                case 13:
                    currentConfig.RiskTimeRange =
                        ReadTimeSpanInput("Enter NEW Risk Time Range (hh:mm:ss): ");
                    break;

                case 14:
                    currentConfig.InactivityTimeRange =
                        ReadTimeSpanInput("Enter NEW Inactivity Time Range (hh:mm:ss): ");
                    break;

                case 15:
                    currentConfig.BaseSalaryMounthly =
                        ReadDoubleInput("Enter NEW Base Salary Monthly: ");
                    break;

                case 16:
                    currentConfig.DeliveryRatePerKm =
                        ReadDoubleInput("Enter NEW Delivery Rate Per Km: ");
                    break;

                case 0:
                    done = true;
                    break;

                default:
                    Console.WriteLine("Invalid option. Try again.");
                    break;
            }
        }

        // Save updated configuration
        s_bl.Admin.SetConfig(currentConfig);
        Console.WriteLine("Configuration updated successfully.");
    }

    private static void InitializeDB()
    {
        Console.WriteLine("\n--- Initialize Database ---");
        s_bl.Admin.InitializeDB();
        Console.WriteLine("Database initialized successfully.");
    }

    private static void ResetDB()
    {
        Console.WriteLine("\n--- Reset Database ---");
        s_bl.Admin.ResetDB();
        Console.WriteLine("Database reset successfully.");
    }
    
}




