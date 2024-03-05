using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Database
{
    public class Database<T>
    {
        private readonly string _filePath;
        private List<T> _data;

        public Database(string filePath)
        {
            _filePath = filePath;
            _data = new List<T>();
            LoadDataAsync().Wait();
        }

        private async Task LoadDataAsync()
        {
            if (File.Exists(_filePath))
            {
                string jsonData;
                using (var fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
                using (var reader = new StreamReader(fileStream))
                {
                    jsonData = await reader.ReadToEndAsync().ConfigureAwait(false);
                }
                _data = JsonConvert.DeserializeObject<List<T>>(jsonData);
            }
        }

        private async Task SaveDataAsync()
        {
            string jsonData = JsonConvert.SerializeObject(_data, Formatting.Indented);
            using (var fileStream = new FileStream(_filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            using (var writer = new StreamWriter(fileStream))
            {
                await writer.WriteAsync(jsonData).ConfigureAwait(false);
            }
        }

        public async Task AddItemAsync(T item)
        {
            _data.Add(item);
            await SaveDataAsync().ConfigureAwait(false);
        }

        public async Task RemoveItemAsync(Func<T, bool> predicate)
        {
            Predicate<T> convertedPredicate = new Predicate<T>(predicate);
            _data.RemoveAll(convertedPredicate);
            await SaveDataAsync().ConfigureAwait(false);
        }

        public async Task EditItemAsync(Func<T, bool> predicate, Action<T> editAction)
        {
            Predicate<T> convertedPredicate = new Predicate<T>(predicate);
            var itemToEdit = _data.Find(convertedPredicate);
            if (itemToEdit != null)
            {
                editAction(itemToEdit);
                await SaveDataAsync().ConfigureAwait(false);
            }
            else
            {
                throw new ArgumentException("Item not found.");
            }
        }

        public List<T> GetAllItems()
        {
            return _data;
        }
    }
}
