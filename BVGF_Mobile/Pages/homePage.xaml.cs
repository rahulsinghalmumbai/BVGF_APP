using BVGF.Connection;
using BVGF.Model;
using System.Collections.ObjectModel;

namespace BVGF.Pages;

public partial class homePage : ContentPage
{
    private ObservableCollection<MstMember> _members = new ObservableCollection<MstMember>();
    private readonly ApiService _apiService;
    private ObservableCollection<mstCategary> _categories = new ObservableCollection<mstCategary>();

    public ObservableCollection<MstMember> Members => _members;

    public homePage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        BindingContext = this;
        memberCollectionView.ItemsSource = _members;
        
        _apiService = new ApiService();
         LoadCategoriesAsync();
        RecordCountLabel.Text = "Record : 0";
        _members.Clear();
    }

    private void ShowLoading(bool show, string message = "Searching...")
    {
        LoadingOverlay.IsVisible = show;
        LoadingIndicator.IsVisible = show;
        LoadingIndicator.IsRunning = show;
        LoadingText.IsVisible = show;
        LoadingText.Text = message;

        // Disable search button during loading
        SearchButton.IsEnabled = !show;
        SearchButton.Text = show ? "Searching..." : "Search";
    }

    private async void OnHistoryClicked(object sender, EventArgs e)
    {
        //await Navigation.PushAsync(new HistoryView());
    }

    private async void OnSearchClicked(object sender, EventArgs e)
    {
        try
        {
            // Show loading
            ShowLoading(true, "Searching members...");

            var company = CompanyEntry.Text?.Trim();
            var category = CategoryPicker.SelectedItem?.ToString();
            var name = NameEntry.Text?.Trim();
            var city = CityEntry.Text?.Trim();
            var mobile = MobileEntry.Text?.Trim();

            // Add small delay for better UX (optional)
            await Task.Delay(300);

            var members = await _apiService.GetMembersAsync(company, category, name, city, mobile);

            _members.Clear();
            foreach (var m in members)
                _members.Add(m);

            RecordCountLabel.Text = $"Record : {_members.Count}";

            // Show message if no results found
            if (_members.Count == 0)
            {
                await DisplayAlert("Info", "No records found matching your search criteria.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Something went wrong: " + ex.Message, "OK");
        }
        finally
        {
            ShowLoading(false);
        }
    }

    private async void OnResetClicked(object sender, EventArgs e)
    {
        try
        {
            ShowLoading(true, "Resetting...");

            // Clear all search fields
            CompanyEntry.Text = "";
            CategoryPicker.SelectedItem=-1;
            NameEntry.Text = "";
            CityEntry.Text = "";
            MobileEntry.Text = "";

            // Clear members list
            _members.Clear();
            RecordCountLabel.Text = "Record : 0";

            // Small delay for UX
            await Task.Delay(200);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Failed to reset: " + ex.Message, "OK");
        }
        finally
        {
            // Hide loading
            ShowLoading(false);
        }
    }
    private async Task LoadCategoriesAsync()
    {
        var categoryList = await _apiService.GetCategoriesAsync();
        _categories.Clear();
        foreach (var cat in categoryList)
            _categories.Add(cat);

        CategoryPicker.ItemsSource = _categories;
    }

    // Contact ?? tap ???? ?? detail view show ???? ?? ???
    private void OnContactTapped(object sender, EventArgs e)
    {
        var grid = sender as Grid;
        var contact = grid?.BindingContext; // Selected contact data

        // Contact detail view ?? show ????
        memberCollectionView.IsVisible = false;
        SearchSection.IsVisible = false;
        FloatingButtons.IsVisible = false;
        ContactDetailView.IsVisible = true;
        BackButton.IsVisible = true;

        // Contact detail view ?? selected contact data ?? bind ????
        ContactDetailView.BindingContext = contact;
    }

    // Back button click ???? ?? list view ?? ???? ???? ?? ???
    private void OnBackClicked(object sender, EventArgs e)
    {
        // List view ?? show ????
        ContactDetailView.IsVisible = false;
        BackButton.IsVisible = false;
        memberCollectionView.IsVisible = true;
        SearchSection.IsVisible = true;
        FloatingButtons.IsVisible = true;
    }

    // Action button handlers
    private void OnCallTapped(object sender, EventArgs e)
    {
        // Call functionality implement ????
        var contact = ContactDetailView.BindingContext;
        // Phone call logic here
    }

    private void OnWhatsAppTapped(object sender, EventArgs e)
    {
        // WhatsApp functionality implement ????
        var contact = ContactDetailView.BindingContext;
        // WhatsApp logic here
    }

    private void OnSMSTapped(object sender, EventArgs e)
    {
        // SMS functionality implement ????
        var contact = ContactDetailView.BindingContext;
        // SMS logic here
    }

    private void OnEmailTapped(object sender, EventArgs e)
    {
        // Email functionality implement ????
        var contact = ContactDetailView.BindingContext;
        // Email logic here
    }

    private void OnShareTapped(object sender, EventArgs e)
    {
        // Share functionality implement ????
        var contact = ContactDetailView.BindingContext;
        // Share logic here
    }

    private void OnEditContactClicked(object sender, EventArgs e)
    {
        // Edit contact functionality
        var contact = ContactDetailView.BindingContext;
        // Navigate to edit page
    }

    private void OnDeleteContactClicked(object sender, EventArgs e)
    {
        // Delete contact functionality
        var contact = ContactDetailView.BindingContext;
        // Show confirmation dialog and delete
    }

    // Optional: Method to load all members initially
    private async Task LoadAllMembersAsync()
    {
        try
        {
            ShowLoading(true, "Loading members...");

            var members = await _apiService.GetMembersAsync(null, null, null, null, null);

            _members.Clear();
            foreach (var m in members)
                _members.Add(m);

            RecordCountLabel.Text = $"Record : {_members.Count}";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Failed to load members: " + ex.Message, "OK");
        }
        finally
        {
            ShowLoading(false);
        }
    }

    // Call this in constructor if you want to load all members initially
    // protected override async void OnAppearing()
    // {
    //     base.OnAppearing();
    //     if (_members.Count == 0)
    //         await LoadAllMembersAsync();
    // }
}