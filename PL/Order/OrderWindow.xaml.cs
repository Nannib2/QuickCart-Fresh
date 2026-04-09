using BO;
using PL.Courier;
using PL.Helpers;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PL.Order;

/// <summary>
/// Interaction logic for OrderWindow.xaml
/// this window is used for both adding a new order and updating an existing one.
/// </summary>
public partial class OrderWindow : Window
{
    static readonly BLApi.IBl s_bl = BLApi.Factory.Get();
    private Window _previousWindow;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderWindow"/> class with the specified order ID.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to be displayed in the window.  If set to 0, the window will initialize
    /// without loading a specific order.</param>
    public OrderWindow( Window previousWindow, int orderId = 0)
    {
        InitializeComponent();
        InitializeWindow(orderId);
        _previousWindow=previousWindow;
    }

    /* ===================== Dependency Properties (Data Binding) ===================== */

    //  CurrentOrder: object that holds the details of the order being added or updated
    public BO.Order CurrentOrder
    {
        get { return (BO.Order)GetValue(CurrentOrderProperty); }
        set { SetValue(CurrentOrderProperty, value); }
    }
    public static readonly DependencyProperty CurrentOrderProperty =
        DependencyProperty.Register(nameof(CurrentOrder), typeof(BO.Order), typeof(OrderWindow), new PropertyMetadata(null));

    // 2. IsAddMode: represents whether the window is in "Add" mode (true) or "Update" mode (false) 
    public bool IsAddMode
    {
        get { return (bool)GetValue(IsAddModeProperty); }
        set { SetValue(IsAddModeProperty, value); }
    }
    public static readonly DependencyProperty IsAddModeProperty =
        DependencyProperty.Register(nameof(IsAddMode), typeof(bool), typeof(OrderWindow), new PropertyMetadata(true));

    //  ActionButtonText:the text displayed on the main action button (Add or Update)
    public string ActionButtonText
    {
        get { return (string)GetValue(ActionButtonTextProperty); }
        set { SetValue(ActionButtonTextProperty, value); }
    }
    public static readonly DependencyProperty ActionButtonTextProperty =
        DependencyProperty.Register(nameof(ActionButtonText), typeof(string), typeof(OrderWindow), new PropertyMetadata("Add Order"));

    /* ===================== Initialization and logic ===================== */

    /// <summary>
    /// initializes the window based on whether it's in add or update mode.
    /// </summary>
    private void InitializeWindow(int orderId)
    {
            if (orderId == 0) //open in add mode
            {
                IsAddMode = true;
                ActionButtonText = "Add Order";

                //create a new order with default values
                CurrentOrder = new BO.Order
                {
                    Id = 0, 
                    OrderOpeningTime = s_bl.Admin.GetConfig().Clock,
                    OrderStatus = BO.OrderStatus.Open, //sta
                    AmountItems = 1,
                    FreeShippingEligibility = false,
                    DeliveryHistory = new List<BO.DeliveryPerOrderInList>() 
                };
            }
            else //update 
            {
                IsAddMode = false;
                ActionButtonText = "Update Order";
                LoadOrder(orderId); 
            }
    }

/// <summary>
/// Loads the details of an order with the specified ID and sets it as the current order.
/// </summary>
/// <remarks>This method retrieves the order details using the manager's configuration and sets the <see
/// cref="CurrentOrder"/> property. If the order does not exist or an invalid operation occurs, an error message is
/// displayed, and the window is closed.</remarks>
/// <param name="id">The unique identifier of the order to load.</param>
    private async Task LoadOrder(int id)
    {
        try
        {
            BO.Config config = s_bl.Admin.GetConfig();
            CurrentOrder = await s_bl.Order.Read(config.ManagerId, id);
        }
        catch (BO.BlDoesNotExistException ex)
        {
            MessageBox.Show($"Order with ID {id} does not exist: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }
        catch (BO.BlInvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }

    }

    /* ===================== Event Handlers & Observer ===================== */

    
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (!IsAddMode)
        {
            s_bl.Order.AddObserver(OrderObserver);
        }
    }

    
    private void Window_Closed(object sender, EventArgs e)
    {
        if (!IsAddMode)
        {
            s_bl.Order.RemoveObserver(OrderObserver);
        }
    }

    //mutex to prevent concurrent observer calls
    private readonly ObserverMutex _OrderMutex = new(); //stage 7

    /// <summary>
    /// Monitors the current order and updates its details asynchronously if it exists.
    /// </summary>
    /// <remarks>This method invokes an asynchronous operation on the UI thread to load the details of the
    /// current order. If the order no longer exists or an invalid operation occurs, appropriate error messages are
    /// displayed, and the window is closed.</remarks>
    private void OrderObserver()
    {
        if (_OrderMutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;
        if (CurrentOrder == null)
        {
            // Intentionally ignore the returned Task to suppress compiler warning.
            // We need to release the lock state, but we don't need to await the result here.
            _ = _OrderMutex.UnsetLoadInProgressAndCheckRestartRequested();
        }

        int orderId = CurrentOrder!.Id;
        Dispatcher.BeginInvoke(async() =>
        {
           
            try
            {
                if (CurrentOrder != null&& CurrentOrder.Id == orderId)
                    await LoadOrder(orderId);
               

            }
            catch (BO.BlDoesNotExistException)
            {
                
                MessageBox.Show("Order was deleted externally.", "Order Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
                return;
            }
            catch (BO.BlInvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }
            finally
            {
                // release the mutex in finally block
                if (await _OrderMutex.UnsetLoadInProgressAndCheckRestartRequested())
                {
                    OrderObserver();
                }
            }

        });
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
    /// Handles the click event for the "Action" button, performing either the creation or update of an order.
    /// </summary>
    /// <remarks>This method validates the current order's data before proceeding. If the order is in "Add
    /// Mode," it creates a new order; otherwise, it updates the existing order. Validation errors or other exceptions
    /// are displayed to the user via message boxes.</remarks>
    /// <param name="sender">The source of the event, typically the "Action" button.</param>
    /// <param name="e">The event data associated with the button click.</param>
    private async void btnAction_Click(object sender, RoutedEventArgs e)
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
            //validation
            if (CurrentOrder == null) return;

            if (string.IsNullOrWhiteSpace(CurrentOrder.CustomerFullName))
                throw new BO.BlInvalidInputException("Customer Name is required.");

            if (string.IsNullOrWhiteSpace(CurrentOrder.FullAddress))
                throw new BO.BlInvalidInputException("Address is required.");

            if (string.IsNullOrWhiteSpace(CurrentOrder.CustomerPhone))
                throw new BO.BlInvalidInputException("Phone is required.");

            if (CurrentOrder.AmountItems <= 0)
                throw new BO.BlInvalidInputException("Amount of items must be greater than 0.");

            //determine free shipping eligibility
            CurrentOrder.FreeShippingEligibility = CurrentOrder.AmountItems >= 20;

            BO.Config config = s_bl.Admin.GetConfig();

            //perform add or update operation
            if (IsAddMode)
            {
                await s_bl.Order.Create(CurrentOrder, config.ManagerId);
                MessageBox.Show("Order added successfully! Confirmation email sent to nearby couriers.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                backToOrderListWindow();
            }
            else
            {
                await s_bl.Order.Update(CurrentOrder, config.ManagerId);
                MessageBox.Show("Order updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                backToOrderListWindow();
            }
        }
        catch (BO.BlInvalidInputException ex)
        {
            MessageBox.Show($"Invalid Data: {ex.Message}", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (BO.BlInvalidOperationException ex)
        {
            MessageBox.Show($"Operation Failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (BO.BlDoesNotExistException ex)
        {
            MessageBox.Show($"Order Does Not Exist: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (BO.BLTemporaryNotAvailableException)
        {
            MessageBox.Show("Modifications are disabled while the simulation is active. The system is in read-only mode.", "Temporary Unavailability", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
       
        catch (BlExternalServiceException ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// cancel order in the list
    /// </summary>
    private void btnCancelOrder_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentOrder == null) return;

       
        var result = MessageBox.Show(
            $"Are you sure you want to cancel the Order?",
            "Confirm Cancellation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                BO.Config config = s_bl.Admin.GetConfig();
                
                s_bl.Order.CancelOrder(CurrentOrder.Id, config.ManagerId);

                MessageBox.Show("Order canceled successfully. Notification sent to courier (if assigned).", "Canceled", MessageBoxButton.OK, MessageBoxImage.Information);
                backToOrderListWindow();

            }
            catch (BO.BlInvalidOperationException ex)
            {
                MessageBox.Show($"Could not cancel order: {ex.Message}", "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (BO.BlDoesNotExistException ex)
            {
                MessageBox.Show($"Could not cancel order: {ex.Message}", "Dosn't Exist Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (BO.BLTemporaryNotAvailableException)
            {
                MessageBox.Show("Modifications are disabled while the simulation is active. The system is in read-only mode.", "Temporary Unavailability", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (BlExternalServiceException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        
    }

    // <summary>
    /// Navigates back to the main window.
    /// </summary>
    private void backToOrderListWindow()
    {
        _previousWindow.Show();
        this.Close();
    }
    /// <summary>
    /// back to Courierlist window button click event handler.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnBackMainWindow_Click(object sender, RoutedEventArgs e)
    {
        if(CurrentOrder != null && CurrentOrder.OrderStatus != OrderStatus.Canceled &&
            CurrentOrder.OrderStatus != OrderStatus.Delivered &&
            CurrentOrder.OrderStatus != OrderStatus.InProgress) 
            MessageBox.Show("Are you sure you want to exit the screen? The data will not be saved.", "Exit", MessageBoxButton.OK, MessageBoxImage.Warning);
        backToOrderListWindow();
    }
}