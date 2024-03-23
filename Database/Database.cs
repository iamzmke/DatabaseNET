using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class Database<T>
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _fileSemaphore = new SemaphoreSlim(1, 1);
    private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

    public Database(string filePath)
    {
        _filePath = filePath;
    }

    public async Task<List<T>> LoadDataAsync()
    {
        string json;
        await _fileSemaphore.WaitAsync();
        try
        {
            json = await Task.Run(() => File.ReadAllText(_filePath));
        }
        finally
        {
            _fileSemaphore.Release();
        }
        return JsonConvert.DeserializeObject<List<T>>(json, _serializerSettings) ?? new List<T>();
    }

    public async Task SaveDataAsync(List<T> data)
    {
        var json = JsonConvert.SerializeObject(data, Formatting.Indented, _serializerSettings);
        await _fileSemaphore.WaitAsync();
        try
        {
            await Task.Run(() => File.WriteAllText(_filePath, json));
        }
        finally
        {
            _fileSemaphore.Release();
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