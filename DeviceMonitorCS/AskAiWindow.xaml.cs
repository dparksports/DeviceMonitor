using System;
using System.Text.Json;
using System.Windows;
using DeviceMonitorCS.Models;

namespace DeviceMonitorCS
{
    public partial class AskAiWindow : Window
    {
        private GeminiClient _client;
        private string _context;

        public AskAiWindow(object contextItem)
        {
            InitializeComponent();
            _client = new GeminiClient("AIzaSyDRKcIuOLFBMXWbWpf9KWHgEcuEJB6NWtQ"); // Key from user

            try
            {
                _context = JsonSerializer.Serialize(contextItem, new JsonSerializerOptions { WriteIndented = true });
            }
            catch
            {
                _context = contextItem.ToString();
            }

            ContextBox.Text = _context;
            QuestionBox.Focus();
            QuestionBox.SelectAll();

            AskBtn.Click += AskBtn_Click;
        }

        private async void AskBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(QuestionBox.Text)) return;

            string question = QuestionBox.Text;
            string prompt = $"Here is a JSON object representing a system entity (e.g., a process, network connection, or event):\n\n{_context}\n\nUser Question: {question}\n\nPlease provide a concise explanation.";
            
            ResponseBox.Text = "Thinking...";
            AskBtn.IsEnabled = false;
            QuestionBox.IsEnabled = false;

            try
            {
                string answer = await _client.AskAsync(prompt);
                ResponseBox.Text = answer;
            }
            catch (Exception ex)
            {
                ResponseBox.Text = $"Error: {ex.Message}";
            }
            finally
            {
                AskBtn.IsEnabled = true;
                QuestionBox.IsEnabled = true;
                QuestionBox.Focus();
            }
        }
    }
}
