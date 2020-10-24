using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;

namespace ChargerNet.Models
{
    [Table(nameof(User))]
    public class User : BaseModel
    {
        public string Name { get; set; }
        [Unique]
        public string Phone { get; set; }


        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<UserCharger> UserChargers { get; set; }

        public User()
        {
            UserChargers = new List<UserCharger>();
        }
    }
}
