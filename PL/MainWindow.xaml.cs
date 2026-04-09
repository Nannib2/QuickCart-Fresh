
using BO;
using PL.Courier;
using PL.Helpers;
using PL.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; // Required for Task.Run (Async)
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PL;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// Represents the main administrative dashboard.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Reference to the Business Logic layer.
    /// </summary>
    static readonly BLApi.IBl s_bl = BLApi.Factory.Get();

    /// <summary>
    /// Stores the original password hash loaded from the BL.
    /// Used to determine if the user kept the old password or typed a new one.
    /// </summary>
    private string _originalHash = "";

    public int reqesterId = 0;

    /// <summary>
    /// Header text for the UI binding.
    /// </summary>

    // ----------------------------------------------------------------------------
    // Dependency Properties
    // ----------------------------------------------------------------------------

    /// <summary>
    /// Gets or sets the current simulation time.
    /// </summary>
    public DateTime CurrentTime
    {
        get { return (DateTime)GetValue(CurrentTimeProperty); }
        set { SetValue(CurrentTimeProperty, value); }
    }

    public static readonly DependencyProperty CurrentTimeProperty =
        DependencyProperty.Register("CurrentTime", typeof(DateTime), typeof(MainWindow), new PropertyMetadata(DateTime.Now));

    /// <summary>
    /// Gets or sets the configuration object for data binding.
    /// </summary>
    public BO.Config Configuration
    {
        get { return (BO.Config)GetValue(ConfigurationProperty); }
        set { SetValue(ConfigurationProperty, value); }
    }

    public static readonly DependencyProperty ConfigurationProperty =
        DependencyProperty.Register("Configuration", typeof(BO.Config), typeof(MainWindow), new PropertyMetadata(null));

    /// <summary>
    /// Controls whether the input fields are editable.
    /// </summary>
    public bool IsEditingEnabled
    {
        get { return (bool)GetValue(IsEditingEnabledProperty); }
        set { SetValue(IsEditingEnabledProperty, value); }
    }

    public static readonly DependencyProperty IsEditingEnabledProperty =
        DependencyProperty.Register("IsEditingEnabled", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));


    // view Property - order summary
    public IEnumerable<object> OrderSummaryDisplay
    {
        get { return (IEnumerable<object>)GetValue(OrderSummaryDisplayProperty); }
        set { SetValue(OrderSummaryDisplayProperty, value); }
    }

    public static readonly DependencyProperty OrderSummaryDisplayProperty =
        DependencyProperty.Register("OrderSummaryDisplay", typeof(IEnumerable<object>), typeof(MainWindow), new PropertyMetadata(null));

    /// <summary>
    /// Flag indicating if the simulator is currently active.
    /// </summary>
    public bool IsSimulatorRunning
    {
        get { return (bool)GetValue(IsSimulatorRunningProperty); }
        set { SetValue(IsSimulatorRunningProperty, value); }
    }

    public static readonly DependencyProperty IsSimulatorRunningProperty =
        DependencyProperty.Register("IsSimulatorRunning", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

    /// <summary>
    /// The interval (in seconds) for the simulator clock progression.
    /// Bound to the TextBox in the UI.
    /// </summary>
    public int SimulatorInterval
    {
        get { return (int)GetValue(SimulatorIntervalProperty); }
        set { SetValue(SimulatorIntervalProperty, value); }
    }

    public static readonly DependencyProperty SimulatorIntervalProperty =
        DependencyProperty.Register("SimulatorInterval", typeof(int), typeof(MainWindow), new PropertyMetadata(1)); // Default 1 minute


    // ----------------------------------------------------------------------------
    // Constructor & Window Events
    // ----------------------------------------------------------------------------


    /// <summary>
    /// Handles the window loaded event. Initializes data and registers observers.
    /// </summary>
    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        RefreshClock();
        LoadConfiguration(); // Use the smart load method to handle passwords
        RefreshOrderSummary();  // load order summary view

        s_bl.Admin.AddClockObserver(clockObserver);
        s_bl.Admin.AddClockObserver(orderSummaryObserver); //when clock changes, order summary may change too
        s_bl.Admin.AddConfigObserver(configObserver);
        s_bl.Admin.AddConfigObserver(orderSummaryObserver); //for intalizition and reset
        s_bl.Order.AddObserver(orderSummaryObserver);
    }

    /// <summary>
    /// Handles the window closed event. Unregisters observers.
    /// </summary>
    private void MainWindow_Closed(object sender, EventArgs e)
    {

        // Stop simulator if running ensuring clean exit
        if (IsSimulatorRunning)
        {
        if(  MessageBox.Show("are you sure you want to exit program? the simulatot will stop!", "Simulator", MessageBoxButton.OK, MessageBoxImage.Information) == MessageBoxResult.OK)

            s_bl.Admin.StopSimulator();
        }

        s_bl.Admin.RemoveClockObserver(clockObserver);
        s_bl.Admin.RemoveConfigObserver(configObserver);
        s_bl.Admin.RemoveConfigObserver(orderSummaryObserver); //for intalizition and reset
        s_bl.Order.RemoveObserver(orderSummaryObserver);

    }

    // ----------------------------------------------------------------------------
    // Configuration & Password Logic
    // ----------------------------------------------------------------------------

    /// <summary>
    /// Loads the configuration from the BL, stores the original password hash,
    /// and clears the password field for the UI.
    /// </summary>
    private void LoadConfiguration()
    {
            var config = s_bl.Admin.GetConfig();

            // 1. Save the original hash securely
            _originalHash = config.ManagerPassword;

            // 2. Clear the password in the object so the UI box appears empty
            config.ManagerPassword = "";

            Configuration = config;
    }

    /// <summary>
    /// Recursively validates all child controls in the visual tree.
    /// </summary>
    /// <param name="parent">The parent dependency object to start the validation from (usually 'this').</param>
    /// <returns>True if all controls are valid; otherwise, false.</returns>
    private bool IsValid(DependencyObject parent)
    {
        // Check if the current control itself has a validation error
        if (Validation.GetHasError(parent))
        {
            return false;
        }

        // Traverse the visual tree to check all child elements
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            // Get the child element at the specific index
            DependencyObject child = VisualTreeHelper.GetChild(parent, i);

            // Recursively check if the child or its descendants have errors
            if (!IsValid(child))
            {
                return false;
            }
        }

        // If no errors were found in this branch of the tree, return true
        return true;
    }

    /// <summary>
    /// Handles the "Save Changes" button click. 
    /// Checks if the password was changed and updates the BL.
    /// </summary>
    private async void btnUpdateConfig_Click(object sender, RoutedEventArgs e)
    {
        if (!IsValid(this))
        {
            MessageBox.Show("Please correct the highlighted errors before saving.",
                            "Validation Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
            return; // Stop execution if UI validation fails
        }
        try
        {
      
            // Logic: If the password field is empty, the user wants to keep the old password.
            if (string.IsNullOrEmpty(Configuration.ManagerPassword))
            {
                // Restore the original hash
                Configuration.ManagerPassword = _originalHash;
            }
           

           await s_bl.Admin.SetConfig(Configuration);

            this.IsEditingEnabled = false;
            MessageBox.Show("The configuration was updated successfully.", "Update", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (BlInvalidInputException ex)
        {
            MessageBox.Show($"Error updating configuration: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Enables editing mode.
    /// </summary>
    private void btnEditValues_Click(object sender, RoutedEventArgs e)
    {
        this.IsEditingEnabled = true;
        MessageBox.Show("You have entered editing mode. Don't forget to save your changes!", "Configuration Edit", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    /// <summary>
    /// Cancels editing mode and reverts changes.
    /// </summary>
    private void btnCancelChanges_Click(object sender, RoutedEventArgs e)
    {
        if (this.IsEditingEnabled)
        {
            LoadConfiguration(); // Reloading from BL reverts everything
            this.IsEditingEnabled = false;
            MessageBox.Show("Changes canceled. Reverted to original state.", "Cancel", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show("You are not currently in editing mode.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    /// Mutex to prevent concurrent configuration updates
    private readonly ObserverMutex _configMutex = new(); //stage 7

    /// <summary>
    /// Observer callback for configuration changes.
    /// </summary>
    private void configObserver()
    {
        if (_configMutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;
        this.Dispatcher.BeginInvoke(async() =>
        {
            LoadConfiguration();
            // After completing the work, check if a restart was requested
            if (await _configMutex.UnsetLoadInProgressAndCheckRestartRequested())
                configObserver();
        });
    }

    // Clock Logic (Async)
    /// <summary>
    /// Reads the current clock from the BL and updates the property.
    /// </summary>
    private void RefreshClock()
    {
     CurrentTime = s_bl.Admin.GetClock();
    }

    /// <summary>
    /// Advances the simulation clock asynchronously.
    /// Uses a background thread to prevent the UI from freezing.
    /// </summary>
    /// <param name="unit">The time unit to advance by.</param>
    private async void AdvanceTime(BO.TimeUnit unit)
    {
        try
        {
            // Show wait cursor
            // this.Cursor = Cursors.Wait;
            Mouse.OverrideCursor = Cursors.Wait; //to be more quick

            // Run heavy logic on background thread
            await Task.Run(() => s_bl.Admin.ForwardClock(unit));

            // Update UI after completion
            //RefreshClock();
        }
        catch (BO.BlDoesNotExistException ex)
        {
            MessageBox.Show($"Error advancing time: {ex.Message}", "Clock Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            // Restore cursor
            //this.Cursor = Cursors.Arrow;
            Mouse.OverrideCursor = null; //to be more quick
        }
    }

    // Button Event Handlers
    private void btnAddOneMinute_Click(object sender, RoutedEventArgs e) => AdvanceTime(BO.TimeUnit.Minute);
    private void btnAddOneHour_Click(object sender, RoutedEventArgs e) => AdvanceTime(BO.TimeUnit.Hour);
    private void btnAddOneDay_Click(object sender, RoutedEventArgs e) => AdvanceTime(BO.TimeUnit.Day);
    private void btnAddOneMonth_Click(object sender, RoutedEventArgs e) => AdvanceTime(BO.TimeUnit.Month);
    private void btnAddOneYear_Click(object sender, RoutedEventArgs e) => AdvanceTime(BO.TimeUnit.Year);

    /// Mutex to prevent concurrent clock updates
    private readonly ObserverMutex _clockMutex = new(); //stage 7

    /// <summary>
    /// Observer callback for clock updates.
    /// </summary>
    private void clockObserver()
    {
        if (_clockMutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;
        this.Dispatcher.BeginInvoke(async() =>
        {
            RefreshClock();
            // After completing the work, check if a restart was requested
            if (await _clockMutex.UnsetLoadInProgressAndCheckRestartRequested())
                clockObserver();

        });
    }

    // ----------------------------------------------------------------------------
    // Database Operations (Async) & Navigation
    // ----------------------------------------------------------------------------

    /// <summary>
    /// Closes all windows except the main window.
    /// </summary>
    //private void CloseAllSecondaryWindows()
    //{
    //    Window mainWindow = this;
    //    for (int i = System.Windows.Application.Current.Windows.Count - 1; i >= 0; i--)
    //    {
    //        Window window = System.Windows.Application.Current.Windows[i];
    //        if (window != mainWindow)
    //        {
    //            window.Close();
    //        }
    //    }
    //}

    // ----------------------------------------------------------------------------
    // handling Order Summary View
    // ---------------------------------------------------------------------------- 

    /// <summary>
    /// Refresh Order Summary 
    /// </summary>
    private void RefreshOrderSummary()
    {
        int[] rawData = s_bl.Order.GetOrderSummary(reqesterId);

        int scheduleCount = Enum.GetValues(typeof(BO.ScheduleStatus)).Length; 

        var summary = Enum.GetValues(typeof(BO.OrderStatus)).Cast<BO.OrderStatus>().Select(os =>
        {
            int baseIdx = (int)os * scheduleCount;
            return new
            {
                StatusName = os.ToString(),
                OnTimeCount = rawData[baseIdx + (int)BO.ScheduleStatus.OnTime],
                InRiskCount = rawData[baseIdx + (int)BO.ScheduleStatus.InRisk],
                LateCount = rawData[baseIdx + (int)BO.ScheduleStatus.Late],

                // when the manager clicks on the count, we need to know which status and schedule it is
                OnTimeTag = $"{os},OnTime",
                InRiskTag = $"{os},InRisk",
                LateTag = $"{os},Late"
            };
        }).ToList();

        OrderSummaryDisplay = summary;
    }

    /// <summary>
    /// click on order count to open orders list with filter
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OrderCount_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button != null && button.Tag != null)
        {
            string[] parts = button.Tag.ToString()!.Split(',');
            string orderStatus = parts[0];
            string scheduleStatus = parts[1];

            // open orders list window with filter
            if (Enum.TryParse(orderStatus, out BO.OrderStatus orderStatusResult)  && Enum.TryParse(scheduleStatus, out BO.ScheduleStatus scheduleStatusResult)) 
            {
                MessageBox.Show($"Opening Orders List with filter: Order Status = {orderStatusResult}, Schedule Status = {scheduleStatusResult}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
               // this.Hide(); //hide main window
                OpenOrFocusWindow(() => new OrdersListWindow(this,reqesterId,orderStatusResult, scheduleStatusResult));
            }
        }
    }

    //mutex to prevent concurrent order summary updates
    private readonly ObserverMutex _orderSummaryMutex = new(); //stage 7

    /// <summary>
    /// observer callback for order summary updates.
    /// </summary>
    private void orderSummaryObserver()
    {
        if (_orderSummaryMutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;
        this.Dispatcher.BeginInvoke(async() =>
        {
            RefreshOrderSummary();
            // After completing the work, check if a restart was requested
            if (await _orderSummaryMutex.UnsetLoadInProgressAndCheckRestartRequested())
                orderSummaryObserver();

        });
    }

    /// <summary>
    /// Handles the "Reset Database" button click asynchronously.
    /// </summary>
    private void btnReset_Click(object sender, RoutedEventArgs e)
    {
        var previousCursor = this.Cursor;
        try
        {
            if (this.IsEditingEnabled)
            {
                MessageBox.Show("Please save changes first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Are you sure you want to reset the database? This action cannot be undone.", "Confirm Reset", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                //await PerformDbActionAsync(() => s_bl.Admin.ResetDB());
                this.Cursor = Cursors.Wait;
                s_bl.Admin.ResetDB();
                MessageBox.Show("Action completed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (BO.BlInvalidInputException ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (BO.BlDoesNotExistException ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            this.Cursor = previousCursor;
        }
    }

    /// <summary>
    /// Handles the "Initialize Database" button click asynchronously.
    /// </summary>
    private void btninitialization_Click(object sender, RoutedEventArgs e)
    {
        var previousCursor = this.Cursor;
        try
        {
            if (this.IsEditingEnabled)
            {
                MessageBox.Show("Please save changes first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (MessageBox.Show("Are you sure you want to initialize the database?", "Confirm Initialization", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                //await PerformDbActionAsync(() => s_bl.Admin.InitializeDB());
                this.Cursor = Cursors.Wait;
                s_bl.Admin.InitializeDB();
                 MessageBox.Show("Action completed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
             }
        }
        catch (BO.BlFailedToGenerate ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (BO.BlXMLFileLoadCreateException ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (FormatException ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (BO.BlInvalidInputException ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (BO.BlDoesNotExistException ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch(Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            this.Cursor = previousCursor;
        }
    }

    /// <summary>
    /// Helper method to execute heavy database actions on a background thread.
    /// Handles UI state (cursor), closes windows, and refreshes data upon completion.
    /// </summary>
    /// <param name="action">The database action to perform.</param>
        //private async Task PerformDbActionAsync(Action action)
        //{
        //    var previousCursor = this.Cursor;
        //    this.Cursor = Cursors.Wait;
        //    //CloseAllSecondaryWindows();

        //    try
        //    {
        //        await Task.Run(() =>
        //        {
        //            action();
        //           // System.Threading.Thread.Sleep(1000); // Small delay for UX/Synchronization
        //        });

        //        MessageBox.Show("Action completed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

        //        // Refresh all data after DB change
        //        RefreshClock();
        //        LoadConfiguration();
        //    }
        //    catch (BO.BlInvalidInputException ex)
        //    {
        //        MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //    catch (BO.BlDoesNotExistException ex)
        //    {
        //        MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //    catch (BO.BlFailedToGenerate ex)
        //    {
        //        MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //    catch (BO.BlXMLFileLoadCreateException ex)
        //    {

        //        MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //    catch (FormatException ex)
        //    {
        //        MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //    finally
        //    {
        //        this.Cursor = previousCursor;
        //    }
        //}

        /// <summary>
        /// Opens the Courier List window.
        /// </summary>
    private void btnShowCouriersInList_Click(object sender, RoutedEventArgs e)
    {
        //this.Hide(); //hide main window when opening courier list
        OpenOrFocusWindow(() => new CourierListWindow(reqesterId, this));
    }

    /// <summary>
    /// Opens the Orders List window.
    /// </summary>
    private void btnShowOrdersInList_Click(object sender, RoutedEventArgs e)
    {
        //this.Hide(); // Hide main window when opening orders list
        OpenOrFocusWindow(() => new OrdersListWindow(this,reqesterId));
    }


    /// <summary>
    /// Helper method to ensure only one instance of a specific window type is open.
    /// If the window is already open, it brings it to the front and focuses it.
    /// If not, it creates and shows a new instance.
    /// </summary>
    /// <typeparam name="T">The type of the Window to manage.</typeparam>
    private void OpenOrFocusWindow<T>(Func<T> windowFactory) where T : Window
    {
        // 1. Check if an instance of this window type is already open
        var existingWindow = Application.Current.Windows.OfType<T>().FirstOrDefault();

        if (existingWindow != null)
        {
            // 2. If found, bring it to the front (Restore if minimized)
            if (existingWindow.WindowState == WindowState.Minimized)
                existingWindow.WindowState = WindowState.Normal;

            existingWindow.Activate();
            existingWindow.Focus();
        }
        else
        {
            // 3. If not found, create it using the provided factory and show it
            T newWindow = windowFactory();
            newWindow.Show();
        }
    }

    /// <summary>
    /// main window constructor.
    /// </summary>
    public MainWindow(int id)
    {
        reqesterId = id;
        InitializeComponent();
    }


    //---------------simolator logic----------------------
    /// <summary>
    /// Handles the Simulator Start/Stop toggle button.
    /// </summary>
    private void btnToggleSimulator_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!IsSimulatorRunning)
            {
                // Start the Simulator
                int interval = SimulatorInterval;
                if (interval <= 0)
                {
                    MessageBox.Show("Please enter a positive interval.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                s_bl.Admin.StartSimulator(interval); // Stage 7 call
                IsSimulatorRunning = true;
            }
            else
            {
                // Stop the Simulator
                s_bl.Admin.StopSimulator(); // Stage 7 call
                IsSimulatorRunning = false;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Simulator Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            // Safety: Ensure flag is reset if start fails
            if (!IsSimulatorRunning) IsSimulatorRunning = false;
        }
    }
    }