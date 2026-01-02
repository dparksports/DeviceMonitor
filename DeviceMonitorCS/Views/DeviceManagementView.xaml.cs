using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Management;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DeviceMonitorCS.Views
{
    public partial class DeviceManagementView : UserControl
    {
        public ObservableCollection<DeviceInventoryItem> InventoryList { get; set; } = new ObservableCollection<DeviceInventoryItem>();
        public ObservableCollection<DeviceHistoryItem> HistoryList { get; set; } = new ObservableCollection<DeviceHistoryItem>();

        public DeviceManagementView()
        {
            InitializeComponent();
            InventoryGrid.ItemsSource = InventoryList;
            HistoryGrid.ItemsSource = HistoryList;

            Loaded += (s, e) => RefreshData();
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        private async void RefreshData()
        {
            await LoadInventory();
            await LoadHistory();
        }

        private async Task LoadInventory()
        {
            InventoryList.Clear();
            int kbdCount = 0;
            int mouseCount = 0;
            int monCount = 0;

            await Task.Run(() =>
            {
                // Keyboards
                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Keyboard"))
                    {
                        foreach (var device in searcher.Get())
                        {
                            kbdCount++;
                            AddInventory("Keyboard", device);
                        }
                    }
                }
                catch { }

                // Mice
                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PointingDevice"))
                    {
                        foreach (var device in searcher.Get())
                        {
                            mouseCount++;
                            AddInventory("Mouse", device);
                        }
                    }
                }
                catch { }

                // Monitors
                // Win32_DesktopMonitor often fails on modern Windows; fallback to PnP entity if needed, but try standard first
                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DesktopMonitor"))
                    {
                        foreach (var device in searcher.Get())
                        {
                            monCount++;
                            AddInventory("Monitor", device);
                        }
                    }
                }
                catch { }
            });

            KeyboardCountFormatted.Text = kbdCount.ToString();
            MouseCountFormatted.Text = mouseCount.ToString();
            MonitorCountFormatted.Text = monCount.ToString();
        }

        private void AddInventory(string category, ManagementBaseObject device)
        {
            Dispatcher.Invoke(() =>
            {
                InventoryList.Add(new DeviceInventoryItem
                {
                    Category = category,
                    Name = device["Name"]?.ToString() ?? "Unknown",
                    Manufacturer = device["Manufacturer"]?.ToString(),
                    Status = device["Status"]?.ToString(),
                    DeviceId = device["DeviceID"]?.ToString()
                });
            });
        }

        private async Task LoadHistory()
        {
            HistoryList.Clear();

            await Task.Run(() =>
            {
                try
                {
                    // Kernel-PnP Event IDs:
                    // 400: Device Configured (often means "Ready to use" after plug-in)
                    // 410: Device Started
                    // 420: Device Deleted (Unplugged/Removed)
                    string query = "*[System[(EventID=400 or EventID=410)]]";
                    
                    var elq = new EventLogQuery("Microsoft-Windows-Kernel-PnP/Configuration", PathType.LogName, query) { ReverseDirection = true };
                    using (var reader = new EventLogReader(elq))
                    {
                        EventRecord eventInstance;
                        while ((eventInstance = reader.ReadEvent()) != null)
                        {
                            string desc = eventInstance.FormatDescription();
                            
                            // Naive filter for Keyboard/Mouse/Monitor to avoid spamming all PnP events
                            // Checking if the description contains keywords or if we can parse the properties
                            // Usually FormatDescription contains the Name.
                            
                            bool relevant = desc.Contains("Keyboard", StringComparison.OrdinalIgnoreCase) ||
                                            desc.Contains("Mouse", StringComparison.OrdinalIgnoreCase) ||
                                            desc.Contains("Monitor", StringComparison.OrdinalIgnoreCase) ||
                                            desc.Contains("Display", StringComparison.OrdinalIgnoreCase) ||
                                            desc.Contains("HID", StringComparison.OrdinalIgnoreCase);

                            if (relevant)
                            {
                                var item = new DeviceHistoryItem
                                {
                                    Time = eventInstance.TimeCreated?.ToString("yyyy-MM-dd HH:mm:ss"),
                                    EventName = eventInstance.Id == 400 ? "Configured" : (eventInstance.Id == 410 ? "Started" : $"Event {eventInstance.Id}"),
                                    DeviceName = desc
                                };
                                
                                Dispatcher.Invoke(() => HistoryList.Add(item));
                            }

                            if (HistoryList.Count > 50) break; // Limit 50
                        }
                    }
                }
                catch (Exception ex)
                {
                     Dispatcher.Invoke(() => HistoryList.Add(new DeviceHistoryItem { EventName = "Error", DeviceName = ex.Message }));
                }
            });
        }
    }

    public class DeviceInventoryItem
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public string Status { get; set; }
        public string DeviceId { get; set; }
    }

    public class DeviceHistoryItem
    {
        public string Time { get; set; }
        public string EventName { get; set; }
        public string DeviceName { get; set; }
    }
}
