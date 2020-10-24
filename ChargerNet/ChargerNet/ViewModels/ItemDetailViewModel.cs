using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using ChargerNet.Globals;
using ChargerNet.Models;
using ChargerNet.Services;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace ChargerNet.ViewModels
{
    [QueryProperty(nameof(ItemId), nameof(ItemId))]
    public class ItemDetailViewModel : BaseViewModel
    {
        private static readonly TimeSpan StartOfDay = new TimeSpan(9, 00, 0);
        private static readonly TimeSpan EndOfDay = new TimeSpan(21, 00, 0);
        private const int DefaultPeriod = 30;

        private IDataStore<Charger> _chargerStore;
        private IDataStore<UserCharger> _userChargerStore;

        private Charger charger;
        public Charger Charger
        {
            get => charger;
            set
            {
                charger = value;
                OnPropertyChanged(nameof(SelectedPriceString));
            }
        }
        public ObservableCollection<UserCharger> Items { get; private set; }
        public ObservableCollection<UserCharger> SelectedItems { get; private set; }
        public Command<UserCharger> ItemTapped { get; }
        public Command Confirm { get; }
        public Map Map { get; private set; }

        public ItemDetailViewModel()
        {
            _chargerStore = DependencyService.Get<IDataStore<Charger>>();
            _userChargerStore = DependencyService.Get<IDataStore<UserCharger>>();
            ItemTapped = new Command<UserCharger>(OnItemSelected);
            Confirm = new Command(OnConfirm);
            Map = new Map()
            {
                IsShowingUser = true
            };

            Items = new ObservableCollection<UserCharger>();
            SelectedItems = new ObservableCollection<UserCharger>();
            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
            ReservationDate = DateTime.Now;
        }

        private void SelectedItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(IsSelected));
            OnPropertyChanged(nameof(SelectedDurationString));
            OnPropertyChanged(nameof(SelectedPriceString));
        }

        #region Bindable Properties
        public bool IsSelected => SelectedItems?.Count() > 0;
        public string SelectedDurationString
        {
            get
            {
                if (SelectedItems?.Count() == 0)
                {
                    return string.Empty;
                }

                var minutes = SelectedItems.Sum(x => DefaultPeriod);
                return TimeSpan.FromMinutes(minutes).ToString(@"hh\:mm");
            }
        }
        public string SelectedPriceString
        {
            get
            {
                if (Charger == null || SelectedItems?.Count() == 0)
                {
                    return string.Empty;
                }

                var price = SelectedItems.Sum(x => Charger.Price);
                return $"{price}{Variables.UAH}";
            }
        }
        private string name;
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        private string price;
        public string Price
        {
            get => price;
            set => SetProperty(ref price, value);
        }

        private int itemId;
        public string ItemId
        {
            get
            {
                return itemId.ToString();
            }
            set
            {
                int.TryParse(value, out itemId);
                LoadItemId();
            }
        }

        private DateTime _reservationDate = DateTime.Now;
        public DateTime ReservationDate
        {
            get => _reservationDate;
            set
            {
                SetChargerItems(value);
                SetProperty(ref _reservationDate, value);
            }
        }
        public DateTime MinDate => DateTime.Now;

        public async void LoadItemId()
        {
            try
            {
                Charger = await _chargerStore.GetItemAsync(itemId);
                Title = Name = Charger.Name;
                Price = Charger.PriceString;

                var position = new Position(Charger.Latitude, Charger.Longitude);
                Map.MoveToRegion(new MapSpan(position, 0.01, 0.01));
                try
                {
                    var pin = new Pin
                    {
                        Position = position,
                        Label = Charger.PriceString,
                        Type = PinType.Generic,
                        Address = Charger.Name,
                    };
                    Map.Pins.Add(pin);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("Failed to Load Item");
            }
        }
        #endregion

        private async void SetChargerItems(DateTime dateTime)
        {
            Items.Clear();
            SelectedItems.Clear();
            var now = DateTime.Now;

            TimeSpan start;
            if (now.TimeOfDay > StartOfDay && dateTime.Date == now.Date)
            {
                var temp = now.AddHours(1.5).TimeOfDay;
                start = new TimeSpan(temp.Hours, temp.Minutes - (temp.Minutes % 30), 0);
            }
            else
            {
                start = StartOfDay;
            }

            var userChargers = await _userChargerStore.GetItemsAsync();

            var userId = User.Id;
            while (start < EndOfDay)
            {
                var userCharger = new UserCharger
                {
                    ChargerId = itemId,
                    UserId = userId,
                    DurationMinutes = DefaultPeriod,
                    Reservation = dateTime.Date.Add(start)
                };

                var registered = userChargers.Where(x => x.ChargerId == itemId).FirstOrDefault(x =>
                {
                    var betweenStart = x.Reservation <= userCharger.Reservation && userCharger.Reservation < x.ReservationTill;
                    var betweenEnd = x.Reservation < userCharger.ReservationTill && userCharger.ReservationTill <= x.ReservationTill;
                    return betweenStart || betweenEnd;
                });

                if (registered == null)
                {
                    Items.Add(userCharger);
                }
                else if (registered.UserId == userId)
                {
                    registered.IsSelected = true;
                    SelectedItems.Add(registered);
                    Items.Add(registered);
                }

                start = start.Add(new TimeSpan(0, DefaultPeriod, 0));
            }
        }

        private async void OnItemSelected(UserCharger userCharger)
        {
            if (userCharger == null)
                return;

            if (userCharger.IsSelected)
            {
                SelectedItems.Remove(userCharger);
                await _userChargerStore.DeleteItemAsync(userCharger).ConfigureAwait(false);
            }
            else
            {
                SelectedItems.Add(userCharger);
            }

            userCharger.IsSelected = !userCharger.IsSelected;
        }

        private async void OnConfirm()
        {
            foreach (var selected in SelectedItems)
            {
                var exists = await _userChargerStore.ExistsAsync(selected.Id).ConfigureAwait(false);
                if (exists)
                {
                    var existed = await _userChargerStore.GetItemAsync(selected.Id).ConfigureAwait(false);
                    if (existed.UserId == selected.UserId)
                    {
                        await _userChargerStore.UpdateItemAsync(selected).ConfigureAwait(false);
                    }
                }
                else
                {
                    await _userChargerStore.AddItemAsync(selected).ConfigureAwait(false);
                }
            }
            Device.BeginInvokeOnMainThread(async () =>
            {
                await App.Current.MainPage.Navigation.PopToRootAsync();
            });
        }
    }
}
