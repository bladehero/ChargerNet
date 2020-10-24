using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ChargerNet.Models
{
    [Table(nameof(UserCharger))]
    public class UserCharger : BaseModel, INotifyPropertyChanged
    {
        [ForeignKey(typeof(User))]
        public int UserId { get; set; }
        [ForeignKey(typeof(Charger))]
        public int ChargerId { get; set; }
        public DateTime Reservation { get; set; }
        public int DurationMinutes { get; set; }

        [ManyToOne]
        public User User { get; set; }
        [ManyToOne]
        public Charger Charger { get; set; }

        [Ignore]
        public string ReservationDateString => Reservation.Date.ToString("dd.MM.yyyy");
        [Ignore]
        public DateTime ReservationTill => Reservation.AddMinutes(DurationMinutes);
        [Ignore]
        public string ReservationTimeString => Reservation.TimeOfDay.ToString(@"hh\:mm");
        [Ignore]
        public string ReservationTillTimeString => ReservationTill.TimeOfDay.ToString(@"hh\:mm");
        [Ignore]
        public string DurationMinutesString => $"{DurationMinutes}мин.";

        private bool isSelected;
        [Ignore]
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (EqualityComparer<bool>.Default.Equals(isSelected, value))
                    return;

                isSelected = value;
                OnPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            changed?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
