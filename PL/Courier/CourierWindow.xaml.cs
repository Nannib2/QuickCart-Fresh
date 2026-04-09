using BO;
using DalApi;
using PL.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PL.Courier;

/// <summary>
/// Interaction logic for CourierWindow.xaml.
/// This window handles the creation and updating of Courier entities.
public partial class CourierWindow : Window
{
    // Static reference to the Business Logic layer API.
    static readonly BLApi.IBl s_bl = BLApi.Factory.Get();
    private Window _previousWindow;
    public int idCourier;

    // var to hold the original hash of the courier data for change detection
    private string _originalHash = "";

    /// <summary>
    /// Gets the array of possible <see cref="BO.DeliveryTypeMethods"/> for binding to a ComboBox.
    /// </summary>
    public Array DeliveryTypes { get; } = Enum.GetValues(typeof(BO.DeliveryTypeMethods));

    /// <summary>
    /// Gets or sets the text displayed on the main action button (e.g., "Add" or "Update").
    /// </summary>
    public string ButtonText
    {
        get { return (string)GetValue(ButtonTextProperty); }
        set { SetValue(ButtonTextProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="ButtonText"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ButtonTextProperty =
    DependencyProperty.Register("ButtonText", typeof(string), typeof(CourierWindow), new PropertyMetadata(""));

    // Static configuration object retrieved from the Business Logic layer.
    static BO.Config con = s_bl.Admin.GetConfig();
    public int idManager = con.ManagerId;

    /// <summary>
    /// Gets or sets the currently displayed <see cref="BO.Courier"/> object for binding.
    /// </summary>
    public BO.Courier? CurrentCourier
    {
        get { return (BO.Courier?)GetValue(CurrentCourierProperty); }
        set { SetValue(CurrentCourierProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="CurrentCourier"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CurrentCourierProperty =
        DependencyProperty.Register("CurrentCourier", typeof(BO.Courier), typeof(CourierWindow), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets a value indicating whether the window is in "Update" mode (<see langword="true"/>) or "Add" mode (<see langword="false"/>).
    /// </summary>
    public bool IsUpdateMode
    {
        get { return (bool)GetValue(IsUpdateModeProperty); }
        set { SetValue(IsUpdateModeProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="IsUpdateMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsUpdateModeProperty =
        DependencyProperty.Register("IsUpdateMode", typeof(bool), typeof(CourierWindow), new PropertyMetadata(false));

   

    
    /// <summary>
    /// Validates that all required fields are filled.
    /// PersonalMaxAirDistance is excluded as per requirements.
    /// </summary>
    private bool ValidateFields()
    {
        if(CurrentCourier == null)
        {
            MessageBox.Show("Courier data is missing.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        //check each required field
        if (string.IsNullOrWhiteSpace(CurrentCourier.NameCourier))
        {
            MessageBox.Show("Please enter the courier's name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        // phone number check
        if (string.IsNullOrWhiteSpace(CurrentCourier.PhoneNumber))
        {
            MessageBox.Show("Please enter a phone number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        //email check
        if (string.IsNullOrWhiteSpace(CurrentCourier.EmailCourier))
        {
            MessageBox.Show("Please enter an email address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        //password check for new couriers
        if (!IsUpdateMode && string.IsNullOrWhiteSpace(CurrentCourier.PasswordCourier))
        {
            MessageBox.Show("Password is required for new couriers.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        return true; // everything is valid
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
    /// Handles the click event for the main "Add" or "Update" button.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void btnAddUpdate_Click(object sender, RoutedEventArgs e)
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

            if (CurrentCourier == null)
            {
                MessageBox.Show("Data is missing/corrupted.");
                return;
            }
            //create a copy of the current courier data to prevent rehashing password on update
            BO.Courier courierToUpdate = new BO.Courier()
            {
                DeliveryType = CurrentCourier.DeliveryType,
                EmailCourier = CurrentCourier.EmailCourier,
                Id = CurrentCourier.Id,
                IsActive = CurrentCourier.IsActive,
                NameCourier = CurrentCourier.NameCourier,
                PasswordCourier = CurrentCourier.PasswordCourier,
                PersonalMaxAirDistance = CurrentCourier.PersonalMaxAirDistance,
                PhoneNumber = CurrentCourier.PhoneNumber,
                StartDate = CurrentCourier.StartDate,
                OrderInProgress = CurrentCourier.OrderInProgress,
                SalaryForCourier = CurrentCourier.SalaryForCourier,
                TotalDeliveredOnTime = CurrentCourier.TotalDeliveredOnTime,
                TotalLateDeliveries = CurrentCourier.TotalLateDeliveries
            };

            //check that all required fields are filled
            if (!ValidateFields())
            {
                return; //if validation fails, exit the method
            }

            //handle password hashing logic
            if (IsUpdateMode && string.IsNullOrEmpty(courierToUpdate.PasswordCourier))
            {
                courierToUpdate.PasswordCourier = _originalHash;
            }

            
            if (!IsUpdateMode) // Create
            {
                s_bl.Courier.Create(courierToUpdate, idManager);
                MessageBox.Show("Courier was created successfully.", "Create", MessageBoxButton.OK, MessageBoxImage.Information);
                backToCourierLisstWindow();

            }
            else // Update
            {
                s_bl.Courier.Update(courierToUpdate, idManager);
                MessageBox.Show("Courier was updated successfully.", "Update", MessageBoxButton.OK, MessageBoxImage.Information);
                backToCourierLisstWindow();
            }

            
        }
        catch (BO.BlInvalidInputException ex)
        {
            
            if (IsUpdateMode && CurrentCourier != null && CurrentCourier.PasswordCourier == _originalHash)
            {
                CurrentCourier.PasswordCourier = "";
            }

            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (BO.BlInvalidOperationException ex)
        {

            if (IsUpdateMode && CurrentCourier != null && CurrentCourier.PasswordCourier == _originalHash)
            {
                CurrentCourier.PasswordCourier = "";
            }

            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (BO.BlNullPropertyException ex)
        {

            if (IsUpdateMode && CurrentCourier != null && CurrentCourier.PasswordCourier == _originalHash)
            {
                CurrentCourier.PasswordCourier = "";
            }

            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (BO.BlAlreadyExistsException ex)
        {

            if (IsUpdateMode && CurrentCourier != null && CurrentCourier.PasswordCourier == _originalHash)
            {
                CurrentCourier.PasswordCourier = "";
            }

            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (BO.BlDoesNotExistException ex)
        {

            if (IsUpdateMode && CurrentCourier != null && CurrentCourier.PasswordCourier == _originalHash)
            {
                CurrentCourier.PasswordCourier = "";
            }

            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (BO.BLTemporaryNotAvailableException)
        {
            MessageBox.Show("The courier details are temporarily unavailable. Please try again later.", "Temporary Unavailability", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    /// Mutex to synchronize observer callbacks.
    private readonly ObserverMutex _CourierMutex = new(); //stage 7
    /// <summary>
    /// The observer callback method used to refresh the courier data from the BL layer.
    /// </summary>
    /// <remarks>
    /// This method is called by the BL layer when the courier associated with this window is modified externally.
    /// It must use <see cref="Dispatcher.Invoke"/> to update the UI thread.
    /// </remarks>
    private void CourierObserver()
    {
        // Step 1: Mutex check (Thread-safe check to prevent concurrent UI updates)
        if (_CourierMutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;
        if (idCourier == 0) return;

        Dispatcher.BeginInvoke(async () =>
        {
            try
            {
                // Step 2: Fetch the updated courier data from Business Logic
                var updatedCourier = await s_bl.Courier.Read(idManager, idCourier);

                if (updatedCourier == null)
                {
                    MessageBox.Show("The courier was deleted by another user.", "Courier Deleted", MessageBoxButton.OK, MessageBoxImage.Warning);
                    s_bl.Courier.RemoveObserver(CourierObserver);
                    Close();
                    return;
                }

                // Step 3: Update the UI by setting the current courier object
                CurrentCourier = updatedCourier;
            }
            catch (BO.BlDoesNotExistException)
            {
                // Handle case where courier was deleted
                Close(); 
            }
            catch (BO.BlInvalidOperationException ex)
           {
               MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
             Close();
          }
            finally
            {
                if (await _CourierMutex.UnsetLoadInProgressAndCheckRestartRequested())
                    CourierObserver();
            }
        });
    }

    /// <summary>
    /// Handles the <see cref="Window.Loaded"/> event. Subscribes the <see cref="CourierObserver"/>
    /// to the BL layer for real-time updates if the window is in Update mode (ID != 0).
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private async void CourierWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Check if we are in update mode and the courier data hasn't been loaded yet
        if (IsUpdateMode && CurrentCourier == null)
        {
            try
            {
                // Asynchronously fetch courier details from the Business Logic layer
                CurrentCourier = await s_bl.Courier.Read(idManager, idCourier);

                if (CurrentCourier != null)
                {
                    // Store the original password hash for later verification and clear the field for UI security
                    _originalHash = CurrentCourier.PasswordCourier;
                    CurrentCourier.PasswordCourier = "";
                }
                else
                {
                    // Handle case where courier record is missing
                    MessageBox.Show("Courier not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return; 
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
                Close();
                return; 
            }
        }

        // Safety check before registering the Observer
        // The null-conditional operator (?.) ensures we don't access the Id if the object is null
        if (CurrentCourier?.Id != 0 && CurrentCourier != null)
        {
            s_bl.Courier.AddObserver(CurrentCourier.Id, CourierObserver);
        }
    }

    /// <summary>
    /// Handles the <see cref="Window.Closed"/> event. Unsubscribes the <see cref="CourierObserver"/>
    /// from the BL layer to prevent memory leaks and unnecessary calls.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void CourierWindow_Closed(object sender, EventArgs e)
    => s_bl.Courier.RemoveObserver(CurrentCourier!.Id, CourierObserver);


    /// <summary>
    /// Initializes a new instance of the <see cref="CourierWindow"/> class.
    /// </summary>
    /// <param name="id">The ID of the courier to display/update (0 for adding a new courier).</param>
    public CourierWindow( Window previousWindow, int id = 0)
    {
        try
        {
            InitializeComponent();
            _previousWindow=previousWindow;
            if (id != 0) // Update mode
            {
                IsUpdateMode = true;
                ButtonText = "Update Courier";
                idCourier = id;
            }
            else // Add mode
            {
                IsUpdateMode = false;
                ButtonText = "Add Courier";

                //create a new SalaryForCourier object with default values
                BO.SalaryForCourier salary = new BO.SalaryForCourier()
                {
                    Month = (BO.Months)s_bl.Admin.GetClock().Month,
                    Year = s_bl.Admin.GetClock().Year,
                    BaseSalary = s_bl.Admin.GetConfig().BaseSalaryMounthly,
                    TotalSalary = s_bl.Admin.GetConfig().BaseSalaryMounthly
                };

                CurrentCourier = new BO.Courier()
                {
                    //Id = 0,
                    IsActive = true,
                    StartDate = s_bl.Admin.GetClock(),
                    SalaryForCourier = salary,
                    PasswordCourier = "" // initialize password as empty
                };
            }
        }
        catch (BO.BlDoesNotExistException ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }
        catch (BO.BlInvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }
        catch (BO.BLTemporaryNotAvailableException)
        {
            MessageBox.Show("The courier details are temporarily unavailable. Please try again later.", "Temporary Unavailability", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        _previousWindow = previousWindow;
    }

    /// <summary>
    /// Navigates back to the main window.
    /// </summary>

    private void backToCourierLisstWindow()
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
        MessageBox.Show("Are you sure you want to exit the screen? The data will not be saved.","Exit",MessageBoxButton.OK,MessageBoxImage.Warning);
        backToCourierLisstWindow();
    }

    /// <summary>
    /// Handles the click event of the "Show Salary" button, displaying detailed salary information for the current
    /// courier.
    /// </summary>
    /// <remarks>If the <see cref="CurrentCourier"/> or its <see cref="SalaryForCourier"/> property is <see
    /// langword="null"/>, a message box is displayed indicating that no salary details are available. Otherwise, a
    /// detailed breakdown of the courier's salary, including base salary, bonuses, and total salary, is shown in a
    /// message box.</remarks>
    /// <param name="sender">The source of the event, typically the "Show Salary" button.</param>
    /// <param name="e">The event data associated with the button click.</param>
    private void btnShowSalary_Click(object sender, RoutedEventArgs e)
    {
        // validation to ensure CurrentCourier and its SalaryForCourier are not null
        if (CurrentCourier == null || CurrentCourier.SalaryForCourier == null)
        {
            if (CurrentCourier != null && !CurrentCourier.IsActive)
            { 
                MessageBox.Show("The courier is inactive. No salary details available.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
                MessageBox.Show("No salary details available.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var s = CurrentCourier.SalaryForCourier;

        // build the detailed salary string
        string details = $"Salary Statement for: {s.Month} {s.Year}\n" +
                         $"----------------------------------------------\n" +
                         $"Base Salary: \t\t{s.BaseSalary:N2} ₪\n" +
                         $"Distance Payment: \t{s.DistanceSalaryComponent:N2} ₪\n\n" +

                         $"Bonuses Breakdown:\n" +
                         $" • Volume Bonus: \t{s.DeliveryVolumeBonus:N2} ₪\n" +
                         $" • Performance Bonus: \t{s.PerformanceBonus:N2} ₪\n" +
                         $" • Seniority Bonus: \t{s.SeniorityBonus:N2} ₪\n" +
                         $"Total Bonuses: \t\t{s.TotalBonus:N2} ₪\n" +

                         $"==============================================\n" +
                         $"GRAND TOTAL: \t\t{s.TotalSalary:N2} ₪";

        MessageBox.Show(details, "Full Salary Details", MessageBoxButton.OK, MessageBoxImage.None);
    }

    /// <summary>
    /// Handles the click event for showing detailed order information.
    /// </summary>
    private void btnShowOrder_Click(object sender, RoutedEventArgs e)
    {
        // check if there is an order in progress
        if (CurrentCourier == null || CurrentCourier.OrderInProgress == null)
        {
            MessageBox.Show("There is no order currently in progress.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var o = CurrentCourier.OrderInProgress;

      
        string orderDetails =
            $"Order Details (ID: {o.OrderId})\n" +
            $"----------------------------------------------\n" +
            $"Customer: \t{o.CustomerName}\n" +
            $"Phone: \t\t{o.CustomerPhone}\n" +
            $"Address: \t{o.OrderAddress}\n\n" +

            $"Status & Timing:\n" +
            $" • Status: \t{o.OrderStatus}\n" +
            $" • Started: \t{o.DeliveryStartTime:dd/MM/yyyy HH:mm}\n" +
            $" • Expected: \t{o.ExpectedDeliveryTime:dd/MM/yyyy HH:mm}\n" +
            $" • Deadline: \t{o.MaxDeliveryTime:dd/MM/yyyy HH:mm}\n\n" +

            $"Distance:\n" +
            $" • Air Dist: \t{o.AirDistance:N2} km\n" +
            $" • Actual: \t{(o.ActualDistance.HasValue ? $"{o.ActualDistance:N2} km" : "N/A")}\n" +

            $"==============================================\n" +
            $"Remaining Time: {o.RemainingTime:hh\\:mm\\:ss}";

        //move to a message box to show the order details
        MessageBox.Show(orderDetails, "Order In Progress Details", MessageBoxButton.OK, MessageBoxImage.None);
    }

    /// <summary>
    /// Deletes the specific courier associated with the clicked button.
    /// </summary>
    private void btnDeleteCourier_Click(object sender, RoutedEventArgs e)
    {
            MessageBoxResult result = MessageBox.Show(
                $"Are you sure you want to delete courier ID {CurrentCourier.Id} ({CurrentCourier.NameCourier})?",
                "Confirm Deletion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    BO.Config con = s_bl.Admin.GetConfig();
                    int idManager = con.ManagerId;
                    s_bl.Courier.Delete(CurrentCourier.Id, idManager);

                    MessageBox.Show("Courier deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    backToCourierLisstWindow();
                }
                catch (BO.BlInvalidOperationException)
                {
                    MessageBox.Show("Could not delete the courier. It might be linked to active deliveries.", "Deletion Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (BO.BlNullPropertyException)
                {
                    MessageBox.Show("System error: Missing data for deletion.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (BO.BlDoesNotExistException)
                {
                    MessageBox.Show("The courier does not exist or was already deleted.", "Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            catch (BO.BLTemporaryNotAvailableException)
            {
                MessageBox.Show("The courier details are temporarily unavailable. Please try again later.", "Temporary Unavailability", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }
        }
    
};






