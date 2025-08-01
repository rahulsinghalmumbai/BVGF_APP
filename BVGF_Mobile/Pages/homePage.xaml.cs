﻿using BVGF.Connection;
using BVGF.Model;

using System.Collections.ObjectModel;

namespace BVGF.Pages;

public partial class homePage : ContentPage
{
    private ObservableCollection<MstMember> _members = new ObservableCollection<MstMember>();
    private readonly ApiService _apiService;
    private ObservableCollection<mstCategary> _categories = new ObservableCollection<mstCategary>();
    private Object currentEditingContact;
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
        await Navigation.PushAsync(new BVGF.Pages.history());
        
    }

    private async void OnSearchClicked(object sender, EventArgs e)
    {
        try
        {
            // Show loading
            ShowLoading(true, "Searching members...");

            var company = CompanyEntry.Text?.Trim();
            var selectedCategory = CategoryPicker.SelectedItem as mstCategary;
            long? categoryId = selectedCategory?.CategoryID;
            var name = NameEntry.Text?.Trim();
            var city = CityEntry.Text?.Trim();
            var mobile = MobileEntry.Text?.Trim();

            // Add small delay for better UX (optional)
            await Task.Delay(300);

            var members = await _apiService.GetMembersAsync(company, categoryId, name, city, mobile);

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
            CategoryPicker.SelectedItem = -1;
            NameEntry.Text = "";
            CityEntry.Text = "";
            MobileEntry.Text = "";

            _members.Clear();
            RecordCountLabel.Text = "Record : 0";

          
            await Task.Delay(200);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Failed to reset: " + ex.Message, "OK");
        }
        finally
        {
           
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
        EditCategoryPicker.ItemsSource = _categories;
    }

    private void OnContactTapped(object sender, EventArgs e)
    {
        var grid = sender as Grid;
        var contact = grid?.BindingContext; 

        // Contact detail view ?? show ????
        memberCollectionView.IsVisible = false;
        SearchSection.IsVisible = false;
        //FloatingButtons.IsVisible = false;
        ContactDetailView.IsVisible = true;
        BackButton.IsVisible = true;

        // Contact detail view ?? selected contact data ?? bind ????
        ContactDetailView.BindingContext = contact;
    }

  
    private void OnBackClicked(object sender, EventArgs e)
    {
        // List view ?? show ????
        ContactDetailView.IsVisible = false;
        BackButton.IsVisible = false;
        memberCollectionView.IsVisible = true;
        SearchSection.IsVisible = true;
        //FloatingButtons.IsVisible = true;
    }
    private async void OnCallTapped(object sender, EventArgs e)
    {
        try
        {
            var contact = ContactDetailView.BindingContext as MstMember;
            if (contact == null || string.IsNullOrWhiteSpace(contact.Mobile1))
            {
                await DisplayAlert("Error", "No valid contact information found", "OK");
                return;
            }

            // 1. Check if running on a physical device (emulators don't support calling)
            if (DeviceInfo.DeviceType == DeviceType.Virtual)
            {
                await DisplayAlert("Info", "Phone calls cannot be made from emulators", "OK");
                return;
            }

            // 2. Format the phone number
            var phoneNumber = FormatPhoneNumber(contact.Mobile1);
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                await DisplayAlert("Error", "Invalid phone number format", "OK");
                return;
            }

            // 3. Android-specific permission handling
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                // Check and request CALL_PHONE permission
                var status = await Permissions.CheckStatusAsync<Permissions.Phone>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.Phone>();
                    if (status != PermissionStatus.Granted)
                    {
                        await DisplayAlert("Permission Required",
                            "Phone permission is required to make calls", "OK");
                        return;
                    }
                }
            }

            // 4. Alternative approach using Launcher if PhoneDialer fails
            try
            {
                if (PhoneDialer.Default.IsSupported)
                {
                    PhoneDialer.Default.Open(phoneNumber);
                }
                else
                {
                    // Fallback to tel: URL scheme
                    await Launcher.OpenAsync($"tel:{phoneNumber}");
                }
            }
            catch (FeatureNotSupportedException)
            {
                // Final fallback
                await Launcher.OpenAsync($"tel:{phoneNumber}");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not initiate call: {ex.Message}", "OK");
        }
    }
    private string FormatPhoneNumber(string rawNumber)
    {
        if (string.IsNullOrWhiteSpace(rawNumber))
            return string.Empty;

        //// Remove all non-digit characters except '+' at start
        var digits = new string(rawNumber
            .Where((c, i) => char.IsDigit(c) || (i == 0 && c == '+'))
            .ToArray());

        //// Ensure minimum length (adjust according to your requirements)
        if (digits.Length < 10)
            return string.Empty;

        return digits;
    }
    private async void OnWhatsAppTapped(object sender, EventArgs e)
    {
        try
        {
            var contact = ContactDetailView.BindingContext as MstMember;
            if (contact == null)
            {
                await DisplayAlert("Error", "No contact selected", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(contact.Mobile1))
            {
                await DisplayAlert("Error", "No mobile number available for this contact", "OK");
                return;
            }

            var phoneNumber = FormatPhoneNumberForWhatsApp(contact.Mobile1);
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                await DisplayAlert("Error", "Invalid phone number format", "OK");
                return;
            }

            await OpenWhatsAppViaLauncher(contact, phoneNumber);
        }
        catch (Exception ex)
        {
            try
            {
                var contact = ContactDetailView.BindingContext as MstMember;
                if (contact != null)
                {
                    await Clipboard.Default.SetTextAsync(GenerateWhatsAppMessage(contact));
                    await DisplayAlert("Info", "WhatsApp not available. Contact details copied to clipboard.", "OK");
                }
            }
            catch
            {
                await DisplayAlert("Error", "Could not open WhatsApp. Please try again.", "OK");
            }
        }
    }
    private string FormatPhoneNumberForWhatsApp(string rawNumber)
    {
        if (string.IsNullOrWhiteSpace(rawNumber))
            return string.Empty;

        var digits = new string(rawNumber
            .Where((c, i) => char.IsDigit(c) || (i == 0 && c == '+'))
            .ToArray());

        digits = digits.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

        if (!digits.StartsWith("+"))
        {

            if (digits.StartsWith("0"))
                digits = digits.Substring(1);

            if (digits.Length == 10)
                digits = "+91" + digits;
            else if (digits.Length == 11 && digits.StartsWith("91"))
                digits = "+" + digits;
            else if (!digits.StartsWith("+"))
                digits = "+" + digits;
        }

        return digits;
    }

   
    private string GenerateWhatsAppMessage(MstMember contact)
    {
        var message = $"*Contact Details*\n\n";
        message += $"📝 *Name:* {contact.Name}\n";
        message += $"📱 *Mobile:* {contact.Mobile1}\n";

        if (!string.IsNullOrEmpty(contact.Mobile2))
            message += $"📞 *Alt Mobile:* {contact.Mobile2}\n";

        if (!string.IsNullOrEmpty(contact.Company))
            message += $"🏢 *Company:* {contact.Company}\n";

        if (!string.IsNullOrEmpty(contact.City))
            message += $"📍 *City:* {contact.City}\n";

        if (!string.IsNullOrEmpty(contact.Name))
            message += $"🏷️ *Category:* {contact.Name}\n";

        message += "\n📲 *Shared from BT Address Book*";

        return message;
    }

   
    private string GenerateShortWhatsAppMessage(MstMember contact)
    {
        var message = $"*{contact.Name}*\n";
        message += $"📱 {contact.Mobile1}\n";

        if (!string.IsNullOrEmpty(contact.Company))
            message += $"🏢 {contact.Company}\n";

        if (!string.IsNullOrEmpty(contact.City))
            message += $"📍 {contact.City}\n";

        message += "\n📲 BT Address Book";

        return message;
    }

  
    private async Task OpenWhatsAppViaLauncher(MstMember contact, string phoneNumber)
    {
        try
        {
            // Generate message
            var message = GenerateShortWhatsAppMessage(contact);
            var encodedMessage = System.Web.HttpUtility.UrlEncode(message);

            // Keep URI length under control
            if (encodedMessage.Length > 400)
            {
                message = $"*{contact.Name}*\n📱 {contact.Mobile1}\n\n📲 BT Address Book";
                encodedMessage = System.Web.HttpUtility.UrlEncode(message);
            }

            // Try different WhatsApp URL schemes
            await TryWhatsAppSchemes(phoneNumber, encodedMessage, contact);
        }
        catch (Exception ex)
        {
            // Try alternative approach
            await TryAlternativeWhatsApp(contact, phoneNumber);
        }
    }

    private async Task TryWhatsAppSchemes(string phoneNumber, string encodedMessage, MstMember contact)
    {
        var schemes = new List<string>();

        // Remove + from phone number for some schemes
        var cleanNumber = phoneNumber.Replace("+", "");

        // Different WhatsApp URL schemes to try
        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            schemes.Add($"https://wa.me/{cleanNumber}?text={encodedMessage}");
            schemes.Add($"whatsapp://send?phone={cleanNumber}&text={encodedMessage}");
            schemes.Add($"https://api.whatsapp.com/send?phone={cleanNumber}&text={encodedMessage}");
        }
        else if (DeviceInfo.Platform == DevicePlatform.iOS)
        {
            schemes.Add($"https://wa.me/{cleanNumber}?text={encodedMessage}");
            schemes.Add($"whatsapp://send?phone={cleanNumber}&text={encodedMessage}");
        }
        else
        {
            schemes.Add($"https://wa.me/{cleanNumber}?text={encodedMessage}");
        }

        // Try each scheme until one works
        foreach (var scheme in schemes)
        {
            try
            {
                var canOpen = await Launcher.CanOpenAsync(scheme);
                if (canOpen)
                {
                    await Task.Delay(100); // Small delay for stability
                    await Launcher.OpenAsync(scheme);
                    return; // Success, exit method
                }
            }
            catch (Exception ex)
            {
                // Continue to next scheme
                continue;
            }
        }

        // If all schemes fail, try alternative
        await TryAlternativeWhatsApp(contact, phoneNumber);
    }

    private async Task TryAlternativeWhatsApp(MstMember contact, string phoneNumber)
    {
        try
        {
            // Try opening WhatsApp without message first
            var schemes = new List<string>
        {
            "whatsapp://",
            "https://wa.me/",
            "https://web.whatsapp.com/"
        };

            foreach (var scheme in schemes)
            {
                try
                {
                    var canOpen = await Launcher.CanOpenAsync(scheme);
                    if (canOpen)
                    {
                        await Launcher.OpenAsync(scheme);

                        // Copy contact info to clipboard for manual sharing
                        await Task.Delay(1000); // Wait a bit
                        var contactInfo = GenerateWhatsAppMessage(contact);
                        await Clipboard.Default.SetTextAsync(contactInfo);

                        await DisplayAlert("Info",
                            "WhatsApp opened. Contact details copied to clipboard - paste in chat.", "OK");
                        return;
                    }
                }
                catch
                {
                    continue;
                }
            }

            // If WhatsApp can't be opened at all
            throw new Exception("WhatsApp not available");
        }
        catch
        {
            // Final fallback - just copy to clipboard
            await Clipboard.Default.SetTextAsync(GenerateWhatsAppMessage(contact));
            await DisplayAlert("Info",
                "WhatsApp not installed. Contact details copied to clipboard.", "OK");
        }
    }

   
    private async void OnWhatsAppWithOptionsClicked(object sender, EventArgs e)
    {
        try
        {
            var contact = ContactDetailView.BindingContext as MstMember;
            if (contact == null) return;

            // If contact has multiple mobile numbers, let user choose
            var mobileNumbers = new List<string>();
            if (!string.IsNullOrWhiteSpace(contact.Mobile1))
                mobileNumbers.Add(contact.Mobile1);
            if (!string.IsNullOrWhiteSpace(contact.Mobile2))
                mobileNumbers.Add(contact.Mobile2);

            if (mobileNumbers.Count == 0)
            {
                await DisplayAlert("Error", "No mobile numbers available", "OK");
                return;
            }

            if (mobileNumbers.Count == 1)
            {
                // Single number - send directly
                await SendWhatsAppToNumber(contact, mobileNumbers[0]);
            }
            else
            {
                // Multiple numbers - show selection
                var action = await DisplayActionSheet(
                    "Choose mobile number for WhatsApp:",
                    "Cancel",
                    null,
                    $"Mobile 1: {contact.Mobile1}",
                    $"Mobile 2: {contact.Mobile2}"
                );

                if (action != null && action != "Cancel")
                {
                    var selectedNumber = action.Contains("Mobile 1") ? contact.Mobile1 : contact.Mobile2;
                    await SendWhatsAppToNumber(contact, selectedNumber);
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"WhatsApp error: {ex.Message}", "OK");
        }
    }

    private async Task SendWhatsAppToNumber(MstMember contact, string phoneNumber)
    {
        var formattedNumber = FormatPhoneNumberForWhatsApp(phoneNumber);
        await OpenWhatsAppViaLauncher(contact, formattedNumber);
    }
    private async void OnSMSTapped(object sender, EventArgs e)
    {
        try
        {
            var contact = ContactDetailView.BindingContext as MstMember;
            if (contact == null)
            {
                await DisplayAlert("Error", "No contact selected", "OK");
                return;
            }

            // Check if we have a valid mobile number
            if (string.IsNullOrWhiteSpace(contact.Mobile1))
            {
                await DisplayAlert("Error", "No mobile number available for this contact", "OK");
                return;
            }

            // Format the phone number (remove any special characters)
            var phoneNumber = FormatPhoneNumberForSMS(contact.Mobile1);
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                await DisplayAlert("Error", "Invalid phone number format", "OK");
                return;
            }

            // Always use URL scheme approach - more stable
            await OpenSMSViaLauncher(contact, phoneNumber);
        }
        catch (Exception ex)
        {
            try
            {
                // Final fallback - copy to clipboard
                await Clipboard.Default.SetTextAsync(GenerateSMSBody(ContactDetailView.BindingContext as MstMember));
                await DisplayAlert("Info", "SMS app not available. Contact details copied to clipboard.", "OK");
            }
            catch
            {
                await DisplayAlert("Error", "Could not send SMS. Please try again.", "OK");
            }
        }
    }

  
    private string FormatPhoneNumberForSMS(string rawNumber)
    {
        if (string.IsNullOrWhiteSpace(rawNumber))
            return string.Empty;

        // Remove all non-digit characters except '+' at start
        var digits = new string(rawNumber
            .Where((c, i) => char.IsDigit(c) || (i == 0 && c == '+'))
            .ToArray());

        // Remove any spaces, hyphens, brackets, etc.
        digits = digits.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

        return digits;
    }

    private string GenerateSMSBody(MstMember contact)
    {
        var message = $"Contact Details:\n";
        message += $"Name: {contact.Name}\n";

        if (!string.IsNullOrEmpty(contact.Company))
            message += $"Company: {contact.Company}\n";

        if (!string.IsNullOrEmpty(contact.City))
            message += $"City: {contact.City}\n";

        message += $"Mobile: {contact.Mobile1}\n";

        if (!string.IsNullOrEmpty(contact.Mobile2))
            message += $"Alt Mobile: {contact.Mobile2}\n";

        if (!string.IsNullOrEmpty(contact.Name))
            message += $"Category: {contact.Name}\n";

        message += "\nShared from BT Address Book";

        return message;
    }
    private async Task OpenSMSViaLauncher(MstMember contact, string phoneNumber)
    {
        try
        {
            var body = GenerateShortSMSBody(contact);
            var encodedBody = System.Web.HttpUtility.UrlEncode(body);

            // Keep URI length under control
            if (encodedBody.Length > 500)
            {
                body = $"Contact: {contact.Name}\nMobile: {contact.Mobile1}\n\nFrom: BT Address Book";
                encodedBody = System.Web.HttpUtility.UrlEncode(body);
            }

            string smsUri;

            // Use simple SMS URI - more compatible
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                // Android SMS intent
                smsUri = $"sms:{phoneNumber}?body={encodedBody}";
            }
            else if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                // iOS SMS scheme
                smsUri = $"sms:{phoneNumber}&body={encodedBody}";
            }
            else
            {
                // Generic SMS scheme
                smsUri = $"sms:{phoneNumber}?body={encodedBody}";
            }

            // Use Launcher with proper error handling
            var canOpen = await Launcher.CanOpenAsync(smsUri);
            if (canOpen)
            {
                await Launcher.OpenAsync(smsUri);
            }
            else
            {
                throw new Exception("No SMS app available");
            }
        }
        catch (Exception ex)
        {
            // Try alternative approach
            await TryAlternativeSMS(contact, phoneNumber);
        }
    }
   
    private async Task TryAlternativeSMS(MstMember contact, string phoneNumber)
    {
        try
        {
            // Try with minimal data
            var simpleUri = $"sms:{phoneNumber}";
            await Launcher.OpenAsync(simpleUri);

            // If successful, copy details to clipboard for manual entry
            await Task.Delay(500); // Small delay
            var contactInfo = GenerateShortSMSBody(contact);
            await Clipboard.Default.SetTextAsync(contactInfo);

            await DisplayAlert("Info", "SMS app opened. Contact details copied to clipboard - paste in message.", "OK");
        }
        catch
        {
            // Final fallback - just copy to clipboard
            await Clipboard.Default.SetTextAsync(GenerateShortSMSBody(contact));
            await DisplayAlert("Info", "Contact details copied to clipboard", "OK");
        }
    }
    private string GenerateShortSMSBody(MstMember contact)
    {
        var message = $"Contact: {contact.Name}\n";
        message += $"Mobile: {contact.Mobile1}\n";

        if (!string.IsNullOrEmpty(contact.Company))
            message += $"Company: {contact.Company}\n";

        if (!string.IsNullOrEmpty(contact.City))
            message += $"City: {contact.City}\n";

        message += "From: BT Address Book";

        return message;
    }
  
    private async void OnSMSWithOptionsClicked(object sender, EventArgs e)
    {
        try
        {
            var contact = ContactDetailView.BindingContext as MstMember;
            if (contact == null) return;

            // If contact has multiple mobile numbers, let user choose
            var mobileNumbers = new List<string>();
            if (!string.IsNullOrWhiteSpace(contact.Mobile1))
                mobileNumbers.Add(contact.Mobile1);
            if (!string.IsNullOrWhiteSpace(contact.Mobile2))
                mobileNumbers.Add(contact.Mobile2);

            if (mobileNumbers.Count == 0)
            {
                await DisplayAlert("Error", "No mobile numbers available", "OK");
                return;
            }

            if (mobileNumbers.Count == 1)
            {
                await SendSMSToNumber(contact, mobileNumbers[0]);
            }
            else
            {
                var action = await DisplayActionSheet(
                    "Choose mobile number:",
                    "Cancel",
                    null,
                    $"Mobile 1: {contact.Mobile1}",
                    $"Mobile 2: {contact.Mobile2}"
                );

                if (action != null && action != "Cancel")
                {
                    var selectedNumber = action.Contains("Mobile 1") ? contact.Mobile1 : contact.Mobile2;
                    await SendSMSToNumber(contact, selectedNumber);
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"SMS error: {ex.Message}", "OK");
        }
    }
    private async Task SendSMSToNumber(MstMember contact, string phoneNumber)
    {
        var formattedNumber = FormatPhoneNumberForSMS(phoneNumber);

        if (Sms.Default.IsComposeSupported)
        {
            var smsMessage = new SmsMessage
            {
                Body = GenerateSMSBody(contact),
                Recipients = new List<string> { formattedNumber }
            };
            await Sms.Default.ComposeAsync(smsMessage);
        }
        else
        {
            await OpenSMSViaLauncher(contact, formattedNumber);
        }
    }
    private async void OnEmailTapped(object sender, EventArgs e)
    {
        try
        {
            var contact = ContactDetailView.BindingContext as MstMember;
            if (contact == null)
            {
                await DisplayAlert("Error", "No contact selected", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(contact.Email1))
            {
                await DisplayAlert("Error", "No email address available for this contact", "OK");
                return;
            }

            if (!Email.Default.IsComposeSupported)
            {
                // Fallback to mailto: URL scheme
                await OpenEmailViaLauncher(contact);
                return;
            }

            var message = new EmailMessage
            {
                Subject = $"Contact: {contact.Name}",
                Body = GenerateEmailBody(contact),
                // BodyFormat = EmailBodyFormat.PlainText, 
                To = new List<string> { contact.Email1 }
            };

           
            await Email.Default.ComposeAsync(message);
        }
        catch (FeatureNotSupportedException)
        {
            var contact = ContactDetailView.BindingContext as MstMember;
           
            await OpenEmailViaLauncher(contact);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not send email: {ex.Message}", "OK");
        }
    }

    private string GenerateEmailBody(MstMember contact)
    {
        return $@"Hii {contact.Name},";
    }
  

    private async Task OpenEmailViaLauncher(MstMember contact)
    {
        try
        {
            // Check if contact has email for fallback too
            if (string.IsNullOrWhiteSpace(contact.Email1))
            {
                await DisplayAlert("Error", "No email address available for this contact", "OK");
                return;
            }

            var subject = Uri.EscapeDataString($"Contact: {contact.Name}");
            var body = Uri.EscapeDataString(GenerateEmailBody(contact));
            var uri = $"mailto:{contact.Email1}?subject={subject}&body={body}";

            await Launcher.OpenAsync(uri);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error",
                $"No email clients available: {ex.Message}", "OK");

            // Final fallback - copy to clipboard
            await Clipboard.Default.SetTextAsync(GenerateEmailBody(contact));
            await DisplayAlert("Info",
                "Contact details copied to clipboard", "OK");
        }
    }


private void OnEditContactClicked(object sender, EventArgs e)
{
        if (ContactDetailView.BindingContext is MstMember contact)
        {
            currentEditingContact = contact;
            PopulateEditForm(contact);
            ShowEditPopup();
        }
}
    private void PopulateEditForm(MstMember contact)
    {
        try
        {
            // Basic Information
            EditNameEntry.Text = contact.Name ?? string.Empty;
            EditMobile1Entry.Text = contact.Mobile1 ?? string.Empty;
            EditMobile2Entry.Text = contact.Mobile2 ?? string.Empty;
            EditMobile3Entry.Text = contact.Mobile3 ?? string.Empty;
            EditTelephoneEntry.Text = contact.Telephone ?? string.Empty;

            // Email Information
            EditEmail1Entry.Text = contact.Email1 ?? string.Empty;
            EditEmail2Entry.Text = contact.Email2 ?? string.Empty;
            EditEmail3Entry.Text = contact.Email3 ?? string.Empty;

            // Address Information - Combine City and Address
            //EditCityEntry.Text = contact.City ?? string.Empty;
            EditAddressEditor.Text = string.IsNullOrEmpty(contact.Address)
                ? contact.City ?? string.Empty
                : $"{contact.City}, {contact.Address}";

            // Company Information - Combine Company, CompCity and CompAddress
            EditCompanyEntry.Text = string.IsNullOrEmpty(contact.CompAddress)
                ? contact.CompCity ?? string.Empty
                : $"{contact.CompCity}, {contact.CompAddress}";
            //EditCompanyAddressEditor.Text = string.IsNullOrEmpty(contact.CompAddress)
            //    ? contact.CompCity ?? string.Empty
            //    : $"{contact.CompCity}, {contact.CompAddress}";

            // Category
            if (!string.IsNullOrEmpty(contact.CategoryName))
            {
                var selectedCategory = _categories.FirstOrDefault(c =>
                    c.CategoryName.Equals(contact.CategoryName, StringComparison.OrdinalIgnoreCase));
                EditCategoryPicker.SelectedItem = selectedCategory;
            }
            else
            {
                EditCategoryPicker.SelectedItem = null;
            }

           
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error populating edit form: {ex.Message}");
        }
    }
    private async void OnSaveEditClicked(object sender, EventArgs e)
    {
        try
        {
            // First cast the currentEditingContact to MstMember
            if (!(currentEditingContact is MstMember originalContact))
            {
                await DisplayAlert("Error", "Invalid contact data", "OK");
                return;
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(EditNameEntry.Text))
            {
                await DisplayAlert("Validation Error", "Name is required.", "OK");
                return;
            }
            // Show loading indicator
            LoadingOverlay.IsVisible = true;
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            LoadingText.IsVisible = true;
            LoadingText.TextColor = Colors.Black;
            LoadingText.Text = "Updating contact...";

            // Split address and city
            var addressParts = EditAddressEditor.Text?.Split(new[] { ',' }, 2, StringSplitOptions.RemoveEmptyEntries)
                                .Select(part => part.Trim())
                                .ToArray();

            // Split company address and city
            var companyAddressParts = EditCompanyEntry.Text?.Split(new[] { ',' }, 3, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(p => p.Trim())
                                 .ToArray();

            // Create or update the member object
            var member = new MstMember
            {
                // Preserve original ID
                MemberID = originalContact.MemberID,

                // Update fields from form
                Name = EditNameEntry.Text.Trim(),
                Address = addressParts?.Length > 1 ? addressParts[1] : null,
                City = addressParts?.Length > 0 ? addressParts[0] : null,
                Mobile1 = EditMobile1Entry.Text.Trim(),
                Mobile2 = string.IsNullOrWhiteSpace(EditMobile2Entry.Text) ? null : EditMobile2Entry.Text.Trim(),
                Mobile3 = string.IsNullOrWhiteSpace(EditMobile3Entry.Text) ? null : EditMobile3Entry.Text.Trim(),
                Telephone = string.IsNullOrWhiteSpace(EditTelephoneEntry.Text) ? null : EditTelephoneEntry.Text.Trim(),
                Email1 = EditEmail1Entry.Text.Trim(),
                Email2 = string.IsNullOrWhiteSpace(EditEmail2Entry.Text) ? null : EditEmail2Entry.Text.Trim(),
                Email3 = string.IsNullOrWhiteSpace(EditEmail3Entry.Text) ? null : EditEmail3Entry.Text.Trim(),
                Company = companyAddressParts?.Length > 0 ? companyAddressParts[0] : null,
                CompAddress = companyAddressParts?.Length > 1 ? companyAddressParts[1] : null,
                CompCity = companyAddressParts?.Length > 2 ? companyAddressParts[2] : null,
                CategoryName = (EditCategoryPicker.SelectedItem as mstCategary)?.CategoryName,

                // Preserve original values
                //Status = originalContact.Status,
                //CreatedBy = originalContact.CreatedBy,
                //CreatedDt = originalContact.CreatedDt,
                DOB = originalContact.DOB,

                UpdatedBy = 1, 
                UpdatedDt = DateTime.Now
            };

            var apiResponse = await _apiService.UpsertMemberAsync(member);

            // Hide loading
            LoadingOverlay.IsVisible = false;
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            LoadingText.IsVisible = false;

            if (apiResponse?.Status == "200" && apiResponse.Data != null)
            {
                // Update the current editing contact with the response data
                currentEditingContact = apiResponse.Data;
                // Update the UI binding context to reflect changes
                ContactDetailView.BindingContext = null;
                ContactDetailView.BindingContext = currentEditingContact;

                // Hide popup
                HideEditPopup();

                // Show success message
                await DisplayAlert("Success", "Contact updated successfully!", "OK");
            }
            else
            {
                await DisplayAlert("Error", apiResponse?.Message ?? "Failed to update contact. Please try again.", "OK");
            }
        }
        catch (Exception ex)
        {
            // Hide loading
            LoadingOverlay.IsVisible = false;
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            LoadingText.IsVisible = false;

            // Show error message
            await DisplayAlert("Error", "Failed to update contact: " + ex.Message, "OK");
        }
    }
    // Method to show edit popup
    private void ShowEditPopup()
{
    EditContactOverlay.IsVisible = true;

    // Optional: Add fade-in animation
    EditContactOverlay.FadeTo(1, 250);
}

// Method to hide edit popup
private void HideEditPopup()
{
    EditContactOverlay.FadeTo(0, 250).ContinueWith(t =>
    {
        Device.BeginInvokeOnMainThread(() =>
        {
            EditContactOverlay.IsVisible = false;
        });
    });
}

// Method to handle close popup button
private void OnCloseEditPopupClicked(object sender, EventArgs e)
{
    HideEditPopup();
}

// Method to handle cancel edit button
private void OnCancelEditClicked(object sender, EventArgs e)
{
    HideEditPopup();
}

// Optional: Handle back button to close popup if it's open
protected override bool OnBackButtonPressed()
{
    if (EditContactOverlay.IsVisible)
    {
        HideEditPopup();
        return true;
    }

    if (ContactDetailView.IsVisible)
    {
        OnBackClicked(null, null);
        return true;
    }

    return base.OnBackButtonPressed();
}
}