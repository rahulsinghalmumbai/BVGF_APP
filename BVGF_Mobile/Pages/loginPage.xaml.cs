using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace BVGF.Pages
{
    public partial class loginPage : ContentPage
    {
        private bool isPasswordVisible = false;

        public loginPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text))
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
                // Simulate API call
                await Task.Delay(2000);

                // TODO: Replace with actual authentication logic
                bool isLoginSuccessful = await AuthenticateUser(UsernameEntry.Text, PasswordEntry.Text);

                if (isLoginSuccessful)
                {
                    await DisplayAlert("Success", "Login successful!", "OK");
                    await Navigation.PushAsync(new homePage());
                    // TODO: Navigate to main page
                    // await Shell.Current.GoToAsync("//MainPage");
                }
                else
                {
                    await DisplayAlert("Error", "Invalid username or password", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Login failed: {ex.Message}", "OK");
            }
            finally
            {
                // Hide loading indicator
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
                LoginButton.IsEnabled = true;
            }
        }

        private void OnTogglePasswordClicked(object sender, EventArgs e)
        {
            isPasswordVisible = !isPasswordVisible;
            PasswordEntry.IsPassword = !isPasswordVisible;
            TogglePasswordButton.Text = isPasswordVisible ? "🙈" : "👁️";
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



        private async Task<bool> AuthenticateUser(string username, string password)
        {
            // TODO: Replace with actual authentication logic
            // This is just a dummy implementation
            await Task.Delay(1000); // Simulate network delay

            // For demo purposes, accept any non-empty credentials
            return !string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password);
        }

        // Override back button behavior if needed
        protected override bool OnBackButtonPressed()
        {
            // TODO: Handle back button press if needed
            return base.OnBackButtonPressed();
        }
    }
}