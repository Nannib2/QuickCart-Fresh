using PL.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using PL.Helpers;

namespace PL.Courier;

/// <summary>
/// Interaction logic for CourierDeliveriesHistoryWindow.xaml
/// </summary>
public partial class CourierDeliveriesHistoryWindow : Window
{
    // Static reference to the Business Logic layer API.
    static readonly BLApi.IBl s_bl = BLApi.Factory.Get();
    private readonly int _currentCourierId;
    private Window _previousWindow;
    #region Dependency Properties

    /// <summary>
    /// list of closed deliveries to display in the UI
    /// </summary>
    public IEnumerable<BO.ClosedDeliveryInList> ClosedDeliveries
    {
        get { return (IEnumerable<BO.ClosedDeliveryInList>)GetValue(ClosedDeliveriesProperty); }
        set { SetValue(ClosedDeliveriesProperty, value); }
    }
    public static readonly DependencyProperty ClosedDeliveriesProperty =
        DependencyProperty.Register("ClosedDeliveries", typeof(IEnumerable<BO.ClosedDeliveryInList>), typeof(CourierDeliveriesHistoryWindow));

    /// <summary>
    /// filter value selected in the ComboBox (order requirements)
    /// </summary>
    public BO.OrderRequirements? SelectedFilter
    {
        get { return (BO.OrderRequirements?)GetValue(SelectedFilterProperty); }
        set { SetValue(SelectedFilterProperty, value); }
    }
    public static readonly DependencyProperty SelectedFilterProperty =
        DependencyProperty.Register("SelectedFilter", typeof(BO.OrderRequirements?), typeof(CourierDeliveriesHistoryWindow) ,new PropertyMetadata(null));
           
    /// <summary>
    /// sort property selected in the ComboBox
    /// </summary>
    public BO.ClosedDeliveryInListProperties? SelectedSort
    {
        get { return (BO.ClosedDeliveryInListProperties)GetValue(SelectedSortProperty); }
        set { SetValue(SelectedSortProperty, value); }
    }
    public static readonly DependencyProperty SelectedSortProperty =
        DependencyProperty.Register("SelectedSort", typeof(BO.ClosedDeliveryInListProperties), typeof(CourierDeliveriesHistoryWindow) ,new PropertyMetadata(null));

    //arr ays for ComboBoxes ItemsSource
    public IEnumerable ClosedDeliveryInList => new ClosedDeliveryInListPropertiesCollection();

    #endregion

    

    /// <summary>
    /// combo box logic - filter 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ComboBox_FilterSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        CourierHistoryObserver();
    }

    /// <summary>
    /// combo box logic - sort
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ComboBox_SortSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        CourierHistoryObserver();
    }


    /// <summary>
    /// Handles the <see cref="Window.Loaded"/> event. Subscribes the <see cref="CourierObserver"/>
    /// to the BL layer for real-time updates if the window is in Update mode (ID != 0).
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void CourierDeliveriesHistoryWindow_Loaded(object sender, RoutedEventArgs e)
    => s_bl.Courier.AddObserver(CourierHistoryObserver);
    


    ///// <summary>
    ///// Handles the <see cref="Window.Closed"/> event. Unsubscribes the <see cref="CourierObserver"/>
    ///// from the BL layer to prevent memory leaks and unnecessary calls.
    ///// </summary>
    ///// <param name="sender">The source of the event.</param>
    ///// <param name="e">The event data.</param>
    private void CourierDeliveriesHistoryWindow_Closed(object sender, EventArgs e)
    => s_bl.Courier.RemoveObserver(CourierHistoryObserver);


    //mutex to prevent overlapping refresh calls
    private readonly ObserverMutex _CourierHistoryMutex = new (); //stage 7

  
    /// <summary>
    /// observer
    /// </summary>
    private void CourierHistoryObserver()
    {
        // cheaking 
        if (_CourierHistoryMutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;

        Dispatcher.BeginInvoke(async () =>
        {
            try
            {
                var newList = s_bl.Order.GetClosedDeliveriesByCourier(
                    _currentCourierId,
                    _currentCourierId,
                    SelectedFilter,
                    SelectedSort
                );

               //update the property
                ClosedDeliveries = newList;

                
                System.Diagnostics.Debug.WriteLine("History Updated Successfully");
            }
            catch (BO.BlInvalidInputException ex)
            {
                System.Diagnostics.Debug.WriteLine($"History Refresh Warning: {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"History Refresh Error: {ex.Message}");
            }
            finally
            {
                // release the mutex
                if (await _CourierHistoryMutex.UnsetLoadInProgressAndCheckRestartRequested())
                {
                    CourierHistoryObserver();
                }
            }
        });
    }

    /// <summary>
    ///  Initialize the window
    /// </summary>
    /// <param name="courierId"></param>
    /// <param name="previousWindow"></param>
    public CourierDeliveriesHistoryWindow(int courierId, Window previousWindow)
    {
        _currentCourierId = courierId;
        this.DataContext = this;  // Binding context set to this class
        _previousWindow = previousWindow;

        InitializeComponent();

        // load data initially 
        CourierHistoryObserver();
    }

    /// <summary>
    /// back button 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnBackCourierWindow_Click(object sender, RoutedEventArgs e)
    {
        _previousWindow.Show();
        this.Close();
        
    }
}
