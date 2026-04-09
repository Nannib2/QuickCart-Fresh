using BO;
using PL.Courier;
using PL.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace PL.Order;

public partial class OrdersListWindow : Window
{
    static readonly BLApi.IBl s_bl = BLApi.Factory.Get();
    private Window _previousWindow;
    public int reqesterId = 0;
    public OrdersListWindow( Window previousWindow ,int idManager, OrderStatus? orderStatus=null, ScheduleStatus? scheduleStatus=null)
    {
        InitializeComponent();
        SelectedStatus= orderStatus;
        SelectedScheduleStatus= scheduleStatus;
        reqesterId = idManager;
        _previousWindow = previousWindow;
        RefreshOrderList();
    }

    /* ===================== Dependency Properties ===================== */


    public IEnumerable<BO.OrderInList> OrderList
    {
        get { return (IEnumerable<BO.OrderInList>)GetValue(OrderListProperty); }
        set { SetValue(OrderListProperty, value); }
    }
    public static readonly DependencyProperty OrderListProperty =
        DependencyProperty.Register(nameof(OrderList), typeof(IEnumerable<BO.OrderInList>), typeof(OrdersListWindow), new PropertyMetadata(null));

    //  Type
    public BO.OrderRequirements? SelectedOrderType
    {
        get { return (BO.OrderRequirements?)GetValue(SelectedOrderTypeProperty); }
        set { SetValue(SelectedOrderTypeProperty, value); }
    }
    public static readonly DependencyProperty SelectedOrderTypeProperty =
        DependencyProperty.Register(nameof(SelectedOrderType), typeof(BO.OrderRequirements?), typeof(OrdersListWindow), new PropertyMetadata(null, OnFilterChanged));

    // Status
    public BO.OrderStatus? SelectedStatus
    {
        get { return (BO.OrderStatus?)GetValue(SelectedStatusProperty); }
        set { SetValue(SelectedStatusProperty, value); }
    }
    public static readonly DependencyProperty SelectedStatusProperty =
        DependencyProperty.Register(nameof(SelectedStatus), typeof(BO.OrderStatus?), typeof(OrdersListWindow), new PropertyMetadata(null, OnFilterChanged));


    // Status
    public BO.ScheduleStatus? SelectedScheduleStatus
    {
        get { return (BO.ScheduleStatus?)GetValue(SelectedScheduleStatusProperty); }
        set { SetValue(SelectedScheduleStatusProperty, value); }
    }
    public static readonly DependencyProperty SelectedScheduleStatusProperty =
        DependencyProperty.Register(nameof(SelectedScheduleStatus), typeof(BO.ScheduleStatus?), typeof(OrdersListWindow), new PropertyMetadata(null, OnFilterChanged));

    // Sort
    public BO.OrderInListProperties? SelectedSortProperty
    {
        get { return (BO.OrderInListProperties?)GetValue(SelectedSortPropertyProperty); }
        set { SetValue(SelectedSortPropertyProperty, value); }
    }
    public static readonly DependencyProperty SelectedSortPropertyProperty =
        DependencyProperty.Register(nameof(SelectedSortProperty), typeof(BO.OrderInListProperties?), typeof(OrdersListWindow), new PropertyMetadata(null, OnFilterChanged));

    //  Group
    public string SelectedGroupProperty
    {
        get { return (string)GetValue(SelectedGroupPropertyProperty); }
        set { SetValue(SelectedGroupPropertyProperty, value); }
    }
    public static readonly DependencyProperty SelectedGroupPropertyProperty =
        DependencyProperty.Register(nameof(SelectedGroupProperty), typeof(string), typeof(OrdersListWindow), new PropertyMetadata("None", OnFilterChanged));

    public BO.OrderInList? SelectedOrder
    {
        get { return (BO.OrderInList?)GetValue(SelectedOrderProperty); }
        set { SetValue(SelectedOrderProperty, value); }
    }
    public static readonly DependencyProperty SelectedOrderProperty =
        DependencyProperty.Register(nameof(SelectedOrder), typeof(BO.OrderInList), typeof(OrdersListWindow), new PropertyMetadata(null));


  
    private static void OnFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is OrdersListWindow window)
        {
            window.RefreshOrderList();
        }
    }

    /* ===================== Logic Methods ===================== */
    //
    private void RefreshOrderList()
    {
        try
        {
            BO.Config config = s_bl.Admin.GetConfig();
            OrderList = s_bl.Order.ReadAll(
            requesterId: config.ManagerId,
            statusFilter: SelectedStatus,   
            typeFilter: SelectedOrderType,         
            scheduleFilter: SelectedScheduleStatus, 
            sortBy: SelectedSortProperty
        );
           
            ApplyGroupingAndSorting();
        }
        catch (BO.BlInvalidOperationException ex)
        {
            MessageBox.Show($"Error loading data: {ex.Message}", "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (BlExternalServiceException ex)
        {
            MessageBox.Show($"Error loading data: {ex.Message}", "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (BlInvalidInputException ex)
        {
            MessageBox.Show($"Error loading data: {ex.Message}", "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ApplyGroupingAndSorting()
    {
        if (OrderList == null) return;

        ICollectionView view = CollectionViewSource.GetDefaultView(OrderList);
        view.SortDescriptions.Clear();

        if (SelectedSortProperty.HasValue)
        {
            string sortPropertyName = SelectedSortProperty.Value.ToString();
            view.SortDescriptions.Add(new SortDescription(sortPropertyName, ListSortDirection.Ascending));
        }

        view.GroupDescriptions.Clear();
        if (!string.IsNullOrEmpty(SelectedGroupProperty) && SelectedGroupProperty != "None")
        {
            view.GroupDescriptions.Add(new PropertyGroupDescription(SelectedGroupProperty));
        }
    }

    /* ===================== Event Handlers ===================== */

    private void OrderListWindow_Loaded(object sender, RoutedEventArgs e)
    {
        s_bl.Order.AddObserver(OrderListObserver);
        s_bl.Admin.AddClockObserver(OrderListObserver);
    }

    private void OrderListWindow_Closed(object sender, EventArgs e)
    {
        s_bl.Order.RemoveObserver(OrderListObserver);
    }

    //mutex for observer
    private readonly ObserverMutex _OrderListMutex = new(); //stage 7
    /// <summary>
    /// Invokes the <see cref="RefreshOrderList"/> method on the UI thread to update the order list.
    /// </summary>
    /// <remarks>This method ensures that the <see cref="RefreshOrderList"/> method is executed on the thread
    /// associated with the UI dispatcher, which is necessary for updating UI elements in a thread-safe
    /// manner.</remarks>
    private void OrderListObserver()
    {
        if (_OrderListMutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;
        Dispatcher.BeginInvoke(async () => 
        {
            RefreshOrderList();
            // After completing the work, check if a restart was requested
            if (await _OrderListMutex.UnsetLoadInProgressAndCheckRestartRequested())
                OrderListObserver();
        });
    }

   
    private void btnAddOrder_Click(object sender, RoutedEventArgs e)
    {
        //this.Hide(); //hide this window
        new OrderWindow(this).Show();
    }

    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (SelectedOrder != null)
        {
            //this.Hide(); //hide this window
            OrderWindow win= new OrderWindow(this,SelectedOrder.OrderId);
            win.Show();
        }
    }

    private void btnCancelOrder_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedOrder != null)
        {
            BO.OrderInList orderToCancel = SelectedOrder;

            MessageBoxResult result = MessageBox.Show(
                $"Are you sure you want to cancel order #{orderToCancel.OrderId}?",
                "Cancel Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    BO.Config config = s_bl.Admin.GetConfig();
                    s_bl.Order.CancelOrder(orderToCancel.OrderId, config.ManagerId);
                    MessageBox.Show("Order canceled successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (BO.BlInvalidOperationException ex)
                {
                    MessageBox.Show($"Failed to cancel: {ex.Message}", "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (BO.BlDoesNotExistException ex)
                {
                    MessageBox.Show($"Failed to cancel: {ex.Message}", "Does Not Exist Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (BO.BLTemporaryNotAvailableException)
                {
                    MessageBox.Show("Modifications are disabled while the simulation is active. The system is in read-only mode.", "Temporary Unavailability", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }
        }
        else
        {
            MessageBox.Show("Please select an order first.", "Selection Missing", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void btnBackMainWindow_Click(object sender, RoutedEventArgs e)
    {
        _previousWindow.Show();
        this.Close();
    }
}