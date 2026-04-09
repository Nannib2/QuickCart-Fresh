using BO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace PL.Converters;

/// <summary>
/// Converts a button's text (string) into a boolean value, typically for use
/// as a ReadOnly property binding.
/// </summary>
/// <remarks>
/// Returns <c>true</c> if the input string is equal to "Update" (case-insensitive),
/// otherwise returns <c>false</c>.
/// </remarks>
public class ButtonTextToReadOnlyConverter : IValueConverter
{
    /// <summary>
    /// Converts a string value to a boolean based on whether it equals "Update".
    /// </summary>
    /// <param name="value">The value produced by the binding source (expected to be a string).</param>
    /// <param name="targetType">The type of the binding target property (expected to be boolean).</param>
    /// <param name="parameter">The converter parameter to use (not used).</param>
    /// <param name="culture">The culture to use in the converter (not used).</param>
    /// <returns><c>true</c> if the input string is "Update" (case-insensitive); otherwise, <c>false</c>.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string text)
            return text.Equals("Update", StringComparison.OrdinalIgnoreCase);

        return false;
    }

    /// <summary>
    /// Not implemented for this one-way converter.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// from BO.Courier.IsActive (bool) to status string converter.
/// </summary>
public class BoolIsActiveToStatusStringConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean value (IsActive) to a status string.
    /// </summary>
    /// <param name="value">The boolean value from BO.Courier.IsActive.</param>
    /// <returns>"Active Courier" if true, "NOT ACTIVE COURIER" if false.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isActive)
        {
            return isActive ? "Active Courier" : "Inactive Courier";
        }

        return "Unknown Status";
    }

    /// <summary>
    /// Not implemented for this one-way converter.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// edit mode and not null to boolean converter for enabling/disabling a ComboBox.
/// </summary>
public class EditModeAndNotNullToBooleanConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        // values[0] -IsReadMode (bool)
        // values[1] -CurrentOrderForCourier (object)

        if (values[0] is bool isReadMode && isReadMode == false) // edit mode
        {
            if (values[1] == null) // active order assigned
            {
                return true; // ComboBox is enabled
            }
        }
        return false; //not edit mode or no active order assigned
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// if orderInProgress is null show "no orders" message converter.
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// if orderInProgress is not null show order details converter.
/// </summary>
public class NotNullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        //if the value is not null, return Visible, else Collapsed
        return value != null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// inverse boolean converter. true => false, false => true.
/// </summary>
public class InverseBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue; // return the inverse
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return false;
    }
}


    /// <summary>
    /// A helper class that provides Attached Properties to enable Data Binding for the PasswordBox control.
    /// Since PasswordBox does not support direct binding to the Password property for security reasons,
    /// this helper acts as a bridge between the View (PasswordBox) and the ViewModel.
    /// </summary>
    public static class PasswordBoxHelper
    {
        // ------------------------------------------------------------------------
        // 1. BoundPassword Attached Property
        // Holds the password string from the ViewModel.
        // ------------------------------------------------------------------------

        /// <summary>
        /// Gets the bound password value.
        /// </summary>
        public static string GetBoundPassword(DependencyObject d) => (string)d.GetValue(BoundPasswordProperty);

        /// <summary>
        /// Sets the bound password value.
        /// </summary>
        public static void SetBoundPassword(DependencyObject d, string value) => d.SetValue(BoundPasswordProperty, value);

        /// <summary>
        /// DependencyProperty for the password string.
        /// Updates are triggered when the property changes in the ViewModel or via the PasswordBox.
        /// </summary>
        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached("BoundPassword",
            typeof(string),
            typeof(PasswordBoxHelper),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBoundPasswordChanged));

        // ------------------------------------------------------------------------
        // 2. Attach Attached Property
        // Activates the binding behavior when set to True.
        // ------------------------------------------------------------------------

        /// <summary>
        /// Gets the Attach property value.
        /// </summary>
        public static bool GetAttach(DependencyObject d) => (bool)d.GetValue(AttachProperty);

        /// <summary>
        /// Sets the Attach property value.
        /// </summary>
        public static void SetAttach(DependencyObject d, bool value) => d.SetValue(AttachProperty, value);

        /// <summary>
        /// DependencyProperty to enable/disable the password binding behavior.
        /// </summary>
        public static readonly DependencyProperty AttachProperty =
            DependencyProperty.RegisterAttached("Attach",
            typeof(bool),
            typeof(PasswordBoxHelper),
            new PropertyMetadata(false, OnAttachChanged));

        // ------------------------------------------------------------------------
        // 3. PasswordHandler Private Attached Property
        // ------------------------------------------------------------------------

        private static readonly DependencyProperty PasswordHandlerProperty =
            DependencyProperty.RegisterAttached("PasswordHandler",
            typeof(RoutedEventHandler),
            typeof(PasswordBoxHelper),
            new PropertyMetadata(null));

        // ------------------------------------------------------------------------
        // Event Logic 
        // ------------------------------------------------------------------------

        /// <summary>
        /// Handles changes to the 'Attach' property.
        /// Subscribes or unsubscribes from the PasswordChanged event using a lambda capture to avoid casting 'sender'.
        /// </summary>
        private static void OnAttachChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox)
            {
                if ((bool)e.NewValue) // Attached: True
                {
                    // Create a lambda delegate that captures the specific 'passwordBox' instance.
                    // This eliminates the need to use 'object sender' and casting inside the handler.
                    RoutedEventHandler handler = (s, args) =>
                    {
                        // Update the attached property when the user types in the PasswordBox
                        SetBoundPassword(passwordBox, passwordBox.Password);
                    };

                    // Subscribe to the event
                    passwordBox.PasswordChanged += handler;

                    // Store the handler instance so we can unsubscribe later
                    passwordBox.SetValue(PasswordHandlerProperty, handler);
                }
                else // Attached: False
                {
                    // Retrieve the stored handler
                    var handler = (RoutedEventHandler)passwordBox.GetValue(PasswordHandlerProperty);

                    if (handler != null)
                    {
                        // Unsubscribe to prevent memory leaks
                        passwordBox.PasswordChanged -= handler;
                        passwordBox.SetValue(PasswordHandlerProperty, null);
                    }
                }
            }
        }

        /// <summary>
        /// Handles changes to the 'BoundPassword' property (from the ViewModel).
        /// Updates the PasswordBox visual element to match the ViewModel's data.
        /// </summary>
        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox)
            {
                // Retrieve the current event handler
                var handler = (RoutedEventHandler)passwordBox.GetValue(PasswordHandlerProperty);

                // Temporarily detach the event handler to prevent an infinite loop (StackOverflow)
                // where updating the Password triggers the event, which updates the property, which triggers this method again.
                if (handler != null) passwordBox.PasswordChanged -= handler;

                string newPassword = (string)e.NewValue;

                // Update the PasswordBox only if the value has actually changed
                if (passwordBox.Password != newPassword)
                {
                    passwordBox.Password = newPassword ?? string.Empty;
                }

                // Re-attach the event handler
                if (handler != null) passwordBox.PasswordChanged += handler;
            }
        }
    
}

///// <summary>
///// A helper class containing attached properties to enable Two-Way data binding
///// of the <see cref="PasswordBox.Password"/> property in WPF.
///// </summary>
//public static class PasswordBoxHelper
//{
//    // Define the Attached Property to hold the bound password value (ViewModel source).
//    public static readonly DependencyProperty BoundPasswordProperty =
//        DependencyProperty.RegisterAttached("BoundPassword",
//        typeof(string),
//        typeof(PasswordBoxHelper),
//        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBoundPasswordChanged, null, false, UpdateSourceTrigger.PropertyChanged));

//    // Define the helper Attached Property used to activate the binding logic (Attach/Detach events).
//    public static readonly DependencyProperty AttachProperty =
//        DependencyProperty.RegisterAttached("Attach",
//        typeof(bool),
//        typeof(PasswordBoxHelper),
//        new PropertyMetadata(false, OnAttachChanged));

//    // Getters / Setters for the BoundPassword Attached Property.
//    public static string GetBoundPassword(DependencyObject d) => (string)d.GetValue(BoundPasswordProperty);
//    public static void SetBoundPassword(DependencyObject d, string value) => d.SetValue(BoundPasswordProperty, value);
//    public static bool GetAttach(DependencyObject d) => (bool)d.GetValue(AttachProperty);
//    public static void SetAttach(DependencyObject d, bool value) => d.SetValue(AttachProperty, value);


//    // Event Logic
//    private static void OnAttachChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//    {
//        if (d is PasswordBox passwordBox)
//        {
//            if ((bool)e.OldValue == false && (bool)e.NewValue == true)
//            {
//                // Subscribe to the PasswordChanged event when Attach is set to true.
//                passwordBox.PasswordChanged += PasswordChanged;
//            }
//            else if ((bool)e.OldValue == true && (bool)e.NewValue == false)
//            {
//                // Unsubscribe from the event when Attach is set to false.
//                passwordBox.PasswordChanged -= PasswordChanged;
//            }
//        }
//    }

//    private static void PasswordChanged(object sender, RoutedEventArgs e)
//    {
//        if (sender is PasswordBox passwordBox)
//        {
//            // Update the BoundPassword Attached Property with the new password string.
//            SetBoundPassword(passwordBox, passwordBox.Password);
//        }
//    }

//    private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//    {
//        if (d is PasswordBox passwordBox)
//        {
//            // remove the event handler to prevent infinite loop
//            passwordBox.PasswordChanged -= PasswordChanged;

//            string newPassword = (string)e.NewValue;

//            // update the PasswordBox's Password only if it differs from the new value
//            if (passwordBox.Password != newPassword)
//            {
//                passwordBox.Password = newPassword ?? string.Empty;
//            }

//            // return the event handler
//            passwordBox.PasswordChanged += PasswordChanged;
//        }
//    }


//}
public class StatusToCancelVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        
        if (value is OrderStatus status)
        {
           
            if (status != OrderStatus.Delivered && status != OrderStatus.Rejected && status != OrderStatus.Canceled)
            {
                return Visibility.Visible;
            }
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
/// <summary>
/// Converts a value for use in data binding scenarios, supporting nullable selections.
/// </summary>
/// <remarks>This converter is typically used in scenarios where a selection, such as from a ComboBox,  needs to
/// be converted to a nullable value. The <see cref="Convert"/> method passes the value  through unchanged, while the
/// <see cref="ConvertBack"/> method converts specific types  (e.g., <see cref="string"/> or <see cref="ComboBoxItem"/>)
/// to <see langword="null"/>.</remarks>
public class SelectionToNullableConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      
        if (value is string)
        {
            return null;
        }
        
        if (value is ComboBoxItem)
        {
            return null;
        }
        return value;
    }
}


/// <summary>
/// Converts multiple input values to determine whether an item is editable based on the current mode and order status.
/// </summary>
/// <remarks>This converter evaluates two input values: a boolean indicating whether the application is in "add
/// mode"  and an enumeration representing the order status. The result is <see langword="true"/> if the item is
/// editable  (e.g., always editable in "add mode" or editable only when the order status is <see
/// cref="BO.OrderStatus.Open"/>).</remarks>
public class IsEditableConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {


        if (values[0] is bool isAddMode && isAddMode) return true; // add mode => editable

        if (values[1] is BO.OrderStatus status)
        {
            return status == BO.OrderStatus.Open; // 
        }

        return false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();

    }
}
    /// <summary>
    /// Converts a <see cref="bool"/> value to a <see cref="Visibility"/> value, applying an inverse logic.
    /// </summary>
    /// <remarks>This converter is typically used in data binding scenarios where a <see cref="bool"/> value needs to
    /// be  displayed as a <see cref="Visibility"/> value in the UI, with the following logic: <list type="bullet">
    /// <item><description><see langword="true"/> is converted to <see cref="Visibility.Collapsed"/>.</description></item>
    /// <item><description><see langword="false"/> is converted to <see cref="Visibility.Visible"/>.</description></item>
    /// </list> If the input value is not a <see cref="bool"/>, the method defaults to returning <see
    /// cref="Visibility.Visible"/>.</remarks>
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // check if the value is a boolean
            if (value is bool boolValue)
            {
                // apply inverse logic
                if (boolValue == true)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }
            // default case
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a <see cref="TimeSpan"/> value to its string representation and vice versa.
    /// </summary>
    /// <remarks>This converter is primarily used for formatting <see cref="TimeSpan"/> values into a specific string
    /// format and is intended for use in data binding scenarios, such as in WPF or Xamarin applications. The <see
    /// cref="Convert"/> method formats the <see cref="TimeSpan"/> as "dd.hh:mm", where "dd" represents days, "hh"
    /// represents hours, and "mm" represents minutes. If the <see cref="TimeSpan"/> is <see cref="TimeSpan.Zero"/>, the
    /// method returns "00:00:00". The <see cref="ConvertBack"/> method is not implemented and will throw a <see
    /// cref="NotImplementedException"/> if called.</remarks>
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan ts)
            {
                //handle zero timespan
                if (ts == TimeSpan.Zero) return "00:00:00";
                //format timespan as dd.hh:mm
                return ts.ToString(@"dd\.hh\:mm");
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// Converts an integer amount to a boolean indicating eligibility for free shipping.
    /// </summary>
    public class AmountToFreeShippingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //check if the value is an integer
            if (value is int amount)
            {
                return amount >= 20;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// if the selected filter value matches the parameter, show the element; otherwise, collapse it.
    /// </summary>
    public class FilterToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return Visibility.Collapsed;
            //compare the value and parameter as strings
            return value.ToString() == parameter.ToString() ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class CollectionEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Visibility.Visible;
            if (value is ICollection collection && collection.Count == 0) return Visibility.Visible;
            if (value is IEnumerable enumerable && !enumerable.GetEnumerator().MoveNext()) return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// Converts an <see cref="OrderInList"/> object to a user-friendly string representation of the time left for the
    /// order.
    /// </summary>
    /// <remarks>This converter is typically used in data binding scenarios to display the remaining time for an order
    /// in a readable format. If the order's schedule status is <see cref="BO.ScheduleStatus.Late"/>, the method returns "⚠️
    /// TIME OVER". Otherwise, it returns the remaining time formatted as "hh:mm:ss". If the time remaining is null, it
    /// returns "-".</remarks>
    public class TimeLeftDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is OrderInList order)
            {

                if (order.ScheduleStatus == BO.ScheduleStatus.Late)
                {
                    return "⚠️ TIME OVER";
                }


                return order.TimeRemaining.ToString(@"d\.hh\:mm");
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// Converts an <see cref="OrderInList"/> object into a user-friendly string representation of its total time display,
    /// based on the order's status.
    /// </summary>
    /// <remarks>If the order's status is <see cref="BO.OrderStatus.Open"/>, the method returns "🕒 Pending
    /// Collection". Otherwise, it returns the total completion time formatted as "hh:mm:ss". If the total completion time
    /// is null, it returns a hyphen ("-"). For unsupported input types, an empty string is returned.</remarks>
    public class TotalTimeDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is OrderInList order)
            {

                if (order.Status == BO.OrderStatus.Open)
                {
                    return "🕒 Pending Collection";
                }
                if (order.Status == BO.OrderStatus.Canceled)
                {
                    return "❌ CANCELED";//might need to change to 0 timespan need to check with orders 
                }
                //if(order.Status == BO.OrderStatus.Rejected)
                //{
                //    return "❌ REJECTED";//might need to change to 0 timespan need to check with orders 
                //}
                if (order.Status == BO.OrderStatus.InProgress)
                {
                    return "⏳ In Progress";//might need to change to 0 timespan need to check with orders 
                }
                return order.TotalCompletionTime.ToString(@"d\.hh\:mm");
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts an order status value to a <see cref="Visibility"/> value, determining whether a warning should be
    /// displayed.
    /// </summary>
    /// <remarks>This converter checks if the provided status value is not "Open". If the status is not "Open",
    /// it returns <see cref="Visibility.Visible"/> to indicate that a warning should be displayed. Otherwise, it returns
    /// <see cref="Visibility.Collapsed"/> to hide the warning.</remarks>
    public class StatusToWarningVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value != null && value.ToString() != "Open")
            {
                return Visibility.Visible;//show warning
            }
            return Visibility.Collapsed;//hide warning
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts multiple input values into a <see cref="Visibility"/> value based on the specified conditions.
    /// </summary>
    /// <remarks>This converter is typically used to determine the visibility of an action button in a user
    /// interface. The visibility is determined by evaluating the input values, which include a boolean indicating "add
    /// mode" and an <see cref="BO.OrderStatus"/> value. If the "add mode" is active, the button is visible. Otherwise,
    /// the visibility depends on whether the order status is <see cref="BO.OrderStatus.Open"/>.</remarks>
    public class ActionButtonVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //check if we are in add mode
            if (values[0] is bool isAddMode && isAddMode)
            {
                return Visibility.Visible; // in add mode, always visible
            }

            //in case not in add mode, check order status
            if (values[1] is BO.OrderStatus status)
            {
                return status == BO.OrderStatus.Open ? Visibility.Visible : Visibility.Collapsed;
            }

            //default to collapsed
            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

/// <summary>
/// a converter that converts a not-null value to boolean false, and null to true.
/// </summary>
public class NotNullToBooleanFalseConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}



public class BooleanToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // check if the value is a boolean and true
        bool isTrue = value is bool b && b;

        if (isTrue)
        {
            // green
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#81C784"));
        }
        else
        {
            // red
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E57373"));
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class FilterTextToMessageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string filterText = value as string;

        //if there is no filter text, show all orders message
        if (string.IsNullOrWhiteSpace(filterText))
        {
            return "Looks like there are no orders matching your preferences right now. Check Back Soon.";
        }

        //if there is filter text, show no orders message
        return "No orders match your filter criteria.";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// converter from DeliveryCompletionType to background color brush. to closed deliveries listview item background.
/// </summary>
public class DeliveryCompletionToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is BO.DeliveryCompletionType type)
        {
            return type switch
            {
                BO.DeliveryCompletionType.Supplied => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E9")), // green
                BO.DeliveryCompletionType.CustomerRefused => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3E0")), // orenge
                BO.DeliveryCompletionType.Canceled => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBEE")), // red
                BO.DeliveryCompletionType.CustomerNotFound => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3E5F5")), // purple
                BO.DeliveryCompletionType.Failed => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ECEFF1")), // blue gray
                _ => Brushes.White
            };
        }
        return Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

/// <summary>
/// converter from DeliveryCompletionType to icon string. to closed deliveries listview item icon.
/// </summary>
public class DeliveryCompletionToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is BO.DeliveryCompletionType type)
        {
            return type switch
            {
                BO.DeliveryCompletionType.Supplied => "✅",
                BO.DeliveryCompletionType.CustomerRefused => "✋",
                BO.DeliveryCompletionType.Canceled => "❌",
                BO.DeliveryCompletionType.CustomerNotFound => "🏠❓",
                BO.DeliveryCompletionType.Failed => "⚠️",
                _ => "❓"
            };
        }
        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
public class OrderRequirementsToImageConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        
        if (value == null) return null;

        if (value is BO.OrderRequirements requirement)
        {
            string imageName;

            switch (requirement)
            {
                case BO.OrderRequirements.Dry: 
                    imageName = "dry.png";
                    break;

                case BO.OrderRequirements.Chilled: 
                    imageName = "chilled.png";
                    break;

                case BO.OrderRequirements.Frozen: 
                    imageName = "frozen.png";
                    break;

                case BO.OrderRequirements.Fragile:
                    imageName = "fragile.png";
                    break;


                case BO.OrderRequirements.Mixed:
                    imageName = "mixed.png";
                    break;

                default:
                    imageName = "mixed.png"; 
                    break;
            }

            //return the image path
            return $"/Images/{imageName}";
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
/// <summary>
/// Converts a courier's status to a <see cref="Visibility"/> value based on specific conditions.
/// </summary>
/// <remarks>This converter determines the visibility of a UI element based on the state of a courier. The element
/// is visible only if the courier has no delivery history and is not currently handling any orders.</remarks>
public class CourierDeleteAbleToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return Visibility.Collapsed;

        //check if the value is a Courier
        if (value is BO.Courier courier)
        {
            //has no history: total on time + total late == 0
            bool hasNoHistory = (courier.TotalDeliveredOnTime + courier.TotalLateDeliveries) == 0;

            // order in progress is null
            bool isIdle = courier.OrderInProgress == null;

            //if both conditions are met, return Visible
            if (hasNoHistory && isIdle)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }
        if (value is BO.CourierInList courierInList)
        {
            //has no history: total on time + total late == 0
            bool hasNoHistory = (courierInList.OnTimeDeliveries + courierInList.LateDeliveries) == 0;

            // order in progress is null
            bool isIdle = courierInList.ActiveDeliveryId == null;

            //if both conditions are met, return Visible
            if (hasNoHistory && isIdle)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }

            
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts the simulator running state (bool) to button text.
/// False -> "▶ Start Simulator"
/// True  -> "⏹ Stop Simulator"
/// </summary>
public class SimulatorStatusToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isRunning = (bool)value;
        return isRunning ? "⏹ Stop Simulator" : "▶ Start Simulator";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// converts the simulator running state (bool) to button background color.
/// </summary>
public class SimulatorStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isRunning = (bool)value;
        // Uses standard WPF colors, can be adjusted to hex codes (e.g., #FF4CAF50)
        return isRunning ? "#FFAB91" : "#A5D6A7";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
