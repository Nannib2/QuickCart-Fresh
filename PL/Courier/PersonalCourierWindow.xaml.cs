using BO;
using PL.Helpers;
using PL.Order;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PL.Courier;

/// <summary>
/// Interaction logic for PersonalCourierWindow.xaml
/// </summary>
public partial class PersonalCourierWindow : Window
{
    // Static reference to the Business Logic layer API.
    static readonly BLApi.IBl s_bl = BLApi.Factory.Get();

    // Variable to hold the original hash of the courier data for change detection
    private string _originalHash = "";
    // Logged-in courier ID
    private readonly int _courierId;

    public BO.Courier? CurrentCourier
    {
        get { return (BO.Courier?)GetValue(CurrentCourierProperty); }
        set { SetValue(CurrentCourierProperty, value); }
    }

    public static readonly DependencyProperty CurrentCourierProperty =
        DependencyProperty.Register("CurrentCourier", typeof(BO.Courier), typeof(PersonalCourierWindow), new PropertyMetadata(null));

    public BO.OrderInProgress? CurrentOrderForCourier
    {
        get { return (BO.OrderInProgress?)GetValue(CurrentOrderForCourierProperty); }
        set { SetValue(CurrentOrderForCourierProperty, value); }
    }

    public static readonly DependencyProperty CurrentOrderForCourierProperty =
        DependencyProperty.Register("CurrentOrderForCourier", typeof(BO.OrderInProgress), typeof(PersonalCourierWindow), new PropertyMetadata(null));

    public bool IsReadMode
    {
        get { return (bool)GetValue(IsReadModeProperty); }
        set { SetValue(IsReadModeProperty, value); }
    }

    public static readonly DependencyProperty IsReadModeProperty =
        DependencyProperty.Register("IsReadMode", typeof(bool), typeof(PersonalCourierWindow), new PropertyMetadata(true));

    public BO.DeliveryCompletionType? SelectedCompletionType { get; set; } = null;

    // Helper property for ComboBox (assuming simple Enum binding)
    public Array DeliveryTypes { get; } = Enum.GetValues(typeof(BO.DeliveryTypeMethods));

    /// <summary>
    /// Constructor accepting the logged-in courier's ID.
    /// </summary>
    /// <param name="courierId">The ID of the courier to display.</param>
    public PersonalCourierWindow(int courierId)
    {
        InitializeComponent(); // Good practice to call this first to init UI components

        this.DataContext = this; // Set DataContext to self for binding
        _courierId = courierId;
        // Load the courier data using the passed ID
        LoadData(courierId);
    }

    private async void LoadData(int id)
    {
        try
        {
            var courier = await s_bl.Courier.Read(_courierId, _courierId);
            if (courier != null)
            {
                CurrentCourier = courier;
                _originalHash = courier.PasswordCourier;
                CurrentCourier.PasswordCourier = ""; // for view
                CurrentOrderForCourier = courier.OrderInProgress;
            }
        }
        catch (BO.BlDoesNotExistException ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Close(); // Close window if courier not found
        }
        catch (BO.BlInvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }
    }

    /// <summary>
    /// Validates that all required fields are filled.
    /// </summary>
    private bool ValidateFields()
    {
        if (CurrentCourier == null)
        {
            MessageBox.Show("Courier data is missing.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        if (string.IsNullOrWhiteSpace(CurrentCourier.NameCourier))
        {
            MessageBox.Show("Please enter the courier's name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        if (string.IsNullOrWhiteSpace(CurrentCourier.PhoneNumber))
        {
            MessageBox.Show("Please enter a phone number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        if (string.IsNullOrWhiteSpace(CurrentCourier.EmailCourier))
        {
            MessageBox.Show("Please enter an email address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        return true;
    }

    // --- Button Event Handlers ---

    private void btnEdit_Click(object sender, RoutedEventArgs e)
    {
        IsReadMode = false;
        MessageBox.Show("You are now in edit mode. Make your changes and click Save when done.", "Edit Mode", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
        try { 
        if (CurrentCourier != null)
        {
            LoadData(CurrentCourier.Id); // Reload to discard changes
            IsReadMode = true;
            MessageBox.Show("Changes have been discarded.", "Cancel Edit", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    
        }
            catch(BO.BLTemporaryNotAvailableException)
            {
                MessageBox.Show("The courier details are temporarily unavailable. Please try again later.", "Temporary Unavailability", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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
        return true;
    }

    /// <summary>
    /// save changes
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnSave_Click(object sender, RoutedEventArgs e)
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
            if (!ValidateFields()) return;

            // Restore original password if the field is empty (user didn't change it)
            if (string.IsNullOrEmpty(CurrentCourier!.PasswordCourier))
            {
                CurrentCourier.PasswordCourier = _originalHash;
            }

            s_bl.Courier.Update(CurrentCourier, _courierId);
            MessageBox.Show("Your details have been updated successfully.", "Update", MessageBoxButton.OK, MessageBoxImage.Information);
            _originalHash = CurrentCourier.PasswordCourier;
            CurrentCourier.PasswordCourier = "";
            IsReadMode = true;
        }
        catch (BO.BlDoesNotExistException ex) 
        {
            // Restore empty password field on error for UI consistency
            if (CurrentCourier != null && CurrentCourier.PasswordCourier == _originalHash)
            {
                CurrentCourier.PasswordCourier = "";
            }
            MessageBox.Show(ex.Message, "Doesn't Exist Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (BO.BlInvalidInputException ex) 
        {
            // Restore empty password field on error for UI consistency
            if (CurrentCourier != null && CurrentCourier.PasswordCourier == _originalHash)
            {
                CurrentCourier.PasswordCourier = "";
            }
            MessageBox.Show(ex.Message, "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (BO.BlInvalidOperationException ex) 
        {
            // Restore empty password field on error for UI consistency
            if (CurrentCourier != null && CurrentCourier.PasswordCourier == _originalHash)
            {
                CurrentCourier.PasswordCourier = "";
            }
            MessageBox.Show(ex.Message, "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    
            catch(BO.BLTemporaryNotAvailableException)
            {
                MessageBox.Show("Modifications are disabled while the simulation is active. The system is in read-only mode.", "Temporary Unavailability", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

    }

    private void btnShowSalary_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentCourier == null || CurrentCourier.SalaryForCourier == null)
        {
            MessageBox.Show("No salary details available.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var s = CurrentCourier.SalaryForCourier;
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

    private void btnFinish_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedCompletionType == null)
        {
            MessageBox.Show("Please select a completion status.", "Missing Data", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        try
        {
            s_bl.Order.FinishDeliveryHandling(CurrentCourier!.Id, CurrentCourier!.Id, CurrentOrderForCourier!.DeliveryId, SelectedCompletionType);
            // CurrentOrderForCourier = null; // Clear UI not ok, by observer
            MessageBox.Show("Delivery closed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (BO.BlInvalidOperationException ex)
        {
            MessageBox.Show($"Error finishing delivery: {ex.Message}", "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (BO.BlDoesNotExistException ex)
        {
            MessageBox.Show($"Error finishing delivery: {ex.Message}", "Doesn't Exist Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    
         catch(BO.BLTemporaryNotAvailableException)
        {
                MessageBox.Show("Modifications are disabled while the simulation is active. The system is in read-only mode.", "Temporary Unavailability", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void btnPickOrder_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentCourier != null)
        {
           // this.Hide(); //hide this window
            new SelectedOrderWindow(CurrentCourier.Id, this).Show();
        }
    }

    private void btnHistory_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentCourier != null)
        {
            //this.Hide();  //hide this window
            new CourierDeliveriesHistoryWindow(CurrentCourier.Id,this).Show();
        }
    }

    // --- Observer Logic ---

    // Mutex to prevent concurrent observer invocations
    private readonly ObserverMutex _CourierObserver = new(); //stage 7

    /// <summary>
    /// Observes changes to the current courier and updates the application state accordingly.
    /// </summary>
    /// <remarks>This method monitors the state of the currently selected courier and ensures that the
    /// application reflects any updates      or deletions made to the courier by other users. If the courier is
    /// deleted, the method displays a warning message and      closes the window. If the courier is updated, the method
    /// refreshes the courier's data and updates the associated order      view if necessary.</remarks>
    private void CourierObserver()
    {
        if (_CourierObserver.CheckAndSetLoadInProgressOrRestartRequired())
            return;
        Dispatcher.BeginInvoke(async() =>
        {
            try
            {
                
                var updatedCourier = await s_bl.Courier.Read(_courierId, _courierId);

                if (updatedCourier == null)
                {
                    MessageBox.Show("The courier was deleted.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Close();
                    return;
                }

                // update the CurrentCourier
                CurrentCourier = updatedCourier;

                // update active order
                CurrentOrderForCourier = updatedCourier.OrderInProgress;

             
            }
            catch (BO.BlDoesNotExistException ex)
            {
                // Handle deletion or read errors 
                MessageBox.Show(ex.Message, "Doesn't Exist Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }
            catch (BO.BlInvalidOperationException ex)
            {
                // Handle deletion or read errors
                MessageBox.Show(ex.Message, "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }
            finally
            {
                
                if (await _CourierObserver.UnsetLoadInProgressAndCheckRestartRequested())
                    CourierObserver();
            }
        });
    }

    /// <summary>
    /// loded the window
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PersonalCourierWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Safety check to ensure we have a courier before registering observer
        if (CurrentCourier != null)
        {
            s_bl.Courier.AddObserver(CurrentCourier.Id, CourierObserver);
            s_bl.Order.AddObserver(CurrentCourier.Id, CourierObserver);
            s_bl.Admin.AddClockObserver(CourierObserver);
        }
    }

    /// <summary>
    /// closed the window
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PersonalCourierWindow_Closed(object sender, EventArgs e)
    {
        if (CurrentCourier != null)
        {
            s_bl.Courier.RemoveObserver(CurrentCourier.Id, CourierObserver);
            s_bl.Order.RemoveObserver(CurrentCourier.Id, CourierObserver);
            s_bl.Admin.RemoveClockObserver(CourierObserver);
        }
    }
}