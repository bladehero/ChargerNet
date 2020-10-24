using ChargerNet.Models;
using ChargerNet.Services;
using ChargerNet.Views;
using System;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace ChargerNet.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public static readonly Position StartPosition = new Position(46.469391, 30.740883);

        public IDataStore<Charger> ChargerStore => DependencyService.Get<IDataStore<Charger>>();

        public AboutViewModel()
        {
            Title = "Карта";

            InitializeMap();
        }

        public void InitializeMap()
        {
            Map = new Map()
            {
                IsShowingUser = true
            };
            Map.MoveToRegion(new MapSpan(StartPosition, 0.5, 0.5).WithZoom(2));
            try
            {
                var chargers = ChargerStore.GetItems();
                foreach (var charger in chargers)
                {
                    var pin = new Pin
                    {
                        Position = new Position(charger.Latitude, charger.Longitude),
                        Label = charger.PriceString,
                        Type = PinType.Generic,
                        Address = charger.Name,
                    };
                    pin.InfoWindowClicked += async (s,e) =>
                    {
                        await Shell.Current.GoToAsync($"{nameof(ItemDetailPage)}?{nameof(ItemDetailViewModel.ItemId)}={charger.Id}");
                    };
                    Map.Pins.Add(pin);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public Map Map { get; private set; }
    }
}