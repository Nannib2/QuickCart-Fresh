using Dal;
using DalApi;
using DO;
using System;
using System.Runtime.ConstrainedExecution;
using static System.Collections.Specialized.BitVector32;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DalTest;

internal class Program
{

    //static readonly IDal s_dal = new DalList(); //stage 2
    //static readonly IDal s_dal = new DalXml(); //stage 3
    static readonly IDal s_dal = Factory.Get; //stage 4

    /// <summary>
    /// Represents the main menu options available in the system.
    /// Each value corresponds to a user action in the main menu.
    /// </summary>
    public enum MainMenuOption
    {
        /// <summary>
        /// Exit the main menu and terminate the program.
        /// </summary>
        Exit = 0,

        /// <summary>
        /// Display the sub-menu for Courier.
        /// </summary>
        CourierMenu = 1,

        /// <summary>
        /// Display the sub-menu for Order.
        /// </summary>
        OrderMenu = 2,

        /// <summary>
        /// Display the sub-menu for Delivery.
        /// </summary>
        DeliveryMenu = 3,

        /// <summary>
        /// Perform data initialization by calling Initialization.Do().
        /// </summary>
        Initialization = 4,

        /// <summary>
        /// Display all data currently stored in the database for all entities.
        /// </summary>
        DisplayAllData = 5,

        /// <summary>
        /// Display the configuration entity sub-menu.
        /// </summary>
        ConfigMenu = 6,
        /// <summary>
        ///   Reset database and config  
        /// </summary>
        ResetDatabaseAndConfig = 7
    }

    /// <summary>
    /// Reset the database and configuration data to their default state.
    //
    /// <summary>
    /// Defines the standard set of actions (CRUD operations and batch actions)
    /// that can be performed on an entity (e.g., Courier, Order, Delivery)
    /// within the system menus.
    /// </summary>
    public enum EntityAction
    {
        /// <summary>
            /// Exits the current entity menu and returns to the Main Menu.
            /// </summary>
        Exit,
        /// <summary>
            /// Action to create a new entity record.
            /// </summary>
        Create,
        /// <summary>
            /// Action to read and display a single entity record based on an identifier.
            /// </summary>
        Read,
        /// <summary>
            /// Action to read and display all existing entity records.
            /// </summary>
        ReadAll,
        /// <summary>
            /// Action to update an existing entity record.
            /// </summary>
        Update,
        /// <summary>
            /// Action to delete a single entity record.
            /// </summary>
        Delete,
        /// <summary>
            /// Action to delete all entity records of the specific type.
            /// </summary>
        DeleteAll
    }
    /// <summary>
    /// Defines the various configuration values that can be manipulated
    /// or viewed within the system's Configuration Menu.
    /// </summary>
    public enum ConfigValues
    {
        /// <summary>
            /// The current system time (simulation clock).
            /// </summary>
        Clock,
        /// <summary>
            /// The manager's system identification ID.
            /// </summary>
        MangerId,
        /// <summary>
            /// The password associated with the manager ID.
            /// </summary>
        MangerPassword,
        /// <summary>
          	/// The geographical address of the main company headquarters.
          	/// </summary>
        AddressCompany,
        /// <summary>
          	/// The latitude coordinate of the company headquarters.
          	/// </summary>
        Latitude,
        /// <summary>
          	/// The longitude coordinate of the company headquarters.
          	/// </summary>
        Longitude,
        /// <summary>
          	/// The maximum distance (in kilometers) allowed for drone/air deliveries.
          	/// </summary>
        MaxAirDeliveryDistanceKm,
        /// <summary>
          	/// The average speed (in km/h) used for calculating motorcycle delivery times.
          	/// </summary>
        AverageMotorcycleSpeedKmh,
        /// <summary>
          	/// The average speed (in km/h) used for calculating bicycle delivery times.
          	/// </summary>
        AverageBicycleSpeedKmh,
        /// <summary>
          	/// The average speed (in km/h) used for calculating car delivery times.
          	/// </summary>
        AverageCarSpeedKmh,
        /// <summary>
          	/// The average speed (in km/h) used for calculating walking delivery times.
          	/// </summary>
        AverageWalkingSpeedKmh,
        /// <summary>
          	/// The maximum acceptable time range for a delivery to be considered on time.
          	/// </summary>
        MaxDeliveryTimeRange,
        /// <summary>
          	/// The time range threshold after which a delivery is considered at risk.
          	/// </summary>
        RiskTimeRange,
        /// <summary>
          	/// The time range after which a courier or system component is considered inactive.
          	/// </summary>
        InactivityTimeRange
    }
    /// <summary>
    /// Defines the specific actions available within the system's Configuration Menu,
    /// focusing on time manipulation and configuration management.
    /// </summary>
    public enum ConfigAction
    {
        /// <summary>
            /// Exits the Configuration Menu and returns to the Main Menu.
          	/// </summary>
        Exit = 0,
        /// <summary>
          	/// Advances the system clock by one minute for simulation purposes.
          	/// </summary>
        AdvanceMinute = 1,
        /// <summary>
          	/// Advances the system clock by one hour for simulation purposes.
          	/// </summary>
        AdvanceHour = 2,
        /// <summary>
          	/// Advances the system clock by one day for simulation purposes.
          	/// </summary>
        AdvanceDay = 3,
        /// <summary>
          	/// Advances the system clock by one month for simulation purposes.
          	/// </summary>
        AdvanceMonth = 4,
        /// <summary>
          	/// Advances the system clock by one year for simulation purposes.
          	/// </summary>
        AdvanceYear = 5,
        /// <summary>
          	/// Displays the current time of the system's simulation clock.
          	/// </summary>
        ShowClock = 6,
        /// <summary>
          	/// Allows the user to set a specific configuration value (from <see cref="ConfigValues"/>).
          	/// </summary>
        SetConfig = 7,
        /// <summary>
          	/// Retrieves and displays a specific configuration value (from <see cref="ConfigValues"/>).
          	/// </summary>
        GetConfig = 8,
        /// <summary>
          	/// Resets all configuration values to their default factory settings.
          	/// </summary>
        ResetConfig = 9
    }
    /// <summary>
    /// Displays the main console menu for the application. This method provides
    /// the primary navigation point to all sub-menus and system-level operations.
    /// It runs in a loop until the user selects the Exit option.
    /// </summary>
    ///This code block, which includes the 'switch' statement logic and initial data structure definitions, was created using an artificial intelligence(AI) model.
    private static void ShowMainMenu()
    {
        bool flag = true;
        while (flag)
        {
            Console.WriteLine("===== MAIN MENU =====");
            Console.WriteLine("0 - Exit");
            Console.WriteLine("1 - Courier Menu");
            Console.WriteLine("2 - Order Menu");
            Console.WriteLine("3 - Delivery Menu");
            Console.WriteLine("4 - Initialize Data");
            Console.WriteLine("5 - Display All Data");
            Console.WriteLine("6 - Configuration Menu");
            Console.WriteLine("7 - Reset Database and Configuration");
            Console.Write("Enter your choice: ");
            MainMenuOption choice;
            while (true)
            {
                if (Enum.TryParse(Console.ReadLine(), out choice))
                    break;
                else
                {
                    Console.WriteLine("Invalid input, please try again.\n");
                }
            }

            switch (choice)
            {
                case MainMenuOption.Exit:
                    {
                        Console.WriteLine("Exiting the program. Goodbye!");
                        flag = false;
                    }
                    break;

                case MainMenuOption.CourierMenu:
                    EntityMenu("Courier");
                    break;

                case MainMenuOption.OrderMenu:
                    EntityMenu("Order");
                    break;

                case MainMenuOption.DeliveryMenu:
                    EntityMenu("Delivery");
                    break;

                case MainMenuOption.Initialization:
                    InitializeData();
                    break;

                case MainMenuOption.DisplayAllData:
                    DisplayingAllData();
                    break;

                case MainMenuOption.ConfigMenu:
                    ConfigMenu();
                    break;

                case MainMenuOption.ResetDatabaseAndConfig:
                    Console.WriteLine("Resetting database and configuration...");
                    ResetDatabaseAndConfig();
                    break;

                default:
                    Console.WriteLine("Unknown option selected.");
                    break;
            }

        }
    }


    /// <summary>
    /// Displays a generic CRUD (Create, Read, Update, Delete) menu for a specified entity
    /// and handles user input to dispatch the appropriate action.
    /// The menu options are based on the <see cref="EntityAction"/> enumeration.
    /// </summary>
    /// <param name="entity">The name of the entity to manage (e.g., "Courier", "Order", "Delivery").
    /// This string is used for display purposes and to determine which specific functions to call.</param>
    ///This code block, which includes the 'switch' statement logic and initial data structure definitions, was created using an artificial intelligence(AI) model.
    private static void EntityMenu(string entity)
    {
        bool flag = true;
        while (flag)
        {
            Console.WriteLine($"\n=== {entity.ToUpper()} MENU ===");
            Console.WriteLine("0 - Exit");
            Console.WriteLine("1 - Create");
            Console.WriteLine("2 - Read");
            Console.WriteLine("3 - Read All");
            Console.WriteLine("4 - Update");
            Console.WriteLine("5 - Delete");
            Console.WriteLine("6 - Delete All");
            Console.Write("Enter your choice: ");
            EntityAction action;
            while (true)
            {
                if (!EntityAction.TryParse(Console.ReadLine(), out action) &&
                    Enum.IsDefined(typeof(EntityAction), action))
                {
                    Console.WriteLine("Invalid input, please try again.\n");
                }
                else
                    break;
            }
            switch (action)
            {
                case EntityAction.Exit:
                    {
                        Console.WriteLine($"Exiting {entity} menu.");
                        flag = false;
                    }
                    break;

                case EntityAction.Create:
                    if (entity == "Courier")
                    {
                        CreateCourier();
                    }
                    else if (entity == "Order")
                    {
                        CreateOrder();
                    }
                    else if (entity == "Delivery")
                    {
                        CreateDelivery();
                    }

                    break;

                case EntityAction.Read:
                    if (entity == "Courier")
                    {
                        ReadEntity("Courier");
                    }
                    else if (entity == "Order")
                    {
                        ReadEntity("Order");
                    }
                    else if (entity == "Delivery")
                    {
                        ReadEntity("Delivery");
                    }
                    break;

                case EntityAction.ReadAll:
                    if (entity == "Courier")
                    {
                        ReadAllCouriers();
                    }
                    else if (entity == "Order")
                    {
                        ReadAllOrders();
                    }
                    else if (entity == "Delivery")
                    {
                        ReadAllDeliveries();
                    }
                    break;

                case EntityAction.Update:
                    if (entity == "Courier")
                    {
                        UpdateCourier();
                    }
                    else if (entity == "Order")
                    {
                        UpdateOrder();
                    }
                    else if (entity == "Delivery")
                    {
                        UpdateDelivery();
                    }
                    break;

                case EntityAction.Delete:
                    if (entity == "Courier")
                    {
                        DeleteCourier("Courier");
                    }
                    else if (entity == "Order")
                    {
                        DeleteOrder("Order");
                    }
                    else if (entity == "Delivery")
                    {
                        DeleteDelivery("Delivery");
                    }
                    break;

                case EntityAction.DeleteAll:
                    if (entity == "Courier")
                    {
                        s_dal!.Courier.DeleteAll();
                    }
                    else if (entity == "Order")
                    {
                        s_dal!.Order.DeleteAll();
                    }
                    else if (entity == "Delivery")
                    {
                        s_dal!.Delivery.DeleteAll();
                    }
                    break;

                default:
                    Console.WriteLine("Unknown option selected.");
                    break;
            }


        }
    }

    /// <summary>
    /// Prompts the user for a Delivery ID and performs a delete operation on the corresponding data access layer.
    /// Handles potential exceptions during the reading of the ID or the deletion process.
    /// </summary>
    /// <param name="entity">The name of the entity being deleted (should be "Delivery" in this context), used for display in console messages.</param>
    private static void DeleteDelivery(string entity)
    {
        try
        {
            int id = ReadId(entity, "delete");
            s_dal!.Delivery.Delete(id);
            Console.WriteLine($"{entity} with ID {id} deleted successfully.");
        }
        catch (DalDoesNotExistException ex)
        {
             Console.WriteLine($"Error deleting {entity}: {ex.Message}");
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error deleting {entity}: {ex.Message}");
        }
    }
    /// <summary>
    /// Prompts the user for an Order ID and performs a delete operation on the corresponding data access layer.
    /// Handles potential exceptions during the reading of the ID or the deletion process.
    /// </summary>
    /// <param name="entity">The name of the entity being deleted (should be "Order" in this context), used for display in console messages.</param>
    private static void DeleteOrder(string entity)
    {
        try
        {
            int id = ReadId(entity, "delete");
            s_dal!.Order.Delete(id);
            Console.WriteLine($"{entity} with ID {id} deleted successfully.");
        }
        catch (DalDoesNotExistException ex)
        {
            Console.WriteLine($"Error deleting {entity}: {ex.Message}");
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error deleting {entity}: {ex.Message}");
        }
    }
    /// <summary>
    /// Prompts the user for a Courier ID and performs a delete operation on the corresponding data access layer.
    /// Handles potential exceptions during the reading of the ID or the deletion process.
    /// </summary>
    /// <param name="entity">The name of the entity being deleted (should be "Courier" in this context), used for display in console messages.</param>
    private static void DeleteCourier(string entity)
    {
        try
        {
            int id = ReadId(entity, "delete");
            s_dal!.Courier.Delete(id);
            Console.WriteLine($"{entity} with ID {id} deleted successfully.");
        }
        catch (DalDoesNotExistException ex)
        {
            Console.WriteLine($"Error deleting {entity}: {ex.Message}");
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error deleting {entity}: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles the interactive process of creating a new Courier entity.
    /// Prompts the user for all necessary courier details (ID, name, phone, email, password, active status, max distance, and delivery type).
    /// Validates user input for numerical and boolean fields, then constructs a new <c>Courier</c> object and persists it
    /// using the data access layer (<c>s_dalCourier</c>).
    /// The courier's start time is set to the current system clock (<c>s_dalConfig!.Clock</c>).
    /// </summary>
    private static void CreateCourier()
    {
        try
        {
            Console.WriteLine("\n=== Add New Courier ===");

           
            int id = ReadId("Courier","add");
            Console.Write("Enter Full Name: ");
            string fullName = Console.ReadLine()!;

            Console.Write("Enter Phone Number (10 digits): ");
            string phone = Console.ReadLine()!;

            Console.Write("Enter Email Address: ");
            string email = Console.ReadLine()!;

            Console.Write("Enter Password: ");
            string password = Console.ReadLine()!;

            Console.Write("Is the courier active? (true/false): ");
            bool isActive;
            while (true)
            {
                if (bool.TryParse(Console.ReadLine(), out isActive))
                    break;
                else
                    Console.WriteLine("Invalid input. Please enter a valid value and try again.");
            }

            Console.Write("Enter Max Distance (or leave empty for unlimited): ");
            string? maxDistInput = Console.ReadLine();
            double? maxDistance = null;

            if (!string.IsNullOrWhiteSpace(maxDistInput))
            {
                double parsedValue;
                while (true)
                {
                    if (double.TryParse(maxDistInput, out parsedValue))
                    {
                        maxDistance = parsedValue;
                        break;
                    }
                    else
                        Console.WriteLine("Invalid format. Please enter a valid number.");
                }

            }

            Console.WriteLine("Select Delivery Type:");
            Console.WriteLine("0 - Motorcycle");
            Console.WriteLine("1 - Car");
            Console.WriteLine("2 - Bike");
            Console.WriteLine("3 - Foot");

            DeliveryTypeMethods deliveryType;
            while (true)
            { 
                string? input = Console.ReadLine();

                if (DeliveryTypeMethods.TryParse(input, out deliveryType) &&
                    Enum.IsDefined(typeof(DeliveryTypeMethods), deliveryType))
                {
                    break; 
                }

                Console.WriteLine("Invalid delivery type. Please enter a valid number or name.");
            }

            DateTime startWorkTime = s_dal!.Config.Clock;

            Courier newItem = new Courier
            {
                Id = id,
                NameCourier = fullName,
                PhoneNumber = phone,
                EmailCourier = email,
                PasswordCourier = password,
                Active = isActive,
                PersonalMaxAirDistance = maxDistance,
                CourierDeliveryType = deliveryType,
                EmploymentStartDateTime = startWorkTime
            };

            s_dal!.Courier.Create(newItem);

            Console.WriteLine("Courier added successfully!");

        }
        catch (DalAlreadyExistsException ex)
        {
            Console.WriteLine($"Error creating courier: {ex.Message}");
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error creating courier: {ex.Message}");
        }


    }
    /// <summary>
    /// Handles the interactive process of creating a new Order entity.
    /// Prompts the user for order specifics including type, description, customer address, geographical coordinates (latitude/longitude),
    /// customer details, and item amount.
    /// Automatically determines <c>FreeShippingEligibility</c> based on the item amount and sets the <c>OpenOrderDateTime</c>
    /// using the current system clock (<c>s_dalConfig!.Clock</c>).
    /// The new <c>Order</c> is persisted using the data access layer (<c>s_dalOrder</c>).
    /// </summary>
    private static void CreateOrder()
    {
        try
        {
            Console.WriteLine("\n=== Add New Order ===");
            Console.WriteLine("Select Order Type:");
            Console.WriteLine("0 - Frozen");
            Console.WriteLine("1 - Chilled");
            Console.WriteLine("2 - Dry");
            Console.WriteLine("3 - Fragile");
            Console.WriteLine("4 - Mixed");
            OrderRequirements type;
            while (true)
            {
                string? input = Console.ReadLine();

                if (OrderRequirements.TryParse(input, out type) &&
                    Enum.IsDefined(typeof(OrderRequirements), type))
                {
                    break;
                }
                Console.WriteLine("Invalid order type. Please enter a valid number or name.");
            }
            Console.Write("Enter Description (optional): ");
            string? description = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(description))
                description = null;

            Console.Write("Enter Full Address: ");
            string address = Console.ReadLine()!;
            Console.Write("Enter Full latitude: ");
            double latitude;
            while (true)
            {
                if (double.TryParse(Console.ReadLine(), out latitude))
                    break;
                else
                    Console.WriteLine("Invalid input. Please enter a valid value and try again.");
            }

            Console.Write("Enter Longitude: ");
            double longitude;
            while (true)
            {
                if (double.TryParse(Console.ReadLine(), out longitude))
                    break;
                else
                    Console.WriteLine("Invalid input. Please enter a valid value and try again.");
            }
            Console.Write("Enter Customer Full Name: ");
            string customerName = Console.ReadLine()!;

            Console.Write("Enter Customer Phone (10 digits): ");
            string customerPhone = Console.ReadLine()!;

            Console.Write("Enter Amount of items: ");
            int amount;
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out amount))
                    break;
                Console.WriteLine("Invalid input. Please enter a valid value and try again.");
            }
            bool freeShippingEligibility = false;
            if (amount > 20)
                freeShippingEligibility = true;

            DateTime orderTime = s_dal!.Config.Clock;
            Order newItem = new Order
            {
                Id = 0,
                OrderType = type,
                ShortOrderDescription = description,
                OrderAddress = address,
                Latitude = latitude,
                Longitude = longitude,
                CustomerFullName = customerName,
                CustomerPhone = customerPhone,
                AmountItems = amount,
                FreeShippingEligibility = freeShippingEligibility,
                OpenOrderDateTime = orderTime
            };

            s_dal!.Order.Create(newItem);

            Console.WriteLine("Order added successfully!");
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error creating an order: {ex.Message}");
        }

    }
    /// <summary>
    /// Handles the interactive process of creating a new Delivery entity.
    /// Prompts the user for a valid existing Order ID and an active Courier ID to link the delivery.
    /// Performs checks to ensure the Order and Courier IDs are valid and the Courier is active.
    /// Calculates the initial <c>DeliveryDistanceKm</c> between the company's address (from config) and the order's destination.
    /// The new <c>Delivery</c> is initialized and persisted using the data access layer (<c>s_dalDelivery</c>).
    /// </summary>
    private static void CreateDelivery()
    {
        try
        {
            Console.WriteLine("\n=== Add New Delivery ===");
            int id = ReadId("order", "deliver");
            if (s_dal!.Order.Read(id) == null)
                throw new DalDoesNotExistException("Order ID does not exist");

            int idCourier = ReadId("courier", "make the delivery");
            Courier? courier = s_dal!.Courier.Read(idCourier);
            if (courier == null || courier.Active == false)
                throw new DalDoesNotExistException("Courier ID does not exist or courier is not active");


            DeliveryTypeMethods deliveryType = (DeliveryTypeMethods)courier!.CourierDeliveryType;
            
           

            DateTime startWorkTime = s_dal!.Config.Clock;
            Order order = s_dal.Order.Read(id)!;
            double lat = s_dal.Config.Latitude ?? 0;
            double lon = s_dal.Config.Longitude ?? 0;
            double deliveryDistanceKm = CalculateDistance(order.Latitude, order.Longitude, lat, lon);
            DeliveryCompletionType? delType = null;
            DateTime? endingWorkTime = null;
            Delivery newItem = new Delivery
            {
                Id = 0,
                OrderId = id,
                CourierId = idCourier,
                DeliveryType = deliveryType,
                OrderStartDateTime = startWorkTime,
                DeliveryDistanceKm = deliveryDistanceKm,
                DeliveryTypeEnding = delType,
                OrderEndDateTime = endingWorkTime
            };
            s_dal!.Delivery.Create(newItem);
            Console.WriteLine("Delivery added successfully!");
        }
        catch (DalDoesNotExistException ex)
        { 
            Console.WriteLine($"Error creating delivery: {ex.Message}");
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error creating delivery: {ex.Message}");
        }


    }

    /// <summary>
    /// Prompts the user to enter an ID for a specific entity and action (e.g., "view," "delete").
    /// This function ensures the user's input is a valid integer before returning it.
    /// </summary>
    /// <param name="entity">The name of the data object (e.g., "Courier", "Order") being manipulated.</param>
    /// <param name="action">The operation being performed (e.g., "view", "delete") to customize the prompt.</param>
    /// <returns>The validated integer ID entered by the user.</returns>
    private static int ReadId(string entity, string action)
    {
        Console.Write($"Enter the ID of the {entity} to {action}: ");

        while (true)
        {
            if (int.TryParse(Console.ReadLine(), out int id))
                return id;
            else
            {
                Console.WriteLine("Invalid input. Please enter a valid value and try again.");
            }
        }
    }

    /// <summary>
    /// Serves as a dispatcher method to read and display a single entity (Courier, Order, or Delivery) based on the input entity type.
    /// It first calls <c>ReadId</c> to prompt the user for the target ID and then uses a switch statement to delegate the display to the appropriate <c>Read</c> function.
    /// </summary>
    /// <param name="entity">The type of entity to read (e.g., "Courier", "Order", "Delivery").</param>
    private static void ReadEntity(string entity)
    {
        {
            try
            {
                int id = ReadId(entity, "view");
                switch (entity)
                {
                    case "Courier":
                        ReadCourier(id);
                        break;
                    case "Order":
                        ReadOrder(id);
                        break;
                    case "Delivery":
                        ReadDelivery(id);
                        break;
                    default: Console.WriteLine("Unknown entity type.");
                        break;

                }

            }
            catch (DalDoesNotExistException ex)
            {
                Console.WriteLine($"Error reading {entity}: {ex.Message}");
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                Console.WriteLine($"Error reading {entity}: {ex.Message}");
            }

        }
    }
    /// <summary>
    /// Retrieves a single Courier object by its ID from the data access layer (<c>s_dalCourier</c>)
    /// and prints its details to the console. Displays an error message if the Courier is not found.
    /// </summary>
    /// <param name="id">The unique identifier of the Courier to retrieve.</param>
    private static void ReadCourier(int id)
    {
            DO.Courier? courier = s_dal!.Courier.Read(id);
            if (courier != null)
            {
                Console.WriteLine(courier);
            }
            else
            {
                throw new DalDoesNotExistException($"Error: Courier with ID {id} not found.");
            }
    }
    /// <summary>
    /// Retrieves a single Delivery object by its ID from the data access layer (<c>s_dalDelivery</c>)
    /// and prints its details to the console. Displays an error message if the Delivery is not found.
    /// </summary>
    /// <param name="id">The unique identifier of the Delivery to retrieve.</param>
    private static void ReadDelivery(int id)
    {
        
            DO.Delivery? delivery = s_dal!.Delivery.Read(id);
            if (delivery != null)
            {
                Console.WriteLine(delivery);
            }
            else
            {
            throw new DalDoesNotExistException($"Error: Delivery with ID {id} not found.");
            }
        
      
    }
    /// <summary>
    /// Retrieves a single Order object by its ID from the data access layer (<c>s_dalOrder</c>)
    /// and prints its details to the console. Displays an error message if the Order is not found.
    /// </summary>
    /// <param name="id">The unique identifier of the Order to retrieve.</param>
    private static void ReadOrder(int id)
    {
       
            DO.Order? order = s_dal!.Order.Read(id);
            if (order != null)
            {
                Console.WriteLine(order);
            }
            else
            {
            throw new DalDoesNotExistException($"Error: Order with ID {id} not found.");
            }
       
    }


    /// <summary>
    /// Reads all Order entities from the data access layer (<c>s_dalOrder</c>) and displays them sequentially on the console.
    /// Prints a descriptive message if the collection of orders is empty.
    /// </summary>
    private static void ReadAllOrders()
    {
        Console.WriteLine("--- Displaying All Orders ---");
        try
        {
            IEnumerable<Order> orders = s_dal!.Order.ReadAll();



            foreach (Order order in orders)
            {
                Console.WriteLine(order);
            }

            if (!orders.Any())
            {
                Console.WriteLine("The order list is currently empty.");
            }
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine("Error reading  all the orders: {ex.Message}");
        }



    }
    /// <summary>
    /// Reads all Courier entities from the data access layer (<c>s_dalCourier</c>) and displays them sequentially on the console.
    /// Prints a descriptive message if the collection of couriers is empty.
    /// </summary>
    private static void ReadAllCouriers()
    {
        try
        {
            Console.WriteLine("--- Displaying All Couriers ---");
        

            IEnumerable<Courier> couriers = s_dal!.Courier.ReadAll();

            //  Iterate over the list and print each entity's data.

            foreach (Courier courier in couriers)
            {
                Console.WriteLine(courier); // Assumes DO.Courier has a readable ToString() implementation.
            }

            if (!couriers.Any())
            {
                Console.WriteLine("The courier list is currently empty.");
            }
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error reading  all the orders: {ex.Message}");
        }

    }
     /// <summary>
     /// Reads all Delivery entities from the data access layer (<c>s_dalDelivery</c>) and displays them sequentially on the console.
    /// Prints a descriptive message if the collection of deliveries is empty.
    /// </summary>
    private static void ReadAllDeliveries()
    {
        try
        {
            Console.WriteLine("--- Displaying All Deliverys ---");

            IEnumerable<Delivery> deliverys = s_dal!.Delivery.ReadAll();

            //  Iterate over the list and print each entity's data.

            foreach (Delivery delivery in deliverys)
            {
                Console.WriteLine(delivery);
            }

            if (!deliverys.Any())
            {
                Console.WriteLine("The delivery list is currently empty.");
            }
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error reading  all the deliverys: {ex.Message}");
        }

    }


    /// <summary>
    /// Prompts the user to update an existing Order entity by its ID.
    /// It guides the user through entering new values for the order's properties,
    /// validates critical input (ID, OrderType, Latitude, Longitude, AmountItems),
    /// and then constructs a new Order object to update the data access layer.
    /// </summary>
    private static void UpdateOrder()
    {
        try
        {
            int id = ReadId("Order", "update");
            if(s_dal!.Order.Read(id) == null)
                throw new DalDoesNotExistException("Order ID does not exist");
            Console.Write("The Order:");
            // Display the current state of the Order before prompting for changes.
            Console.WriteLine(s_dal!.Order.Read(id));
            Console.WriteLine("Select Order Type:");
            Console.WriteLine("0 - Frozen");
            Console.WriteLine("1 - Chilled");
            Console.WriteLine("2 - Dry");
            Console.WriteLine("3 - Fragile");
            Console.WriteLine("4 - Mixed");
            OrderRequirements type;
            // Input validation loop for the OrderRequirements enum.
            while (true)
            {
                if (OrderRequirements.TryParse(Console.ReadLine(), out type))
                    break;
                else
                    Console.WriteLine("Invalid input. Please enter a valid value and try again.");
            }


            Console.Write("Enter Description (optional): ");
            string? description = Console.ReadLine();
            // Sets description to null if the user leaves it empty or inputs only whitespace.
            if (string.IsNullOrWhiteSpace(description))
                description = null;

            Console.Write("Enter Full Address: ");
            string address = Console.ReadLine()!;

            Console.Write("Enter Latitude: ");
            double latitude;
            // Input validation loop for double type (Latitude).
            while (true)
            {
                if (double.TryParse(Console.ReadLine(), out latitude))
                    break;
                else
                    Console.WriteLine("Invalid input. Please enter a valid value and try again.");
            }

            Console.Write("Enter Longitude: ");
            double longitude;
            // Input validation loop for double type (Longitude).
            while (true)
            {
                if (double.TryParse(Console.ReadLine(), out longitude))
                    break;
                else
                    Console.WriteLine("Invalid input. Please enter a valid value and try again.");
            }
            Console.Write("Enter Customer Full Name: ");
            string customerName = Console.ReadLine()!;

            Console.Write("Enter Customer Phone (10 digits): ");
            string customerPhone = Console.ReadLine()!;

            Console.Write("Enter AmountItems: ");
            int amount;
            // Input validation loop for integer type (AmountItems).
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out amount))
                    break;
                else
                    Console.WriteLine("Invalid input. Please enter a valid value and try again.");
            }
            bool freeShippingEligibility = false;
            // Logic to determine if the order qualifies for free shipping.
            if (amount > 20)
                freeShippingEligibility = true;


            DateTime orderTime = s_dal!.Config.Clock;
            // Create the updated Order data object.
            Order newItem = new Order
            {
                Id = id,
                OrderType = type,
                ShortOrderDescription = description,
                OrderAddress = address,
                Latitude = latitude,
                Longitude = longitude,
                CustomerFullName = customerName,
                CustomerPhone = customerPhone,
                AmountItems = amount,
                FreeShippingEligibility = freeShippingEligibility,
                OpenOrderDateTime = orderTime
            };

            s_dal!.Order.Update(newItem);

            Console.WriteLine("Order Updateed successfully!");
        }
        catch (DalDoesNotExistException ex)
        {
            Console.WriteLine($"Error Updateing order: {ex.Message}");
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error Updateing order: {ex.Message}");
        }

    }
    /// <summary>
    /// Prompts the user to update an existing Courier entity by its ID.
    /// It collects updated details such as contact information, active status,
    /// maximum travel distance, and delivery vehicle type, validating input where necessary.
    /// </summary>
    private static void UpdateCourier()
    {
        try
        {
            int id = ReadId("Courier", "update");
            if(s_dal!.Courier.Read(id) == null)
                throw new DalDoesNotExistException("Courier ID does not exist");
            Console.Write("The Courier:");
            // Display the current state of the Courier before prompting for changes.
            Console.WriteLine(s_dal!.Courier.Read(id));
            Console.Write("Enter Full Name: ");
            string fullName = Console.ReadLine()!;

            Console.Write("Enter Phone Number (10 digits): ");
            string phone = Console.ReadLine()!;

            Console.Write("Enter Email Address: ");
            string email = Console.ReadLine()!;

            Console.Write("Enter Password: ");
            string password = Console.ReadLine()!;

            Console.Write("Is the courier active? (true/false): ");
            bool isActive;
            // Input validation loop for boolean type (isActive).
            while (true)
            {
                if (bool.TryParse(Console.ReadLine(), out isActive))
                    break;
            }
            Console.Write("Enter Max Distance (or leave empty for unlimited): ");
            string maxDistInput = Console.ReadLine()!;
            double? maxDistance = null;

            // Conditional block to handle the optional Max Distance input.
            if (!string.IsNullOrWhiteSpace(maxDistInput))
            {
                double parsedValue;
                // Input validation loop for double type (Max Distance).
                while (true)
                {
                    if (double.TryParse(maxDistInput, out parsedValue))
                    {
                        maxDistance = parsedValue;
                        break;
                    }
                    else
                        // nvalid input handling.
                        Console.WriteLine("Invalid input. Please enter a valid value and try again.");
                }

            }


            Console.WriteLine("Select Delivery Type:");
            Console.WriteLine("0 - Walking");
            Console.WriteLine("1 - Bicycle");
            Console.WriteLine("2 - Motorcycle");
            Console.WriteLine("3 - Car");
            int deliveryType;
            // Input validation loop for integer type (DeliveryType).
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out deliveryType))
                    break;
                Console.WriteLine("Invalid input. Please enter a valid value and try again.");
            }
            // Casts the validated integer input to the DeliveryTypeMethods enum.
            DeliveryTypeMethods vehicle = (DeliveryTypeMethods)deliveryType;
            DateTime startWorkTime = s_dal!.Config.Clock;

            // Create the updated Courier data object.
            Courier newItem = new Courier
            {
                Id = id,
                NameCourier = fullName,
                PhoneNumber = phone,
                EmailCourier = email,
                PasswordCourier = password,
                Active = isActive,
                PersonalMaxAirDistance = maxDistance,
                CourierDeliveryType = vehicle,
                EmploymentStartDateTime = startWorkTime
            };

            s_dal!.Courier.Update(newItem);

            Console.WriteLine("Courier updated successfully!");

        }
        catch (DalDoesNotExistException ex)
        { 
            Console.WriteLine($"Error updating courier: {ex.Message}"); 
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error updating courier: {ex.Message}");
        }

    }
    /// <summary>
    /// Prompts the user to update an existing Delivery entity by its ID.
    /// It requires updating the associated Order ID, Courier ID, and delivery type.
    /// It includes checks to ensure the Order ID exists and the Courier is active.
    /// </summary>
    private static void UpdateDelivery()
    {
        try
        {
            int idToUpdate = ReadId("Delivery", "update");
            if(s_dal!.Delivery.Read(idToUpdate) == null)
                throw new DalDoesNotExistException("Delivery ID does not exist");
            Console.Write("The Delivery:");
            // Display the current state of the Delivery before prompting for changes.
            Console.WriteLine(s_dal!.Delivery.Read(idToUpdate));
            Console.Write("Enter the order ID of the Delivery: ");
            int id;
            // Input validation loop for Order ID.
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out id))
                    break;
                Console.WriteLine("Invalid input. Please enter a valid value and try again.");
            }
            // Validation: Check if the referenced Order ID exists.
            if (s_dal!.Order.Read(id) == null)
                throw new DalDoesNotExistException("Order ID does not exist");

            Console.Write("Enter Courier ID to make the Delivery: ");
            int idCourier;
            // Input validation loop for Courier ID.
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out idCourier))
                    break;
            }
            Courier? courier = s_dal!.Courier.Read(idCourier);
            // Validation: Check if the referenced Courier ID exists and is active.
            if (courier == null || courier.Active == false)
                throw new DalDoesNotExistException("Courier ID does not exist or courier is not active");

            Console.WriteLine("Select Delivery Type:");
            Console.WriteLine("0 - Walking");
            Console.WriteLine("1 - Bicycle");
            Console.WriteLine("2 - Motorcycle");
            Console.WriteLine("3 - Car");

            int deliveryType;
            // Input validation loop for Delivery Type.
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out deliveryType))
                    break;
                Console.WriteLine("Invalid input. Please enter a valid value and try again.");
            }
            // Casts the validated integer input to the DeliveryTypeMethods enum.
            DeliveryTypeMethods vehicle = (DeliveryTypeMethods)deliveryType;

            DateTime startWorkTime = s_dal!.Config.Clock;
            Order order = s_dal.Order.Read(id)!;
            // Retrieval of current location from the global configuration (presumably the courier's current location).
            double lat = s_dal.Config.Latitude ?? 0;
            double lon = s_dal.Config.Longitude ?? 0;
            // Calculation of delivery distance using the Haversine formula.
            double deliveryDistanceKm = CalculateDistance(order.Latitude, order.Longitude, lat, lon);
            DeliveryCompletionType? delType = null;
            DateTime? endingWorkTime = null;
            // Create the updated Delivery data object. Note that DeliveryTypeEnding and OrderEndDateTime are set to null.
            Delivery newItem = new Delivery
            {
                Id = idToUpdate,
                OrderId = id,
                CourierId = idCourier,
                DeliveryType = vehicle,
                OrderStartDateTime = startWorkTime,
                DeliveryDistanceKm = deliveryDistanceKm,
                DeliveryTypeEnding = delType,
                OrderEndDateTime = endingWorkTime
            };
            s_dal!.Delivery.Update(newItem);
            Console.WriteLine("Delivery updated successfully!");
        }
        catch (DalDoesNotExistException ex)
        { 
            Console.WriteLine($"Error updating courier: {ex.Message}");
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error updating courier: {ex.Message}");
        }

    }
    /// <summary>
    /// Executes the initial data population logic via the static <c>Initialization.Do</c> method,
    /// passing all necessary DAL accessors and the configuration object.
    /// This is typically used to set up the environment with mock data at startup.
    /// </summary>
    private static void InitializeData()
    {
        try
        {
            //Initialization.Do(s_dal); //stage 2
            Initialization.Do(); //stage 4
            Console.WriteLine("Data initialized successfully!");
        }
        catch (DalNullReferenceException ex)
        {
            Console.WriteLine($"Initialization failed: {ex.Message}");
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Initialization failed: {ex.Message}");
        }
    }
    /// <summary>
    /// Reads and displays all entities (Couriers, Orders, and Deliveries)
    /// from the respective data access layers sequentially for debugging or overview purposes.
    /// </summary>
    private static void DisplayingAllData()
    {
        try 
        {
            Console.WriteLine("\n=== All Couriers ===");
            foreach (var courier in s_dal!.Courier.ReadAll())
                Console.WriteLine(courier);

            Console.WriteLine("\n=== All Orders ===");
            foreach (var order in s_dal!.Order.ReadAll())
                Console.WriteLine(order);

            Console.WriteLine("\n=== All Deliverys ===");
            foreach (var delivery in s_dal!.Delivery.ReadAll())
                Console.WriteLine(delivery);
        }
        catch( DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error displaying all data: {ex.Message}");
        }
        
    }


    /// <summary>
    /// Calculates the geographical distance in kilometers between two points
    /// specified by their latitude and longitude coordinates using the Haversine formula.
    /// </summary>
    /// <param name="lat1">Latitude of the first point (in degrees).</param>
    /// <param name="lon1">Longitude of the first point (in degrees).</param>
    /// <param name="lat2">Latitude of the second point (in degrees).</param>
    /// <param name="lon2">Longitude of the second point (in degrees).</param>
    /// <returns>The distance between the two points in kilometers.</returns>
    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        // Earth radius in kilometers (R).
        const double R = 6371.0;

        // Convert degrees to radians for use in trigonometric functions.
        double latRad1 = DegreesToRadians(lat1);
        double lonRad1 = DegreesToRadians(lon1);
        double latRad2 = DegreesToRadians(lat2);
        double lonRad2 = DegreesToRadians(lon2);

        double dlat = latRad2 - latRad1;
        double dlon = lonRad2 - lonRad1;

        // Haversine formula core calculation (a).
        double a = Math.Pow(Math.Sin(dlat / 2), 2) +
           Math.Cos(latRad1) * Math.Cos(latRad2) *
           Math.Pow(Math.Sin(dlon / 2), 2);

        // Calculate central angle (c).
        double c = 2 * Math.Asin(Math.Sqrt(a));

        // Final distance calculation.
        return R * c;
    }

    /// <summary>
    /// Converts an angle from degrees to radians, a necessary step for trigonometric
    /// calculations in methods like the Haversine distance formula.
    /// </summary>
    /// <param name="degrees">The angle value in degrees.</param>
    /// <returns>The angle value in radians.</returns>
    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
    /// <summary>
    /// Resets the application state by calling the <c>DeleteAll</c> method on all entity data access layers (Courier, Order, Delivery)
    /// and resetting the global configuration object.
    /// </summary>
    private static void ResetDatabaseAndConfig()
    {
        try
        {
            s_dal!.Courier.DeleteAll();
            s_dal!.Order.DeleteAll();
            s_dal!.Delivery.DeleteAll();


            s_dal!.Config.Reset();

            Console.WriteLine("Database and configuration reset successfully!");
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error resetting database and configuration: {ex.Message}");
        }



    }

    /// <summary>
    /// Displays the configuration submenu, handles user input for configuration actions,
    /// and dispatches to the appropriate handler methods for clock management or
    /// configuration variable manipulation. The menu loops until the user chooses to exit (0).
    /// </summary>
    ///This code block, which includes the 'switch' statement logic and initial data structure definitions, was created using an artificial intelligence(AI) model.
    private static void ConfigMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- Select Configuration Action ---");
            Console.WriteLine("0 - Exit the submenu");
            Console.WriteLine("1 - Advance system clock by one minute");
            Console.WriteLine("2 - Advance system clock by one hour");
            Console.WriteLine("3 - Advance system clock by one day");
            Console.WriteLine("4 - Advance system clock by one month");
            Console.WriteLine("5 - Advance system clock by one year");
            Console.WriteLine("6 - Show current system clock value");
            Console.WriteLine("7 - Set new value for a configuration variable");
            Console.WriteLine("8 - Show current value for a configuration variable");
            Console.WriteLine("9 - Reset all configuration variables");
            Console.Write("Enter your choice: ");
            int actionChoice;
            while (true)
            {

                if (int.TryParse(Console.ReadLine(), out actionChoice))
                    break;
                Console.WriteLine("Invalid input. Please enter a valid value and try again.");
            }

            ConfigAction action = (ConfigAction)actionChoice;

                switch (action)
                {
                    case ConfigAction.Exit: break;
                    case ConfigAction.AdvanceMinute:
                        HandleAdvanceMinute();
                        break;

                    case ConfigAction.AdvanceHour:
                        HandleAdvanceHour();
                        break;

                    case ConfigAction.AdvanceDay:
                        HandleAdvanceDay();
                        break;

                    case ConfigAction.AdvanceMonth:
                        HandleAdvanceMonth();
                        break;

                    case ConfigAction.AdvanceYear:
                        HandleAdvanceYear();
                        break;

                    case ConfigAction.ShowClock:
                        HandleShowClock();
                        break;

                    case ConfigAction.SetConfig:
                        ChangeConfigMenu();
                        break;

                    case ConfigAction.GetConfig:
                        HandleGetConfig();
                        break;

                    case ConfigAction.ResetConfig:
                        HandleResetConfig();
                        break;

                    default:
                        Console.WriteLine("unknow");
                        break;
                }

                if (action == ConfigAction.Exit)
                {
                    break;
                }
        }
    }
    /// <summary>
    /// Attempts to advance the system clock stored in the configuration by one minute.
    /// Reports the (intended) new time to the console.
    /// </summary>
    private static void HandleAdvanceMinute()
    {
        try 
        {
            s_dal!.Config.Clock = s_dal!.Config.Clock.AddMinutes(1);
            Console.WriteLine($" Clock advanced by 1 minute. New time: {s_dal!.Config.Clock}");
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error advancing clock: {ex.Message}");
        }


    }
    /// <summary>
    /// Attempts to advance the system clock stored in the configuration by one day (24 hours).
    /// Reports the (intended) new time to the console.
    /// </summary>
    private static void HandleAdvanceDay()
    {
        try
        {
            s_dal!.Config.Clock = s_dal!.Config.Clock.AddDays(1);
            Console.WriteLine($" Clock advanced by 1 day. New time: {s_dal!.Config.Clock}");
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error advancing clock: {ex.Message}");
        }
    }
    /// <summary>
    /// Attempts to advance the system clock stored in the configuration by one hour.
    /// Reports the (intended) new time to the console.
    /// </summary>
    private static void HandleAdvanceHour()
    {
        try
        {
            s_dal!.Config.Clock = s_dal!.Config.Clock.AddHours(1);
            Console.WriteLine($" Clock advanced by 1 hour. New time: {s_dal!.Config.Clock}");
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error advancing clock: {ex.Message}");
        }
    }
    /// <summary>
    /// Attempts to advance the system clock stored in the configuration by one month.
    /// Reports the (intended) new time to the console.
    /// </summary>
    private static void HandleAdvanceMonth()
    {
        try
        {
            s_dal!.Config.Clock = s_dal!.Config.Clock.AddMonths(1);
            Console.WriteLine($" Clock advanced by 1 Month. New time: {s_dal!.Config.Clock}");
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error advancing clock: {ex.Message}");
        }
    }
    /// <summary>
    /// Attempts to advance the system clock stored in the configuration by one year.
    /// Reports the (intended) new time to the console.
    /// </summary>
    private static void HandleAdvanceYear()
    {
        try
        {
            s_dal!.Config.Clock = s_dal!.Config.Clock.AddYears(1);
            Console.WriteLine($" Clock advanced by 1 year. New time: {s_dal!.Config.Clock}");
        }
        catch (DalXMLFileLoadCreateException ex)
        
        {
            Console.WriteLine($"Error advancing clock: {ex.Message}");
        }
    }

    /// <summary>
    /// Displays the current value of the system clock as stored in the application configuration.
    /// </summary>
    private static void HandleShowClock()
    {
        try
        {
            Console.WriteLine($"Current System Clock: {s_dal!.Config.Clock}");
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error showing clock: {ex.Message}");
        }
    }

    /// <summary>
    /// Prompts the user to enter a new date and time value. If successfully parsed as a <see cref="DateTime"/>,
    /// it sets this new value for the system clock in the configuration.
    /// </summary>
    private static void ChangeClock()
    {

        Console.Write("Enter new clock value:");
        DateTime newClock;
        while (!DateTime.TryParse(Console.ReadLine(), out newClock))
        {
            Console.Write("Invalid format. Please enter a valid date and time (e.g., 2023-12-31 14:30): ");
        }
        try
        {
            s_dal!.Config.Clock = newClock;
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error changing clock: {ex.Message}");
        }

    }
    /// <summary>
    /// Prompts the user to enter a new integer value for the Manager ID.
    /// It validates the input and updates the configuration upon success.
    /// </summary>
    private static void ChangeManagerId()
    {
        try
        {
            Console.Write("Enter new Manager ID: ");
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out int newId))
                {
                    s_dal!.Config.ManagerId = newId;
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid input.");
                    Console.Write("Enter new Manager ID: ");
                }

            }
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error changing Manager ID: {ex.Message}");
        }
    }
    /// <summary>
    /// Prompts the user to enter a new string value for the Manager Password.
    /// The configuration is updated directly with the user's input.
    /// </summary>
    private static void ChangeManagerPassword()
    {
        try
        {
            Console.Write("Enter new Manager Password: ");
            string newPassword = Console.ReadLine()!;
            s_dal!.Config.ManagerPassword = newPassword;
        }
        catch(DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error changing Manager Password: {ex.Message}");
        }
    }
    /// <summary>
    /// Prompts the user to enter a new string value for the Company Address.
    /// The configuration is updated directly with the user's input.
    /// </summary>
    private static void ChangeCompanyAddress()
    {
        try
        {
            Console.Write("Enter new Company Address: ");
            string newAddress = Console.ReadLine()!;
            s_dal!.Config.AddressCompany = newAddress;
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error changing Company Address: {ex.Message}");
        }
    }
    /// <summary>
    /// Prompts the user to enter a new floating-point value for the Latitude.
    /// It validates the input as a <see cref="double"/> and updates the configuration upon success.
    /// </summary>
    private static void ChangeLatitude()
    {
        try
        {
            Console.Write("Enter new Latitude: ");
            while (true)
            {
                if (double.TryParse(Console.ReadLine(), out double newLatitude))
                {
                    s_dal!.Config.Latitude = newLatitude;
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid input.");
                    Console.Write("Enter new Latitude: ");
                }

            }
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error changing Latitude: {ex.Message}");
        }
    }
    /// <summary>
    /// Prompts the user to enter a new floating-point value for the Longitude.
    /// It validates the input as a <see cref="double"/> and updates the configuration upon success.
    /// </summary>
    private static void ChangeLongitude()
    {
        try
        {
            Console.Write("Enter new Longitude: ");
            while (true)
            {
                if (double.TryParse(Console.ReadLine(), out double newLongitude))
                {
                    s_dal!.Config.Longitude = newLongitude;
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid input.");
                    Console.Write("Enter new Longitude: ");
                }
            }
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error changing Longitude: {ex.Message}");
        }
    }
    /// <summary>
    /// Prompts the user to enter a new floating-point value for the maximum air delivery distance (in km).
    /// It validates the input as a <see cref="double"/> and updates the configuration upon success.
    /// </summary>
    public static void ChangeMaxAirDeliveryDistance()
    {
        try
        {
            Console.Write("Enter new Max Air Delivery Distance (km): ");//need to able to null
            while (true)
            {
                if (double.TryParse(Console.ReadLine(), out double newDistance))
                {
                    s_dal!.Config.MaxAirDeliveryDistanceKm = newDistance;
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid input.");
                    Console.Write("Enter new Max Air Delivery Distance (km): ");
                }
            }
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error changing Max Air Delivery Distance: {ex.Message}");
        }
    }
    /// <summary>
    /// Prompts the user for a new average motorcycle speed (in km/h).
    /// It validates the input as a <see cref="double"/> and updates the configuration upon success.
    /// </summary>
    private static void ChangeMotorcycleSpeed()
    {
        try
        {
            Console.Write("Enter new Average Motorcycle Speed (km/h): ");
            while (true)
            {
                if (double.TryParse(Console.ReadLine(), out double newSpeed))
                {
                    s_dal!.Config.AverageMotorcycleSpeedKmh = newSpeed;
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid input.");
                    Console.Write("Enter new Average Motorcycle Speed (km/h): ");
                }

            }
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error changing Motorcycle Speed: {ex.Message}");
        }
    }
    /// <summary>
    /// Prompts the user for a new average bicycle speed (in km/h).
    /// It validates the input as a <see cref="double"/> and updates the configuration upon success.
    /// </summary>
    private static void ChangeBicycleSpeed()
    {
        try
        {
            Console.Write("Enter new Average Bicycle Speed (km/h): ");
            while (true)
            {
                if (double.TryParse(Console.ReadLine(), out double newSpeed))
                {
                    s_dal!.Config.AverageBicycleSpeedKmh = newSpeed;
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid input.");
                    Console.Write("Enter new Average Bicycle Speed (km/h): ");
                }
            }
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error changing Bicycle Speed: {ex.Message}");
        }
    }
    /// <summary>
    /// Prompts the user for a new average car speed (in km/h).
    /// It validates the input as a <see cref="double"/> and updates the configuration upon success.
    /// </summary>
    private static void ChangeCarSpeed()
    {
        try
        {
            Console.Write("Enter new Average Car Speed (km/h): ");
            while (true)
            {
                if (double.TryParse(Console.ReadLine(), out double newSpeed))
                {
                    s_dal!.Config.AverageCarSpeedKmh = newSpeed;
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid input.");
                    Console.Write("Enter new Average Car Speed (km/h): ");
                }

            }
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error changing Car Speed: {ex.Message}");
        }

    }
    /// <summary>
    /// Prompts the user for a new average walking speed (in km/h).
    /// It validates the input as a <see cref="double"/> and updates the configuration upon success.
    /// </summary>
    private static void ChangeWalkingSpeed()
    {
        try
        {
            Console.Write("Enter new Average Walking Speed (km/h): ");
            while (true)
            {
                if (double.TryParse(Console.ReadLine(), out double newSpeed))
                {
                    s_dal!.Config.AverageWalkingSpeedKmh = newSpeed;
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid input.");
                    Console.Write("Enter new Average Walking Speed (km/h): ");
                }

            }
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error changing Walking Speed: {ex.Message}");

        }
    }
   
    /// <summary>
    /// Prompts the user for a new maximum delivery time range.
    /// It validates the input as a <see cref="TimeSpan"/> and updates the configuration upon success.
    /// </summary>
    private static void ChangeMaxDeliveryTimeRange()
    {
        try
        {
            double newTimeRange;
            Console.Write("Enter new Max Delivery Time Range (in days): ");
            while (true)
            {
                if (double.TryParse(Console.ReadLine(), out newTimeRange))
                {
                    s_dal!.Config.MaxDeliveryTimeRange = TimeSpan.FromDays(newTimeRange);
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid input.");
                    Console.Write("Enter new Max Delivery Time Range (in days): ");
                }

            }
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error changing Max Delivery Time Range: {ex.Message}");
        }

    }
    /// <summary>
    /// Prompts the user for a new risk time range.
    /// It validates the input as a <see cref="TimeSpan"/> and updates the configuration upon success.
    /// </summary>
    private static void ChangeRiskTimeRange()
    {
        try
        {
            double newTimeRange;
            Console.Write("Enter new Risk Time Range in days");
            while (true)
            {
                if (double.TryParse(Console.ReadLine(), out newTimeRange))
                {
                    s_dal!.Config.RiskTimeRange = TimeSpan.FromDays(newTimeRange);
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid input.");
                    Console.Write("Enter new Risk Time Range  ");
                }

            }
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error changing Risk Time Range: {ex.Message}");
        }

    }
    /// <summary>
    /// Prompts the user for a new inactivity time range.
    /// It validates the input as a <see cref="TimeSpan"/> and updates the configuration upon success.
    /// </summary>
    private static void ChangeInactivityTimeRange()
    {
        try
        {
            double newTimeRange;
            Console.Write("Enter new Inactivity Time Range (in days): ");
            while (true)
            {
                if (double.TryParse(Console.ReadLine(), out newTimeRange))
                {
                    s_dal!.Config.InactivityTimeRange = TimeSpan.FromDays(newTimeRange); ;
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid input.");
                    Console.Write("Enter new Inactivity Time Range (in days): ");
                }

            }
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error changing Inactivity Time Range: {ex.Message}");
        }

    }

    /// <summary>
    /// Displays a menu of all available configuration variables to change.
    /// It prompts the user for a choice and then calls the specific handler method
    /// (<c>Change...</c> function) to set the new value for the selected variable.
    /// </summary>
    ///This code block, which includes the 'switch' statement logic and initial data structure definitions, was created using an artificial intelligence(AI) model.
    private static void ChangeConfigMenu()
    {
        Console.WriteLine("Please choose a configuration value to change:");
        Console.WriteLine("0 - Clock");
        Console.WriteLine("1 - Manager ID");
        Console.WriteLine("2 - Manager Password");
        Console.WriteLine("3 - Company Address");
        Console.WriteLine("4 - Latitude");
        Console.WriteLine("5 - Longitude");
        Console.WriteLine("6 - Max Air Delivery Distance (km)");
        Console.WriteLine("7 - Average Motorcycle Speed (km/h)");
        Console.WriteLine("8 - Average Bicycle Speed (km/h)");
        Console.WriteLine("9 - Average Car Speed (km/h)");
        Console.WriteLine("10 - Average Walking Speed (km/h)");
        Console.WriteLine("11 - Max Delivery Time Range");
        Console.WriteLine("12 - Risk Time Range");
        Console.WriteLine("13 - Inactivity Time Range");
        Console.Write("\nEnter your choice (0–13): ");
        int choice;
        while (true)
        {
            if (!int.TryParse(Console.ReadLine(), out choice) ||
            !Enum.IsDefined(typeof(ConfigValues), choice))
            {
                Console.WriteLine("Invalid choice.");
                Console.WriteLine("Please choose a configuration value to change:");
            }
            else
                break;
        }

        {
            ConfigValues selected = (ConfigValues)choice;

            switch (selected)
            {
                case ConfigValues.Clock:
                    ChangeClock();
                    break;

                case ConfigValues.MangerId:
                    ChangeManagerId();
                    break;

                case ConfigValues.MangerPassword:
                    ChangeManagerPassword();
                    break;

                case ConfigValues.AddressCompany:
                    ChangeCompanyAddress();
                    break;

                case ConfigValues.Latitude:
                    ChangeLatitude();
                    break;

                case ConfigValues.Longitude:
                    ChangeLongitude();
                    break;

                case ConfigValues.MaxAirDeliveryDistanceKm:
                    ChangeMaxAirDeliveryDistance();
                    break;

                case ConfigValues.AverageMotorcycleSpeedKmh:
                    ChangeMotorcycleSpeed();
                    break;

                case ConfigValues.AverageBicycleSpeedKmh:
                    ChangeBicycleSpeed();
                    break;

                case ConfigValues.AverageCarSpeedKmh:
                    ChangeCarSpeed();
                    break;

                case ConfigValues.AverageWalkingSpeedKmh:
                    ChangeWalkingSpeed();
                    break;

                case ConfigValues.MaxDeliveryTimeRange:
                    ChangeMaxDeliveryTimeRange();
                    break;

                case ConfigValues.RiskTimeRange:
                    ChangeRiskTimeRange();
                    break;

                case ConfigValues.InactivityTimeRange:
                    ChangeInactivityTimeRange();
                    break;
            }
        }

    }
    /// <summary>
    /// Displays a menu of all available configuration variables for viewing.
    /// It prompts the user for a choice and then prints the current value of
    /// the selected configuration variable to the console.
    /// </summary>
    ///This code block, which includes the 'switch' statement logic and initial data structure definitions, was created using an artificial intelligence(AI) model.
    private static void HandleGetConfig()
    {
        Console.WriteLine("Please choose a configuration value you want to watch change:");
        Console.WriteLine("0 - Clock");
        Console.WriteLine("1 - Manager ID");
        Console.WriteLine("2 - Manager Password");
        Console.WriteLine("3 - Company Address");
        Console.WriteLine("4 - Latitude");
        Console.WriteLine("5 - Longitude");
        Console.WriteLine("6 - Max Air Delivery Distance (km)");
        Console.WriteLine("7 - Average Motorcycle Speed (km/h)");
        Console.WriteLine("8 - Average Bicycle Speed (km/h)");
        Console.WriteLine("9 - Average Car Speed (km/h)");
        Console.WriteLine("10 - Average Walking Speed (km/h)");
        Console.WriteLine("11 - Max Delivery Time Range");
        Console.WriteLine("12 - Risk Time Range");
        Console.WriteLine("13 - Inactivity Time Range");
        Console.Write("\nEnter your choice (0–13): ");
        int choice;
        while (true)
        {
            if (!int.TryParse(Console.ReadLine(), out choice) ||
            !Enum.IsDefined(typeof(ConfigValues), choice))
            {
                Console.WriteLine("Invalid choice.");
                Console.WriteLine("Please choose a configuration value to change:");
            }
            else
                break;
        }
        ConfigValues selected = (ConfigValues)choice;
        try
        {
            switch (selected)
            {
                case ConfigValues.Clock:
                    Console.WriteLine("Current System Clock Time:");
                    Console.WriteLine(s_dal!.Config.Clock);
                    Console.WriteLine(s_dal!.Config.Clock); // Kept redundant second print from original code
                    break;

                case ConfigValues.MangerId:
                    Console.WriteLine("Manager ID:");
                    Console.WriteLine(s_dal!.Config.ManagerId);
                    break;

                case ConfigValues.MangerPassword:
                    Console.WriteLine("Manager Password:");
                    Console.WriteLine(s_dal!.Config.ManagerPassword);
                    break;

                case ConfigValues.AddressCompany:
                    Console.WriteLine("Company Address:");
                    Console.WriteLine(s_dal!.Config.AddressCompany);
                    break;

                case ConfigValues.Latitude:
                    Console.WriteLine("Company Latitude:");
                    Console.WriteLine(s_dal!.Config.Latitude);
                    break;

                case ConfigValues.Longitude:
                    Console.WriteLine("Company Longitude:");
                    Console.WriteLine(s_dal!.Config.Longitude);
                    break;

                case ConfigValues.MaxAirDeliveryDistanceKm:
                    Console.WriteLine("Maximum Air Delivery Distance (Km):");
                    Console.WriteLine(s_dal!.Config.MaxAirDeliveryDistanceKm);
                    break;

                case ConfigValues.AverageMotorcycleSpeedKmh:
                    Console.WriteLine("Average Motorcycle Speed (Km/h):");
                    Console.WriteLine(s_dal!.Config.AverageMotorcycleSpeedKmh);
                    break;

                case ConfigValues.AverageBicycleSpeedKmh:
                    Console.WriteLine("Average Bicycle Speed (Km/h):");
                    Console.WriteLine(s_dal!.Config.AverageBicycleSpeedKmh);
                    break;

                case ConfigValues.AverageCarSpeedKmh:
                    Console.WriteLine("Average Car Speed (Km/h):");
                    Console.WriteLine(s_dal!.Config.AverageCarSpeedKmh);
                    break;

                case ConfigValues.AverageWalkingSpeedKmh:
                    Console.WriteLine("Average Walking Speed (Km/h):");
                    Console.WriteLine(s_dal!.Config.AverageWalkingSpeedKmh);
                    break;

                case ConfigValues.MaxDeliveryTimeRange:
                    Console.WriteLine("Maximum Allowed Delivery Duration:");
                    Console.WriteLine(s_dal!.Config.MaxDeliveryTimeRange);
                    break;

                case ConfigValues.RiskTimeRange:
                    Console.WriteLine("Risk Time Range:");
                    Console.WriteLine(s_dal!.Config.RiskTimeRange);
                    break;

                case ConfigValues.InactivityTimeRange:
                    Console.WriteLine("Inactivity Time Range:");
                    Console.WriteLine(s_dal!.Config.InactivityTimeRange);
                    break;
            }
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"Error retrieving configuration value: {ex.Message}");
        }



    }


    /// <summary>
    /// Resets all configuration variables held in the static configuration object
    /// (<c>s_dalConfig</c>) to their default values by calling the internal <c>Reset()</c> method.
    /// </summary>
    private static void HandleResetConfig()
    {
        s_dal!.Config.Reset();
        Console.WriteLine(" All configuration variables have been reset.");
    }


    /// <summary>
    /// The main entry point for the application.
    /// It calls the primary application menu (<c>ShowMainMenu</c>) wrapped in a global
    /// exception handler to catch and display any unexpected exceptions.
    /// </summary>
    /// <param name="args">Command line arguments passed to the application.</param>
    public static void Main(string[] args)
    {
            ShowMainMenu();
    }
}




