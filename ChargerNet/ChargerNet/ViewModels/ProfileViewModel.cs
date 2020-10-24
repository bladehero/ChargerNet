using ChargerNet.Globals;
using ChargerNet.Models;
using ChargerNet.Services;
using QRCoder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.Forms;

namespace ChargerNet.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private const int SizeQrCode = 20;

        private IDataStore<User> _userStore;
        private IDataStore<Charger> _chargerStore;
        private IDataStore<UserCharger> _userChargerStore;

        public Command<UserCharger> ItemTapped { get; }
        public Command QrCodeTapped { get; }

        private bool isExpandedQrCode;
        public bool IsExpandedQrCode
        {
            get => isExpandedQrCode;
            set
            {
                SetProperty(ref isExpandedQrCode, value);
            }
        }
        public ImageSource ImageSource
        {
            get
            {
                var generator = new QRCodeGenerator();
                var data = generator.CreateQrCode(Variables.CurrentUser.Current.Phone, QRCodeGenerator.ECCLevel.Q);
                var code = new BitmapByteQRCode(data);
                var bytes = code.GetGraphic(20);
                return ImageSource.FromStream(() => new MemoryStream(bytes));
            }
        }
        public ImageSource ImageSourceX2
        {
            get
            {
                var generator = new QRCodeGenerator();
                var data = generator.CreateQrCode(Variables.CurrentUser.Current.Phone, QRCodeGenerator.ECCLevel.Q);
                var code = new BitmapByteQRCode(data);
                var bytes = code.GetGraphic(SizeQrCode*2);
                return ImageSource.FromStream(() => new MemoryStream(bytes));
            }
        }

        public string Name => Variables.CurrentUser.Current.Name;
        public string Phone => Variables.CurrentUser.Current.Phone;
        public string Spent
        {
            get
            {
                var current = Variables.CurrentUser.Current.User;
                var cost = current.UserChargers?.Where(x => x.ReservationTill < DateTime.Now).Sum(x => x.Charger.Price);
                var value = "—";
                if (cost.GetValueOrDefault() > 0)
                {
                    value = $"{cost}{Variables.UAH}";
                }
                return value;
            }
        }
        public string Charged
        {
            get
            {
                var current = Variables.CurrentUser.Current.User;
                var minutes = current.UserChargers?.Where(x => x.ReservationTill < DateTime.Now).Sum(x => x.DurationMinutes);
                var duration = TimeSpan.FromMinutes(minutes.GetValueOrDefault());

                var value = "—";
                if ((int)duration.TotalDays > 0)
                {
                    value = duration.ToString(@"dd\.hh\:mm");
                }
                else if ((int)duration.TotalHours > 0)
                {
                    value = duration.ToString(@"hh\:mm");
                }
                else if ((int)duration.TotalMinutes > 0)
                {
                    value = "менее 1ч.";
                }
                return value;
            }
        }

        public bool HasPrevious => Previous?.Count() > 0;
        public bool HasNext => Next?.Count() > 0;
        public IEnumerable<UserCharger> Previous => Variables.CurrentUser.Current.User.UserChargers?.Where(x => x.ReservationTill < DateTime.Now);
        public IEnumerable<UserCharger> Next => Variables.CurrentUser.Current.User.UserChargers?.Except(Previous);

        public ProfileViewModel()
        {
            _userStore = DependencyService.Get<IDataStore<User>>();
            _chargerStore = DependencyService.Get<IDataStore<Charger>>();
            _userChargerStore = DependencyService.Get<IDataStore<UserCharger>>();

            ItemTapped = new Command<UserCharger>(OnItemSelected);
            QrCodeTapped = new Command(OnQrCodeTapped);
        }

        public async void OnAppearing()
        {
            var users = await _userStore.GetItemsAsync().ConfigureAwait(false);
            Variables.CurrentUser.Current.User = users.First(x => x.Phone == Phone);
            var userChargers = await _userChargerStore.GetItemsAsync().ConfigureAwait(false);
            Variables.CurrentUser.Current.User.UserChargers = userChargers.Where(x => x.UserId == Variables.CurrentUser.Current.User.Id).ToList();
            var chargers = await _chargerStore.GetItemsAsync().ConfigureAwait(false);
            foreach (var userCharger in Variables.CurrentUser.Current.User.UserChargers)
            {
                userCharger.Charger = chargers.FirstOrDefault(x => x.Id == userCharger.ChargerId);
            }
            OnPropertyChanged(nameof(Spent));
            OnPropertyChanged(nameof(Charged));
            OnPropertyChanged(nameof(Previous));
            OnPropertyChanged(nameof(HasPrevious));
            OnPropertyChanged(nameof(Next));
            OnPropertyChanged(nameof(HasNext));
        }

        private void OnItemSelected(UserCharger userCharger)
        {
            if (userCharger == null)
                return;

            Device.BeginInvokeOnMainThread(async () =>
            {
                var result = await App.Current.MainPage.DisplayAlert("Удаление",
                    $"Удалить бронь на {userCharger.ReservationTimeString}-{userCharger.ReservationTillTimeString} " +
                    $"в \"{userCharger.Charger.Name}\"", "OK", "Отмена");

                if (result)
                {
                    Variables.CurrentUser.Current.User.UserChargers.Remove(userCharger);
                    await _userChargerStore.DeleteItemAsync(userCharger).ConfigureAwait(false);

                    userCharger.IsSelected = !userCharger.IsSelected;

                    OnPropertyChanged(nameof(Spent));
                    OnPropertyChanged(nameof(Charged));
                    OnPropertyChanged(nameof(Previous));
                    OnPropertyChanged(nameof(HasPrevious));
                    OnPropertyChanged(nameof(Next));
                    OnPropertyChanged(nameof(HasNext));
                }
            });
        }

        private void OnQrCodeTapped()
        {
            IsExpandedQrCode = !IsExpandedQrCode;
        }
    }
}
