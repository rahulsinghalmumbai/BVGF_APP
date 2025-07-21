namespace BVGF.Pages;

public partial class homePage : ContentPage
{
	public homePage()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
    }
    private async void OnHistoryClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new history());
    }
    private async void SearchButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new searchContent());
    }
}