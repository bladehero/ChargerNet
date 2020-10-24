using ChargerNet.Globals;
using ChargerNet.Models;
using ChargerNet.Services;
using ChargerNet.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace ChargerNet.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public Command LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new Command(OnLoginClicked);
            if (string.IsNullOrWhiteSpace(Variables.CurrentUser.Current.Phone))
            {
                var deviceInfo = DependencyService.Get<IDeviceInfo>();
                Phone = deviceInfo.GetPhone();
            }
            else
            {
                Phone = Variables.CurrentUser.Current.Phone;
            }
            Name = Variables.CurrentUser.Current.Name;
        }

        private string name;
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        private string phone;
        public string Phone
        {
            get => phone;
            set => SetProperty(ref phone, value);
        }

        private async void OnLoginClicked(object obj)
        {
            var userStore = DependencyService.Get<IDataStore<User>>();
            var users = userStore.GetItems();
            var user = users.FirstOrDefault(x => x.Phone == Phone);
            if (user == null)
            {
                user = new User { Name = Name, Phone = Phone };
                var _ = userStore.AddItem(user);
            }
            else
            {
                user.Name = Name;
                var _ = userStore.UpdateItem(user);
            }

            Variables.CurrentUser.Current.Name = Name;
            Variables.CurrentUser.Current.Phone = Phone;
            Variables.CurrentUser.Current.User = user;
            Variables.CurrentUser.Current.Save();
            await Shell.Current.GoToAsync($"//{nameof(AboutPage)}");
        }
    }
}
