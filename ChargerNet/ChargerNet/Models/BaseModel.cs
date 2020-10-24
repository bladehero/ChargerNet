using SQLite;

namespace ChargerNet.Models
{
    public abstract class BaseModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public override bool Equals(object obj)
        {
            return obj is BaseModel model &&
                   obj.GetType().Equals(GetType()) &&
                   Id == model.Id;
        }

        public override int GetHashCode()
        {
            return 2108858624 + Id.GetHashCode();
        }
    }
}
