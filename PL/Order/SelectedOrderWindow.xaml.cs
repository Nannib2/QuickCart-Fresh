using BO;
using Microsoft.Win32;
using PL.Courier;
using PL.Enums;
using PL.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PL.Order;

/// <summary>
/// Interaction logic for SelectedOrderWindow.xaml
/// </summary>
public partial class SelectedOrderWindow : Window
{
    private static readonly BLApi.IBl s_bl = BLApi.Factory.Get();
    private readonly int _courierId;
    private readonly int _requesterId;
    private readonly Window _previousWindow;

    #region Dependency Properties

    /// <summary>
    /// Collection of open orders available for the courier.
    /// </summary>
    public IEnumerable<BO.OpenOrderInList> OpenOrders
    {
        get => (IEnumerable<BO.OpenOrderInList>)GetValue(OpenOrdersProperty);
        set => SetValue(OpenOrdersProperty, value);
    }
    public static readonly DependencyProperty OpenOrdersProperty =
        DependencyProperty.Register(nameof(OpenOrders), typeof(IEnumerable<BO.OpenOrderInList>), typeof(SelectedOrderWindow));

    /// <summary>
    /// Currently selected order.
    /// </summary>
    public BO.OpenOrderInList SelectedOrder
    {
        get => (BO.OpenOrderInList)GetValue(SelectedOrderProperty);
        set => SetValue(SelectedOrderProperty, value);
    }
    public static readonly DependencyProperty SelectedOrderProperty =
        DependencyProperty.Register(nameof(SelectedOrder), typeof(BO.OpenOrderInList), typeof(SelectedOrderWindow),
            new PropertyMetadata(null, OnSelectedOrderChanged));

    /// <summary>
    /// HTML content injected into the WebBrowser control to display the map.
    /// </summary>
    public string MapHtml
    {
        get => (string)GetValue(MapHtmlProperty);
        set => SetValue(MapHtmlProperty, value);
    }
    public static readonly DependencyProperty MapHtmlProperty =
        DependencyProperty.Register(nameof(MapHtml), typeof(string), typeof(SelectedOrderWindow), new PropertyMetadata(string.Empty));

    /// <summary>
    /// Indicates whether an order is currently selected.
    /// </summary>
    public bool IsOrderSelected
    {
        get => (bool)GetValue(IsOrderSelectedProperty);
        set => SetValue(IsOrderSelectedProperty, value);
    }
    public static readonly DependencyProperty IsOrderSelectedProperty =
        DependencyProperty.Register(nameof(IsOrderSelected), typeof(bool), typeof(SelectedOrderWindow), new PropertyMetadata(false));

    /// <summary>
    /// Text used to filter orders by type or requirement.
    /// </summary>
    public string FilterText
    {
        get => (string)GetValue(FilterTextProperty);
        set => SetValue(FilterTextProperty, value);
    }
    public static readonly DependencyProperty FilterTextProperty =
        DependencyProperty.Register(nameof(FilterText), typeof(string), typeof(SelectedOrderWindow),
            new PropertyMetadata(string.Empty, OnFilterChanged));

    /// <summary>
    /// Selected property used for sorting the order list.
    /// </summary>
    public BO.OpenOrderInListProperties? SelectedSortProperty
    {
        get => (BO.OpenOrderInListProperties?)GetValue(SelectedSortPropertyProperty);
        set => SetValue(SelectedSortPropertyProperty, value);
    }
    public static readonly DependencyProperty SelectedSortPropertyProperty =
        DependencyProperty.Register(nameof(SelectedSortProperty), typeof(BO.OpenOrderInListProperties?),
            typeof(SelectedOrderWindow), new PropertyMetadata(BO.OpenOrderInListProperties.OrderId));

    /// <summary>
    /// Items source for sorting ComboBox.
    /// </summary>
    public IEnumerable OpenOrderInList => new OpenOrderInListPropertiesCollection();

    #endregion

    #region Commands

    public ICommand CollectOrderCommand { get; }
    public ICommand RefreshCommand { get; }

    #endregion

    /// <summary>
    /// Constructor.
    /// </summary>
    public SelectedOrderWindow(int courierId, Window previousWindow)
    {
        // Required for proper Leaflet rendering inside WPF WebBrowser
        SetBrowserEmulation();

        InitializeComponent();

        _courierId = courierId;
        _requesterId = courierId;
        _previousWindow = previousWindow;

        CollectOrderCommand = new RelayCommand<BO.OpenOrderInList>(async (order) => await OnCollectOrder(order));
        RefreshCommand = new RelayCommand(async () => await LoadData());

        DataContext = this;
    }

    /// <summary>
    /// Handles the Window's Loaded event. 
    /// Asynchronously initializes the data and registers an observer to monitor changes in orders.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event data providing details about the load.</param>
    private async void SelectedOrderWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadData();
        s_bl.Order.AddObserver(OpenOrdersObserver);
    }

    /// <summary>
    /// Handles the Window's Closed event.
    /// Ensures proper cleanup by removing the order observer to prevent memory leaks and redundant updates.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event data providing details about the closure.</param>
    private void SelectedOrderWindow_Closed(object sender, EventArgs e)
    {
        s_bl.Order.RemoveObserver(OpenOrdersObserver);
    }

    #region Map Logic (Leaflet)

    /// <summary>
    /// Triggered when the selected order changes.
    /// Updates the map accordingly.
    /// </summary>
    private static async void OnSelectedOrderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SelectedOrderWindow win)
        {
            win.IsOrderSelected = e.NewValue != null;
            await win.UpdateMapHtml();
        }
    }

    /// <summary>
    /// Builds the HTML page that renders the map using Leaflet and OSRM routing.
    /// </summary>
    private async Task UpdateMapHtml()
    {
        if (SelectedOrder == null)
        {
            MapHtml = string.Empty;
            return;
        }

        try
        {
            var fullOrder = await s_bl.Order.Read(s_bl.Admin.GetConfig().ManagerId, SelectedOrder.OrderId);

            // Company location (start point)
            double startLat = s_bl.Admin.GetConfig().Latitude ?? 31.7683;
            double startLon = s_bl.Admin.GetConfig().Longitude ?? 35.2137;
            double endLat = fullOrder.Latitude;
            double endLon = fullOrder.Longitude;

            string html = $@"
            <!DOCTYPE html>
            <html>
            <head>
    <meta http-equiv='X-UA-Compatible' content='IE=edge'/>
    <meta charset='utf-8'/>
    <style>
        body {{ margin:0; padding:0; overflow:hidden; }} 
        #map {{ height:100vh; width:100%; }}
    </style>
    <link rel='stylesheet' href='https://unpkg.com/leaflet@1.7.1/dist/leaflet.css'/>
    <script src='https://unpkg.com/leaflet@1.7.1/dist/leaflet.js'></script>
    <link rel='stylesheet' href='https://unpkg.com/leaflet-routing-machine@3.2.12/dist/leaflet-routing-machine.css'/>
    <script src='https://unpkg.com/leaflet-routing-machine@3.2.12/dist/leaflet-routing-machine.js'></script>
</head>
<body>
    <div id='map'></div>
    <script>
        // 1. Initialize the map
        var map = L.map('map');
        
        L.tileLayer('https://{{s}}.tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png', {{
            attribution: '© OpenStreetMap contributors'
        }}).addTo(map);

        var startPoint = [{startLat}, {startLon}];
        var endPoint = [{endLat}, {endLon}];

        // 2. Add custom markers
        var startMarker = L.marker(startPoint).addTo(map).bindPopup('<b>Company</b><br>Start Point');
        var endMarker = L.marker(endPoint).addTo(map).bindPopup('<b>Order {SelectedOrder.OrderId}</b><br>Destination').openPopup();

        // 3. Add air-line (straight dashed line)
        var airLine = L.polyline([startPoint, endPoint], {{
            color: '#FF5722',   // Orange/Red color
            weight: 3,          // Line thickness
            opacity: 0.7,
            dashArray: '10, 10', // Dashed pattern (10px line, 10px gap)
            lineCap: 'round'
        }}).addTo(map);

        // 4. Add actual driving route (OSRM)
        L.Routing.control({{
            waypoints: [
                L.latLng({startLat}, {startLon}),
                L.latLng({endLat}, {endLon})
            ],
            router: L.Routing.osrmv1({{
                serviceUrl: 'https://router.project-osrm.org/route/v1',
                profile: 'driving'
            }}),
            lineOptions: {{
                styles: [{{color: '#2196F3', opacity: 0.8, weight: 6}}] // Thick blue line
            }},
            createMarker: function() {{ return null; }}, // Disable automatic markers (using custom ones)
            addWaypoints: false,
            draggableWaypoints: false,
            fitSelectedRoutes: false, // Zoom is handled manually below
            show: false // Hide text instructions
        }}).addTo(map);

        // 5. Adjust zoom to fit all markers and lines
        var bounds = L.latLngBounds([startPoint, endPoint]);
        map.fitBounds(bounds, {{ padding: [50, 50] }});

    </script>
</body>
</html>";

            MapHtml = html;
        }
        catch (BO.BlDoesNotExistException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Map Update Failed (File locked?): {ex.Message}");
            MapHtml = "<html><body><h2>Order not found</h2></body></html>";
        }
        catch (BO.BlInvalidOperationException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Map Update Failed (File locked?): {ex.Message}");
            MapHtml = "<html><body><h2>Service temporarily unavailable</h2></body></html>";
        }
        catch (BlNullPropertyException ex) 
        {
            System.Diagnostics.Debug.WriteLine($"Map Update Failed (File locked?): {ex.Message}");
            MapHtml = "<html><body><h2>Error loading map</h2></body></html>";
        }
        catch (System.TimeoutException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Map Update Failed (File locked?): {ex.Message}");
            MapHtml = "<html><body><h2>Error loading map</h2></body></html>";
        }
    }

    /// <summary>
    /// Forces IE11 emulation for the WPF WebBrowser control.
    /// </summary>
    private void SetBrowserEmulation()
    {
        try
        {
            string appName = System.IO.Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            using RegistryKey key = Registry.CurrentUser.CreateSubKey(
                @"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION");
            key?.SetValue(appName, 11001, RegistryValueKind.DWord);
        }
        catch { }
    }

    #endregion

    #region Business Logic

    /// <summary>
    /// Asynchronously loads and filters the list of available orders for the current courier.
    /// </summary>
    /// <remarks>
    /// This method fetches orders from the Business Logic layer based on the current filter criteria:
    /// <list type="bullet">
    /// <item>If no filter is provided, it retrieves all available orders.</item>
    /// <item>If the filter matches a specific <see cref="BO.OrderRequirements"/>, it filters by that requirement.</item>
    /// <item>Otherwise, it performs a manual client-side search based on the order type string.</item>
    /// </list>
    /// The results are sorted according to the <c>SelectedSortProperty</c> and assigned to the <c>OpenOrders</c> property.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when a required argument is null.</exception>
    /// <exception cref="BlDoesNotExistException">Thrown when the requested courier or data is not found.</exception>
    /// <exception cref="BlInvalidOperationException">Thrown when the operation is invalid in the current state.</exception>
    /// <exception cref="BlInvalidInputException">Thrown when input parameters do not meet validation rules.</exception>
    private async Task LoadData()
    {
        try
        {
            IEnumerable<BO.OpenOrderInList> tempOrders;

            if (string.IsNullOrWhiteSpace(FilterText))
                tempOrders = await s_bl.Order.GetAvailableOrdersForCourier(_requesterId, _courierId, null, SelectedSortProperty);
            else if (Enum.TryParse(FilterText, true, out BO.OrderRequirements req))
                tempOrders = await s_bl.Order.GetAvailableOrdersForCourier(_requesterId, _courierId, req, SelectedSortProperty);
            else
            {
                var allAvailable = await s_bl.Order.GetAvailableOrdersForCourier(_requesterId, _courierId, null, SelectedSortProperty);
                tempOrders = allAvailable.Where(o => o.OrderType.ToString().Contains(FilterText)).ToList();
            }
            

            OpenOrders = tempOrders;
        }
        catch (ArgumentNullException ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (BlDoesNotExistException ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (BlInvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (BlInvalidInputException ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (BlExternalServiceException ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
       
    }

    // mutex to prevent concurrent LoadData calls
    private readonly ObserverMutex _OpenOrdersMutex = new(); //stage 7

    /// <summary>
    /// Observer callback to refresh the open orders list when changes occur in the BL layer.
    /// This method handles concurrency with the simulator and ensures thread safety.
    /// </summary>
    private void OpenOrdersObserver()
    {
        //  Concurrency Control: Prevent overlapping updates.
        // If an update is already in progress, mark a flag to restart later and exit immediately.
        if (_OpenOrdersMutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;

        Dispatcher.BeginInvoke(async () =>
        {
            try
            {
                //  Data Loading: Attempt to fetch the latest data on the UI thread.
                await LoadData();
            }
            //  Exception Handling:
            catch (System.IO.IOException ex)
            {
                System.Diagnostics.Debug.WriteLine($"File Locked (Simulator collision): {ex.Message}");
            }
            catch (BO.BlDoesNotExistException ex)
            {
                
                System.Diagnostics.Debug.WriteLine($"Data missing: {ex.Message}");

                // Safety measure: Clear the list to ensure the UI doesn't show stale items.
                OpenOrders = new List<BO.OpenOrderInList>();
            }
            catch (BO.BlInvalidOperationException ex)
            {
                
                System.Diagnostics.Debug.WriteLine($"Logic Error: {ex.Message}");
            }
            catch (System.Xml.XmlException ex)
            {
                
                System.Diagnostics.Debug.WriteLine($"XML Error: {ex.Message}");
            }
            finally
            {
                // Mutex Release
                if (await _OpenOrdersMutex.UnsetLoadInProgressAndCheckRestartRequested())
                    OpenOrdersObserver();
            }
        });
    }

    /// <summary>
    /// when the user change the filter box
    /// </summary>
    /// <param name="d"></param>
    /// <param name="e"></param>
    private static async void OnFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SelectedOrderWindow win)
           await win.LoadData();
    }

    /// <summary>
    /// when the user choose a sort parameter
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ComboBox_SortSelectionChanged(object sender, SelectionChangedEventArgs e) => OpenOrdersObserver();

    private async Task OnCollectOrder(BO.OpenOrderInList order)
    {
        if (order == null) return;

        if (MessageBox.Show($"Collect order {order.OrderId}?", "Confirm",
            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            try
            {
                await s_bl.Order.SelectOrderForHandling(_requesterId, _courierId, order.OrderId);
                MessageBox.Show("Order collected successfully!","Success", MessageBoxButton.OK, MessageBoxImage.Information);
                _previousWindow.Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }
    }

    /// <summary>
    /// back to the previous window
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnBack_Click(object sender, RoutedEventArgs e)
    {
        _previousWindow.Show();
        Close();
    }


    /// <summary>
    /// a button to open the address in google maps
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OpenInGoogleMaps_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedOrder == null) return;

        try
        {
            string url = $"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString(SelectedOrder.FullAddress)}";
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    #endregion
}
