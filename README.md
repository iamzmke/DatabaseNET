<a href="Database.NET"><img src="http://readme-typing-svg.herokuapp.com?font=VT323&size=90&duration=2000&pause=1000&color=F70000&center=true&random=false&width=1100&height=140&lines=%E2%98%A6+Database.NET+%E2%98%A6;%E2%98%A6+By+Smoke+%E2%98%A6" alt="Database.NET" /></a>

# Database.NET

Database.NET is a C# class library designed to provide a simple and efficient way to manage JSON flat-file databases with support for medium datasets, including operations such as adding, removing, and editing items.

## Features

- **Asynchronous File I/O:** Utilizes asynchronous file I/O operations to improve responsiveness and performance, allowing the application to continue executing other tasks while waiting for I/O operations to complete.
- **Memory Caching:** Implements memory caching to reduce disk I/O operations by keeping the data in memory and only writing to the file when necessary.
- **Batch Processing:** Supports batch processing for operations like adding, removing, or editing multiple items, reducing the number of disk I/O operations.
- **Parallelism:** Utilizes parallelism for CPU-bound tasks, such as editing multiple items concurrently, to improve performance on multi-core processors.

## Usage

```csharp
// Assume we have a class representing our data model
public class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
}

// Initialize the database with the file path
var database = new Database<Person>("data.json");

// Add a new person to the database
var newPerson = new Person { Id = 1, Name = "John Doe", Age = 30 };
await database.AddItemAsync(newPerson);

// Remove a person from the database based on a predicate
await database.RemoveItemAsync(person => person.Id == 1);

// Edit a person's details in the database based on a predicate
await database.EditItemAsync(person => person.Name == "John Doe", person => { person.Age = 35; });

// Query data from the database based on a predicate
var adults = await database.QueryAsync(person => person.Age >= 18);

// Iterate through the queried data
foreach (var adult in adults)
{
    Console.WriteLine($"Name: {adult.Name}, Age: {adult.Age}");
}
