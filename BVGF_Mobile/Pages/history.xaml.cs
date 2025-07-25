using System;
using Microsoft.Maui.Controls;

namespace BVGF.Pages
{
    public partial class history : ContentView
    {
        public event EventHandler CloseRequested;

        public history()
        {
            InitializeComponent();
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        // Add methods to load/manage history data
        public void LoadHistoryData()
        {
            // Your data loading logic here
        }
    }
}