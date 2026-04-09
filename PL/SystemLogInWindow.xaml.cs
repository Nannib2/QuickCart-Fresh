using System;
using System.Windows;
using PL.Courier; // Ensures access to PersonalCourierWindow
// using PL.Order; // Uncomment if MainWindow is located in this namespace

namespace PL;

/// <summary>
/// Interaction logic for SystemLoginWindow.xaml.
/// Handles the authentication process for both Managers and Couriers, 
/// directing them to their respective dashboards upon successful login.
/// </summary>
public partial class SystemLoginWindow : Window
{
    /// <summary>
    /// Access to the Business Logic layer.
    /// </summary>
    static readonly BLApi.IBl s_bl = BLApi.Factory.Get();

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemLoginWindow"/> class.
    /// </summary>
    public SystemLoginWindow()
    {
        InitializeComponent();
    }

    /* ===================== Dependency Properties ===================== */

    /// <summary>
    /// Gets or sets the User ID entered by the user.
    /// This property is bound to the TextBox in the UI.
    /// </summary>
    public string EnteredId
    {
        get { return (string)GetValue(EnteredIdProperty); }
        set { SetValue(EnteredIdProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="EnteredId"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty EnteredIdProperty =
        DependencyProperty.Register(nameof(EnteredId), typeof(string), typeof(SystemLoginWindow), new PropertyMetadata(""));

    /// <summary>
    /// Gets or sets the Password entered by the user.
    /// This property is bound to the PasswordBox in the UI via the PasswordBoxHelper.
    /// </summary>
    public string EnteredPassword
    {
        get { return (string)GetValue(EnteredPasswordProperty); }
        set { SetValue(EnteredPasswordProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="EnteredPassword"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty EnteredPasswordProperty =
        DependencyProperty.Register(nameof(EnteredPassword), typeof(string), typeof(SystemLoginWindow), new PropertyMetadata(""));


    /* ===================== Logic ===================== */

    /// <summary>
    /// Handles the click event for the Login button.
    /// Validates the input, authenticates the user via the BL layer, and navigates to the appropriate window.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // 1. Basic Input Validation
            if (string.IsNullOrWhiteSpace(EnteredId))
            {
                MessageBox.Show("Please enter User ID.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(EnteredId, out int userId))
            {
                MessageBox.Show("User ID must be a number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(EnteredPassword))
            {
                MessageBox.Show("Please enter Password.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Authenticate against the Business Logic
            // The EnterSystem method verifies the ID and hashed password.
            // It returns "Manager" or "Courier" upon success, or throws an exception.
            string userType = s_bl.Courier.EnterSystem(userId, EnteredPassword);

            // 3. Navigate based on the returned user type
            if (userType == "Manager")
            {
                // Navigate directly to the main management dashboard
                if (int.TryParse(EnteredId, out int result))
                {
                    new MainWindow(result).Show();
                    ClearFields();
                }
            }
            else // Courier
            {
                // Navigate directly to the personal courier interface
                new Courier.PersonalCourierWindow(userId).Show();
                ClearFields();
            }
        }
        catch (BO.BlInvalidInputException ex)
        {
            MessageBox.Show(ex.Message, "Login Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (BO.BlDoesNotExistException ex)
        {
            MessageBox.Show(ex.Message, "Login Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (BO.BlInvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Clears the input fields (ID and Password) to allow a new login attempt
    /// without closing the window.
    /// </summary>
    private void ClearFields()
    {
        EnteredId = "";
        EnteredPassword = "";
    }
}