using Microsoft.Maui.Controls;

namespace BVGF.Pages
{
    public partial class history : ContentPage
    {
        public history()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            LoadHistoryData();
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            // Navigate back to previous page
            await Navigation.PopAsync();
        }

        private void LoadHistoryData()
        {
            // Sample data - आप यहाँ अपना actual data load कर सकते हैं
            var historyRecords = new List<HistoryRecord>
            {
                new HistoryRecord
                {
                    CompanyName = "Tech Solutions Pvt Ltd",
                    CategoryName = "IT Services",
                    Name = "vivek",
                    City = "Delhi",
                    Mobile = "",
                    Time = "3 hour ago"
                },
                new HistoryRecord
                {
                    CompanyName = "Marketing Hub",
                    CategoryName = "Advertising",
                    Name = "karnal",
                    City = "Haryana",
                    Mobile = "",
                    Time = "1 month ago"
                },
                new HistoryRecord
                {
                    CompanyName = "Telecom Services",
                    CategoryName = "Communication",
                    Name = "",
                    City = "",
                    Mobile = "9560548422",
                    Time = "3 month ago"
                }
            };

            // Clear existing content (except header)
            HistoryContainer.Children.Clear();

            // Add records dynamically
            foreach (var record in historyRecords)
            {
                AddHistoryRecord(record);
            }
        }

        private void AddHistoryRecord(HistoryRecord record)
        {
            // Create main grid for the record
            var recordGrid = new Grid
            {
                BackgroundColor = Color.FromArgb("#FAFAFA"),
                Padding = 5
            };

            // Define columns
            recordGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2.5, GridUnitType.Star) });
            recordGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2.5, GridUnitType.Star) });
            recordGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            recordGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Define rows
            recordGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            recordGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Company row labels
            recordGrid.Add(new Label
            {
                Text = record.CompanyName,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#333"),
                Padding = 5
            }, 0, 0);

            recordGrid.Add(new Label
            {
                Text = record.CategoryName,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#333"),
                Padding = 5
            }, 1, 0);

            recordGrid.Add(new Label
            {
                Text = "",
                FontSize = 12,
                Padding = 5
            }, 2, 0);

            // Time cell (merged)
            var timeFrame = new Frame
            {
                BackgroundColor = Color.FromArgb("#F9F9F9"),
                Padding = 5,
                HasShadow = false,
                Content = new Label
                {
                    Text = record.Time,
                    FontSize = 10,
                    TextColor = Color.FromArgb("#666"),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            };

            recordGrid.Add(timeFrame, 3, 0);
            Grid.SetRowSpan(timeFrame, 2);

            // Person row labels
            recordGrid.Add(new Label
            {
                Text = record.Name,
                FontSize = 12,
                TextColor = Color.FromArgb("#555"),
                Padding = 5
            }, 0, 1);

            recordGrid.Add(new Label
            {
                Text = record.City,
                FontSize = 12,
                TextColor = Color.FromArgb("#555"),
                Padding = 5
            }, 1, 1);

            recordGrid.Add(new Label
            {
                Text = record.Mobile,
                FontSize = 12,
                TextColor = Color.FromArgb("#00796B"),
                FontAttributes = FontAttributes.Bold,
                Padding = 5
            }, 2, 1);

            // Add to container
            HistoryContainer.Children.Add(recordGrid);

            // Add separator
            HistoryContainer.Children.Add(new BoxView
            {
                HeightRequest = 2,
                BackgroundColor = Color.FromArgb("#00BCD4")
            });
        }
    }

    // Data model class
    public class HistoryRecord
    {
        public string CompanyName { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public string Name { get; set; } = "";
        public string City { get; set; } = "";
        public string Mobile { get; set; } = "";
        public string Time { get; set; } = "";
    }
}