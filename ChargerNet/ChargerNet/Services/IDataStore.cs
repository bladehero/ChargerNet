using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargerNet.Services
{
    public interface IDataStore<T>
    {
        SQLiteAsyncConnection AsyncConnection { get; }
        SQLiteConnection Connection { get; }
        string ConnectionString { get; }

        bool AddItem(T item);
        Task<bool> AddItemAsync(T item);
        bool DeleteItem(T item);
        Task<bool> DeleteItemAsync(T item);
        T GetItem(int id);
        Task<T> GetItemAsync(int id);
        bool Exists(int id);
        Task<bool> ExistsAsync(int id);
        IEnumerable<T> GetItems();
        Task<IEnumerable<T>> GetItemsAsync();
        bool UpdateItem(T item);
        Task<bool> UpdateItemAsync(T item);
    }
}
