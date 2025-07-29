using BVGF.Connection;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace BVGF.Pages
{
    public partial class loginPage : ContentPage
    {
        private bool isPasswordVisible = false;
        private readonly ApiService _apiService;

        public loginPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            _apiService = new ApiService();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                await DisplayAlert("Error", "Please enter both username and password", "OK");
                return;
            }

            // Show loading indicator
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            LoginButton.IsEnabled = false;

            try
            {
                //await Task.Delay(2000);

                string isLoginSuccessful = await _apiService.LoginAsync(PasswordEntry.Text.Trim());

                if (!string.IsNullOrEmpty(isLoginSuccessful))
                {
                    await Navigation.PushAsync(new homePage());

                }
                else
                {
                    await DisplayAlert("Error", "Invalid username ", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Login failed: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
                LoginButton.IsEnabled = true;
            }
        }



        private async void OnForgotPasswordTapped(object sender, EventArgs e)
        {
            string email = await DisplayPromptAsync("Forgot Password",
                "Please enter your email address:",
                "Send Reset Link",
                "Cancel",
                "Email address",
                keyboard: Keyboard.Email);

            if (!string.IsNullOrWhiteSpace(email))
            {
                // TODO: Implement forgot password functionality
                await DisplayAlert("Password Reset",
                    $"Password reset link has been sent to {email}",
                    "OK");
            }
        }

        private async void OnSignUpTapped(object sender, EventArgs e)
        {

            await Navigation.PushAsync(new signupPage());
            //await Navigation.PushAsync(new homePage());

        }



        private async Task<bool> AuthenticateUser(string password)
        {
            // TODO: Replace with actual authentication logic
            // This is just a dummy implementation
            await Task.Delay(1000); // Simulate network delay

            // For demo purposes, accept any non-empty credentials
            return !string.IsNullOrWhiteSpace(password);
        }

        // Override back button behavior if needed
        protected override bool OnBackButtonPressed()
        {
            // TODO: Handle back button press if needed
            return base.OnBackButtonPressed();
        }
    }
}