using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;
using ChargerNet.Models;
using ChargerNet.Views;
using ChargerNet.Services;
using System.Linq;
using System.Collections.Generic;

namespace ChargerNet.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {
        private IDataStore<Charger> _chargerStore;
        private IDataStore<UserCharger> _userChargerStore;

        #region Bindable Properties
        private Charger _selectedItem;
        public Charger SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                OnItemSelected(value);
            }
        }

        private bool _allRegisteredSelected;
        public bool AllRegisteredSelected
        {
            get => _allRegisteredSelected;
            set => SetProperty(ref _allRegisteredSelected, value);
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _priceMin;
        public string PriceMin
        {
            get => _priceMin;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    SetProperty(ref _priceMin, value);
                else if (int.TryParse(value, out int current))
                    SetProperty(ref _priceMin, current.ToString());
                else
                    ResetProperty();
            }
        }

        private string _priceMax;
        public string PriceMax
        {
            get => _priceMax;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    SetProperty(ref _priceMax, value);
                else if (int.TryParse(value, out int current))
                    SetProperty(ref _priceMax, current.ToString());
                else
                    ResetProperty();
            }
        }

        private string _durationMinutes;
        public string DurationMinutes
        {
            get => _durationMinutes;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    SetProperty(ref _durationMinutes, value);
                else if (int.TryParse(value, out int current))
                    SetProperty(ref _durationMinutes, current.ToString());
                else
                    ResetProperty();
            }
        }

        private DateTime _reservationDate = DateTime.Now;
        public DateTime ReservationDate
        {
            get => _reservationDate;
            set => SetProperty(ref _reservationDate, value);
        }

        private TimeSpan _reservationTime = (DateTime.Now - DateTime.Now.Date).Add(new TimeSpan(1, 0, 0));
        public TimeSpan ReservationTime
        {
            get => _reservationTime;
            set => SetProperty(ref _reservationTime, value);
        }
        public DateTime MinDate => DateTime.Now;
        #endregion

        public ObservableCollection<Charger> Items { get; private set; }
        public Command LoadItemsCommand { get; }
        public Command AddItemCommand { get; }
        public Command<Charger> ItemTapped { get; }

        public ItemsViewModel()
        {
            Title = "Поиск";
            IsExpanded = true;

            _chargerStore = DependencyService.Get<IDataStore<Charger>>();
            _userChargerStore = DependencyService.Get<IDataStore<UserCharger>>();

            Items = new ObservableCollection<Charger>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
            ItemTapped = new Command<Charger>(OnItemSelected);
            AddItemCommand = new Command(OnAddItem);

            AllRegisteredSelected = true;
        }

        private async Task ExecuteLoadItemsCommand()
        {
            IsBusy = true;

            try
            {
                var chargers = await Search();
                Items.Clear();
                foreach (var charger in chargers)
                {
                    Items.Add(charger);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async void OnAppearing()
        {
            IsBusy = true;
            SelectedItem = null;

            var chargers = await _chargerStore.GetItemsAsync().ConfigureAwait(false);
            if (chargers.Any())
            {
                PriceMin = chargers.Min(x => x.Price).ToString();
            }
            if (chargers.Any())
            {
                PriceMax = chargers.Max(x => x.Price).ToString();
            }

        }

        private async void OnAddItem(object obj)
        {
            IsExpanded = false;
            await ExecuteLoadItemsCommand();
        }

        private async Task<IEnumerable<Charger>> Search()
        {
            var date = ReservationDate.Date.Add(ReservationTime);

            var registered = await _userChargerStore.GetItemsAsync().ConfigureAwait(false);
            int.TryParse(DurationMinutes, out int duration);
            var till = date.AddMinutes(duration);
            registered = registered.Where(x => 
            {
                var betweenStart = x.Reservation <= date && date < x.ReservationTill;
                var betweenEnd = x.Reservation < till && till <= x.ReservationTill;
                return betweenStart || betweenEnd;
            });

            var userId = User.Id;
            var chargers = await _chargerStore.GetItemsAsync().ConfigureAwait(false);
            var notRegisteredByUser = !registered.All(x => x.UserId == userId);
            if (registered?.Count() > 0 && (notRegisteredByUser || !AllRegisteredSelected))
            {
                chargers = chargers.Where(x => !registered.Any(r => r.ChargerId == x.Id));
            }
            if (!string.IsNullOrWhiteSpace(Name))
            {
                chargers = chargers.Where(x => x.Name.Contains(Name));
            }
            if (int.TryParse(PriceMin, out int min))
            {
                chargers = chargers.Where(x => x.Price >= min);
            }
            if (int.TryParse(PriceMax, out int max))
            {
                chargers = chargers.Where(x => x.Price <= max);
            }

            return chargers;
        }

        private async void OnItemSelected(Charger charger)
        {
            if (charger == null)
                return;

            await Shell.Current.GoToAsync($"{nameof(ItemDetailPage)}?{nameof(ItemDetailViewModel.ItemId)}={charger.Id}");
        }
    }
}