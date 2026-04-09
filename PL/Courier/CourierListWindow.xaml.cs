
using BO;
using PL.Helpers;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace PL.Courier
{
    /// <summary>
    /// Interaction logic for CourierListWindow.xaml.
    /// Represents the window that displays and manages the list of couriers.
    /// </summary>
    public partial class CourierListWindow : Window
    {
        /// <summary>
        /// Static reference to the Business Layer interface (Singleton access).
        /// </summary>
        static readonly BLApi.IBl s_bl = BLApi.Factory.Get();

        // The id of the requester (manager)
        public int requesterId = 0;
        private Window _previousWindow;//store the previous window to return to it when needed

        public ICommand DeleteCourierCommand { get; private set; }
        public ICommand AddCourierCommand { get; private set; }
        public ICommand BackCommand { get; private set; }
        // ----------------------------------------------------------------------------
        // Dependency Properties
        // ----------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the list of couriers to be presented in the window.
        /// </summary>
        public IEnumerable<BO.CourierInList>? CourierList
        {
            get { return (IEnumerable<BO.CourierInList>)GetValue(CourierListProperty); }
            set { SetValue(CourierListProperty, value); }
        }

        public static readonly DependencyProperty CourierListProperty =
            DependencyProperty.Register("CourierList", typeof(IEnumerable<BO.CourierInList>), typeof(CourierListWindow), new PropertyMetadata(null));

        /// <summary>
        /// Property for the main filter selection (e.g., Status, DeliveryType).
        /// Includes a callback to refresh the list automatically when changed.
        /// </summary>
        public BO.CourierInListProperties? SelectedPropForFilter
        {
            get { return (BO.CourierInListProperties?)GetValue(SelectedPropForFilterProperty); }
            set { SetValue(SelectedPropForFilterProperty, value); }
        }

        public static readonly DependencyProperty SelectedPropForFilterProperty =
            DependencyProperty.Register(nameof(SelectedPropForFilter), typeof(BO.CourierInListProperties?), typeof(CourierListWindow),
            new PropertyMetadata(null, (d, e) => ((CourierListWindow)d).filterByprop()));//after a change in SelectedPropForFilterProperty, call filterByprop

        /// <summary>
        /// Property for the sort selection.
        /// Includes a callback to refresh the list automatically when changed.
        /// </summary>
        public BO.CourierInListProperties? SelectedPropForSort
        {
            get { return (BO.CourierInListProperties?)GetValue(SelectedPropForSortProperty); }
            set { SetValue(SelectedPropForSortProperty, value); }
        }

        public static readonly DependencyProperty SelectedPropForSortProperty =
            DependencyProperty.Register(nameof(SelectedPropForSort), typeof(BO.CourierInListProperties?), typeof(CourierListWindow),
            new PropertyMetadata(null, (d, e) => ((CourierListWindow)d).sortByProp()));//after a change in SelectedPropForSortProperty, call sortByProp

        /// <summary>
        /// Selected value for "Is Active" filter.
        /// </summary>
        public bool? IsActiveFilter
        {
            get { return (bool?)GetValue(IsActiveFilterProperty); }
            set { SetValue(IsActiveFilterProperty, value); }
        }

        public static readonly DependencyProperty IsActiveFilterProperty =
            DependencyProperty.Register("IsActiveFilter", typeof(bool?), typeof(CourierListWindow),
            new PropertyMetadata(null, (d, e) => ((CourierListWindow)d).filterByprop()));//after a change in IsActiveFilterProperty, call filterByprop

        /// <summary>
        /// Selected value for "Delivery Type" filter.
        /// </summary>
        public BO.DeliveryTypeMethods? DeliveryTypeMethod
        {
            get { return (BO.DeliveryTypeMethods?)GetValue(DeliveryTypeMethodProperty); }
            set { SetValue(DeliveryTypeMethodProperty, value); }
        }

        public static readonly DependencyProperty DeliveryTypeMethodProperty =
            DependencyProperty.Register(nameof(DeliveryTypeMethod), typeof(BO.DeliveryTypeMethods?), typeof(CourierListWindow),
            new PropertyMetadata(null, (d, e) => ((CourierListWindow)d).filterByprop()));//after a change in DeliveryTypeMethodProperty, call filterByprop

        /// <summary>
        /// Gets or sets the courier currently selected in the DataGrid.
        /// Defined as a DependencyProperty to ensure proper Data Binding.
        /// </summary>
        public BO.CourierInList? SelectedCourier
        {
            get { return (BO.CourierInList?)GetValue(SelectedCourierProperty); }
            set { SetValue(SelectedCourierProperty, value); }
        }

        public static readonly DependencyProperty SelectedCourierProperty =
            DependencyProperty.Register("SelectedCourier", typeof(BO.CourierInList), typeof(CourierListWindow), new PropertyMetadata(null));


        /// Gets the array of boolean options for filtering active/inactive couriers.
        /// </summary>
        public bool[] BoolOptions { get; } = { true, false };

        /// <summary>
        /// if the courier can be deleted
        /// </summary>
        public bool IsDeletable { get; set; }


        // ----------------------------------------------------------------------------
        // Constructor & Window Events
        // ----------------------------------------------------------------------------

        public CourierListWindow(int managerId, Window previousWindow)
        {
            requesterId = managerId;
            InitializeComponent();
            _previousWindow = previousWindow;
            
            DeleteCourierCommand = new RelayCommand<BO.CourierInList>(OnDeleteCourier);
            AddCourierCommand = new RelayCommand(OnAddCourier);
            BackCommand = new RelayCommand(OnBack);
            DataContext = this;// Set the DataContext for data binding here for order
        }

        /// <summary>
        /// Handles the Loaded event of the window.
        /// Initializes the list and subscribes to the BL observer updates.
        /// </summary>
        private void CourierInListWindow_Loaded(object sender, RoutedEventArgs e)
        {
            filterByprop(); // Initial load
            s_bl.Courier.AddObserver(CourierListObserver); // Subscribe to updates
        }

        /// <summary>
        /// Handles the Closed event of the window.
        /// Unsubscribes from the BL observer updates to prevent memory leaks.
        /// </summary>
        private void CourierInListWindow_Closed(object sender, EventArgs e)
            => s_bl.Courier.RemoveObserver(CourierListObserver);


        // ----------------------------------------------------------------------------
        // Logic Methods
        // ----------------------------------------------------------------------------

        /// Mutex for thread-safe observer updates.
        private readonly ObserverMutex _CourierListMutex = new(); //stage 7

        /// <summary>
        /// Observer callback method triggered when the courier list changes in the BL.
        /// Refreshes the list display on the UI thread.
        /// </summary>
        private void CourierListObserver()
        {
            if (_CourierListMutex.CheckAndSetLoadInProgressOrRestartRequired())
                return;

            Dispatcher.BeginInvoke(async () =>
            {
                filterByprop();
                // After completing the work, check if a restart was requested
                if (await _CourierListMutex.UnsetLoadInProgressAndCheckRestartRequested())
                    CourierListObserver();
            });
        }

        /// <summary>
        /// Filters and retrieves the list of couriers from the BL according to the selected properties.
        /// </summary>
        private void filterByprop()
        {
            try
            {
                bool? activeParam = null;
                object? innerFilter = null;

                // Map specific filter values
                if (SelectedPropForFilter == CourierInListProperties.IsActive)
                {
                    activeParam = IsActiveFilter;
                }
                else if (SelectedPropForFilter == CourierInListProperties.DeliveryType)
                {
                    innerFilter = DeliveryTypeMethod;
                }

                // Retrieve data
                CourierList = s_bl?.Courier.ReadAll(requesterId, activeParam, SelectedPropForSort, SelectedPropForFilter, innerFilter);
            }
            catch (BO.BlDoesNotExistException)
            {
                MessageBox.Show("Unable to load data. The requested information was not found.", "Operation Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (BO.BlInvalidOperationException)
            {
                MessageBox.Show("A system error occurred while loading the list. Please contact support.", "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Sorts the list based on the selected sort property.
        /// </summary>
        private void sortByProp()
        {
            try
            {
                // Retrieve data with sort only (preserving current filter context if needed, but per logic here it resets filter)
                CourierList = s_bl?.Courier.ReadAll(requesterId, null, SelectedPropForSort, SelectedPropForFilter, null);
            }
            catch (BO.BlDoesNotExistException)
            {
                MessageBox.Show("Unable to load data.", "Operation Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (BO.BlInvalidOperationException)
            {
                MessageBox.Show("System error during sorting.", "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // ----------------------------------------------------------------------------
        // UI Interaction Events
        // ----------------------------------------------------------------------------

        /// <summary>
        /// Opens the details/update window for the selected courier.
        /// </summary>
        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedCourier != null)
            {
                //this.Hide(); //hide this window
                CourierWindow win = new CourierWindow(this, SelectedCourier.Id);
                win.Show();

            }
        }

        /// <summary>
        /// Opens a new window for adding a courier.
        /// </summary>
        private void OnAddCourier()
        {
           // this.Hide(); //hide this window
            CourierWindow win = new CourierWindow(this);
            win.Show();
            
        }

        /// <summary>
        /// Deletes the specific courier associated with the clicked button.
        /// </summary>
        private void OnDeleteCourier(BO.CourierInList courierToDelete)
        {
            if (courierToDelete == null) return;

            MessageBoxResult result = MessageBox.Show(
                $"Are you sure you want to delete courier ID {courierToDelete.Id} ({courierToDelete.NameCourier})?",
                "Confirm Deletion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                    {
                        BO.Config con = s_bl.Admin.GetConfig();
                        int idManager = con.ManagerId;
                        s_bl.Courier.Delete(courierToDelete.Id, idManager);

                        MessageBox.Show("Courier deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
        

        /// <summary>
        /// Navigates back to the main window.
        /// </summary>
        private void OnBack()
        {
            _previousWindow.Show();
            this.Close();
        }
    }
}
