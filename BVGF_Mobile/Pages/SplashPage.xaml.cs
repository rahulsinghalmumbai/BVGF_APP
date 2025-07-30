using BVGF_Mobile;
using System.Threading.Tasks;

namespace BVGF.Pages;
  
public partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        InitializeComponent();
        StartTimer();
    }

    private async void StartTimer()
    {   
        await Task.Delay(3000);
        //Application.Current.MainPage = new AppShell(); 
        await Navigation.PushAsync(new loginPage());
       //await Navigation.PushAsync(new homePage());

    }
}
