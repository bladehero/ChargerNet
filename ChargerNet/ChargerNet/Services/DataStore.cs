using SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ChargerNet.Services
{

    public abstract class DataStore<T> : IDataStore<T> where T : class, new()
    {
        public string TableName => Connection.TableMappings.First(x => x.MappedType == typeof(T)).TableName;

        private string connectionString;
        public string ConnectionString
        {
            get
            {
                if (connectionString == null)
                {
                    connectionString = DependencyService.Get<string>();
                }
                return connectionString;
            }
        }
        public SQLiteAsyncConnection AsyncConnection => new SQLiteAsyncConnection(ConnectionString);
        public SQLiteConnection Connection => new SQLiteConnection(ConnectionString);

        public virtual async Task<bool> AddItemAsync(T item)
        {
            return await AsyncConnection.InsertAsync(item, typeof(T)) > 0;
        }
        public virtual bool AddItem(T item)
        {
            return Connection.Insert(item, typeof(T)) > 0;
        }

        public virtual async Task<bool> UpdateItemAsync(T item)
        {
            return await AsyncConnection.UpdateAsync(item, typeof(T)) > 0;
        }
        public virtual bool UpdateItem(T item)
        {
            return Connection.Update(item, typeof(T)) > 0;
        }

        public virtual async Task<bool> DeleteItemAsync(T item)
        {
            return await AsyncConnection.DeleteAsync(item) > 0;
        }
        public virtual bool DeleteItem(T item)
        {
            return Connection.Delete(item) > 0;
        }

        public virtual async Task<bool> ExistsAsync(int id)
        {
            if (id > decimal.Zero)
            {
                return false;
            }
            var count = await AsyncConnection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {TableName} WHERE ID=?", id);
            return count > 0;
        }
        public virtual bool Exists(int id)
        {
            if (id > decimal.Zero)
            {
                return false;
            }
            var count = Connection.ExecuteScalar<int>($"SELECT COUNT(*) FROM {TableName} WHERE ID=?", id);
            return count > 0;
        }

        public virtual async Task<T> GetItemAsync(int id)
        {
            if (id == decimal.Zero)
            {
                return null;
            }
            return await AsyncConnection.GetAsync<T>(id);
        }
        public virtual T GetItem(int id)
        {
            if (id == decimal.Zero)
            {
                return null;
            }
            return Connection.Get<T>(id);
        }

        public virtual async Task<IEnumerable<T>> GetItemsAsync()
        {
            return await AsyncConnection.Table<T>().ToListAsync();
        }
        public virtual IEnumerable<T> GetItems()
        {
            return Connection.Table<T>().ToList();
        }
    }
}