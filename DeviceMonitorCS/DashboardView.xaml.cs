using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using DeviceMonitorCS.Models;

namespace DeviceMonitorCS
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        public void BindData(ObservableCollection<SecurityEvent> securityEvents, ObservableCollection<DeviceEvent> deviceEvents)
        {
            SecurityGrid.ItemsSource = securityEvents;
            DeviceGrid.ItemsSource = deviceEvents;
        }
        
        // Backward compatibility shim if needed
        public void BindActivity(ObservableCollection<SecurityEvent> events) 
        {
            SecurityGrid.ItemsSource = events;
        }

        public void UpdateLiveStatus(string status, string colorType)
        {
            Dispatcher.Invoke(async () => 
            {
                StatusText.Text = status;
                
                // Color mapping
                Brush targetBrush = (Brush)Application.Current.Resources["NeonGreenBrush"];
                Color targetColor = (Color)Application.Current.Resources["NeonGreen"];

                if (colorType == "Red")
                {
                    targetBrush = (Brush)Application.Current.Resources["NeonRedBrush"];
                    targetColor = (Color)Application.Current.Resources["NeonRed"];
                    NetworkText.Text = "Intervention";
                }
                else if (colorType == "Amber")
                {
                    targetBrush = (Brush)Application.Current.Resources["NeonAmberBrush"];
                    targetColor = (Color)Application.Current.Resources["NeonAmber"];
                    TaskText.Text = "Alert";
                }

                // Apply to Elements
                // We primarily update the "System Security" card for the main status
                StatusText.Foreground = targetBrush;
                SecurityIcon.Foreground = targetBrush;
                SecurityGlow.Color = targetColor;

                // Also update Network Shield for red alerts
                if (colorType == "Red") {
                    NetworkIcon.Foreground = targetBrush;
                    NetworkGlow.Color = targetColor;
                    NetworkText.Foreground = targetBrush;
                }

                // Auto-revert logic
                if (colorType != "Green")
                {
                    await Task.Delay(5000);
                    
                    // Revert System Security
                    StatusText.Text = "Protected";
                    StatusText.Foreground = (Brush)Application.Current.Resources["NeonGreenBrush"];
                    SecurityIcon.Foreground = (Brush)Application.Current.Resources["NeonGreenBrush"];
                    SecurityGlow.Color = (Color)Application.Current.Resources["NeonGreen"];

                    // Revert Network Shield
                    NetworkText.Text = "Active";
                    NetworkText.Foreground = (Brush)Application.Current.Resources["NeonBlueBrush"];
                    NetworkIcon.Foreground = (Brush)Application.Current.Resources["NeonBlueBrush"];
                    NetworkGlow.Color = (Color)Application.Current.Resources["NeonBlue"];

                    // Revert Tasks
                    TaskText.Text = "Scanning...";
                }
            });
        }

        private void AskAi_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var contextMenu = menuItem.Parent as ContextMenu;
            var grid = contextMenu.PlacementTarget as DataGrid;

            if (grid != null && grid.SelectedItem != null)
            {
                var window = new AskAiWindow(grid.SelectedItem);
                window.Owner = System.Windows.Window.GetWindow(this);
                window.ShowDialog();
            }
        }
    }
}
