using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class Database<T>
{
    private readonly string _filePath;
    private readonly object _fileLock = new object();
    private readonly JsonSerializer _serializer;

    public Database(string filePath)
    {
        _filePath = filePath;
        _serializer = new JsonSerializer();
    }

    public async Task<List<T>> LoadDataAsync()
    {
        List<T> data;
        lock (_fileLock)
        {
            using (var fileStream = new FileStream(_filePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
            using (var reader = new StreamReader(fileStream))
            {
                var json = reader.ReadToEnd();
                data = JsonConvert.DeserializeObject<List<T>>(json);
            }
        }
        return data ?? new List<T>();
    }

    public async Task SaveDataAsync(List<T> data)
    {
        lock (_fileLock)
        {
            using (var fileStream = new FileStream(_filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(fileStream))
            {
                var json = JsonConvert.SerializeObject(data);
                writer.Write(json);
            }
        }
    }

    public async Task AddItemAsync(T item)
    {
        var data = await LoadDataAsync();
        data.Add(item);
        await SaveDataAsync(data);
    }

    public async Task RemoveItemAsync(Func<T, bool> predicate)
    {
        var data = await LoadDataAsync();
        Predicate<T> convertedPredicate = new Predicate<T>(predicate);
        data.RemoveAll(convertedPredicate);
        await SaveDataAsync(data);
    }

    public async Task EditItemAsync(Func<T, bool> predicate, Action<T> editAction)
    {
        var data = await LoadDataAsync();
        Predicate<T> convertedPredicate = new Predicate<T>(predicate);
        var itemToEdit = data.Find(convertedPredicate);
        if (itemToEdit != null)
        {
            editAction(itemToEdit);
            await SaveDataAsync(data);
        }
        else
        {
            throw new ArgumentException("Item not found.");
        }
    }

    public async Task<List<T>> QueryAsync(Expression<Func<T, bool>> predicate,
                                          Expression<Func<IEnumerable<T>, IOrderedEnumerable<T>>> orderBy = null,
                                          int? skip = null,
                                          int? take = null)
    {
        var data = await LoadDataAsync();
        var query = data.Where(predicate.Compile());

        if (orderBy != null)
            query = orderBy.Compile()(query);

        if (skip.HasValue)
            query = query.Skip(skip.Value);

        if (take.HasValue)
            query = query.Take(take.Value);

        return query.ToList();
    }
}   