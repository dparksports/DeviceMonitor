using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DeviceMonitorCS.Models;

namespace DeviceMonitorCS
{
    public partial class ConnectionsWindow : Window
    {
        private ConnectionMonitor _monitor;
        private DispatcherTimer _timer;

        public ConnectionsWindow()
        {
            InitializeComponent();
            _monitor = new ConnectionMonitor();

            ActiveGrid.ItemsSource = _monitor.ActiveConnections;
            HistoryGrid.ItemsSource = _monitor.HistoricalConnections;
            MutedList.ItemsSource = _monitor.MutedConnections;

            // Timer
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            _timer.Tick += (s, e) => { if (AutoRefreshToggle.IsChecked == true) _monitor.RefreshConnections(); };
            _timer.Start();

            // Initial Load
            _monitor.RefreshConnections();

            // Event Handlers
            RefreshBtn.Click += (s, e) => _monitor.RefreshConnections();
            ClearHistoryBtn.Click += (s, e) => { _monitor.ClearHistory(); MessageBox.Show("History cleared."); };
            
            // Context Menus
            MuteActiveCtx.Click += (s, e) => MuteSelected(ActiveGrid);
            MuteHistoryCtx.Click += (s, e) => MuteSelected(HistoryGrid);
            
            CopyActiveIpCtx.Click += (s, e) => CopyIp(ActiveGrid);
            CopyHistoryIpCtx.Click += (s, e) => CopyIp(HistoryGrid);

            UnmuteCtx.Click += (s, e) => UnmuteSelected();
            
            // Double click unmute
            MutedList.MouseDoubleClick += (s, e) => UnmuteSelected();
            
            Closed += (s, e) => { _timer.Stop(); _monitor.SavePersistence(); };
        }

        private void MuteSelected(DataGrid grid)
        {
            if (grid.SelectedItem is ConnectionItem item)
            {
                var res = MessageBox.Show($"Mute all contributions from {item.RemoteAddress}?", "Confirm Mute", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    _monitor.MuteConnection(item);
                    _monitor.RefreshConnections(); // Refresh to remove muted items immediately
                }
            }
        }

        private void UnmuteSelected()
        {
            if (MutedList.SelectedItem is string ip)
            {
                 _monitor.UnmuteConnection(ip);
                 _monitor.RefreshConnections();
            }
        }

        private void CopyIp(DataGrid grid)
        {
             if (grid.SelectedItem is ConnectionItem item)
             {
                 Clipboard.SetText(item.RemoteAddress);
             }
        }
    }
}
