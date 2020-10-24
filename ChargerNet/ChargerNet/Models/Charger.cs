using ChargerNet.Globals;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;

namespace ChargerNet.Models
{
    [Table(nameof(Charger))]
    public class Charger : BaseModel
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public string PriceString => $"{Price}{Variables.UAH}/ч.";

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<UserCharger> UserChargers { get; set; }

        public Charger()
        {
            UserChargers = new List<UserCharger>();
        }
    }
}
