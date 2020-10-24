using ChargerNet.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ChargerNet.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProfilePage : ContentPage
    {
        ProfileViewModel _viewModel;

        public ProfilePage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new ProfileViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}