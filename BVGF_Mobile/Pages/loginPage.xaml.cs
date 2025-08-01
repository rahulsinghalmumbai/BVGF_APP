using BVGF.Connection;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace BVGF.Pages
{
    public partial class loginPage : ContentPage
    {
        private readonly ApiService _apiService;
        private readonly SimCardService _simService;

        public loginPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            _apiService = new ApiService();
            _simService = new SimCardService();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            // Step 1: Basic input validation
            if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                await DisplayAlert("Error", "Please enter mobile number", "OK");
                return;
            }

            if (PasswordEntry.Text.Length != 10)
            {
                await DisplayAlert("Error", "Please enter a valid 10-digit mobile number", "OK");
                return;
            }

            // Show loading indicator
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            LoginButton.IsEnabled = false;
            LoginButton.Text = "Verifying...";

            try
            {
                bool hasPermission = await _simService.HasPermissionAsync();
                if (!hasPermission)
                {
                    await DisplayAlert("Permission Required",
                        "Phone permission is required to verify your SIM card number for security purposes.",
                        "OK");
                    return;
                }

                LoginButton.Text = "Checking SIM...";
                var verification = await _simService.VerifyPhoneNumberAsync(PasswordEntry.Text.Trim());

                if (verification.Result != VerificationResult.Success)
                {
                    string availableNumbers = await GetAvailableNumbersMessage();

                    await DisplayAlert(" Verification Failed",
                        $"The entered mobile number ({PasswordEntry.Text}) is not found in your device .\n\n" +
                       // $"Available numbers: {availableNumbers}\n\n" +
                        "This verification is required for security purposes.",
                        "OK");
                    return;
                }

                LoginButton.Text = "Logging in...";
                string isLoginSuccessful = await _apiService.LoginAsync(PasswordEntry.Text.Trim());

                if (!string.IsNullOrEmpty(isLoginSuccessful))
                {
                    // Login successful
                   // await DisplayAlert("Success", "Login successful!", "OK");
                    await Navigation.PushAsync(new homePage());
                }
                else
                {
                    await DisplayAlert("Login Failed",
                        "Invalid mobile number or login credentials. Please check your number and try again.",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Login failed: {ex.Message}", "OK");
            }
            finally
            {
                // Reset UI
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
                LoginButton.IsEnabled = true;
                LoginButton.Text = "LOGIN";
            }
        }

        // Helper method to get available numbers message
        private async Task<string> GetAvailableNumbersMessage()
        {
            try
            {
                var simCards = await _simService.GetAllSimCardInfoAsync();
                string message = "";

                foreach (var sim in simCards)
                {
                    if (!string.IsNullOrEmpty(sim.PhoneNumber))
                    {
                        message += $"• {sim.PhoneNumber} ({sim.CarrierName})\n";
                    }
                    else
                    {
                        message += $"• Number not available ({sim.CarrierName})\n";
                    }
                }

                return string.IsNullOrEmpty(message) ? "No SIM numbers available" : message;
            }
            catch
            {
                return "Unable to retrieve SIM numbers";
            }
        }

        // Method to show user their SIM numbers
        private async void OnShowMyNumbersClicked(object sender, EventArgs e)
        {
            try
            {
                LoadingIndicator.IsVisible = true;
                LoadingIndicator.IsRunning = true;

                bool hasPermission = await _simService.HasPermissionAsync();
                if (!hasPermission)
                {
                    await DisplayAlert("Permission Required",
                        "Phone permission is required to access your SIM card information.",
                        "OK");
                    return;
                }

                var verification = await _simService.GetSimInfoSummaryAsync();

                await DisplayAlert("Your SIM Information", verification, "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to get SIM information: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
            }
        }

        // Rest of your existing methods...
        private async void OnForgotPasswordTapped(object sender, EventArgs e)
        {
            // ... existing implementation ...
        }

        private async void OnSignUpTapped(object sender, EventArgs e)
        {
            // ... existing implementation ...
        }

        protected override bool OnBackButtonPressed()
        {
            // ... existing implementation ...
            return base.OnBackButtonPressed();
        }
    }
}