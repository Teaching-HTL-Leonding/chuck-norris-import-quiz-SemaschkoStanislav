using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

// HttpClient is intended to be instantiated once per application, rather than per-use. See Remarks.
HttpClient client = new();

var factory = new CookbookContextFactory();
using var dbContext = factory.CreateDbContext(args);

Console.WriteLine("Enter a number of joke u want to generate: ");
var userInput =  Convert.ToInt32(Console.ReadLine());

var numOfJokes = userInput >= 1 && userInput <= 10 ? userInput : 5;

for (int i = 0; i < numOfJokes; i++)
{
    try
    {
        HttpResponseMessage response = await client.GetAsync("https://api.chucknorris.io/jokes/random");
        response.EnsureSuccessStatusCode();
        string jsonString = await response.Content.ReadAsStringAsync();

        var jsonObject = JsonSerializer.Deserialize<JsonObject>(jsonString);

        var newJoke = new ChuckNorisJoke {ChuckNorrisId = jsonObject.Id,Url = jsonObject.Url, Joke = jsonObject.Value };
        dbContext.Jokes.Add(newJoke);
        await dbContext.SaveChangesAsync();
    }
    catch (HttpRequestException e)
    {
        Console.WriteLine("\nException Caught!");
        Console.WriteLine("Message :{0} ", e.Message);
    }
}

class JsonObject
{
    [JsonPropertyName("categories")]
    public string[] Categories { get; set; }

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; }

    [JsonPropertyName("icon_url")]
    public string IconUrl { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("updated_at")]
    public string UpdatedAt { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }
}

// Create the model class
class ChuckNorisJoke
{
    public int Id { get; set; }

    [MaxLength(40)]
    public string ChuckNorrisId { get; set; }

    [MaxLength(1024)]
    public string Url { get; set; }

    public string Joke { get; set; }

}

class CookbookContext : DbContext
{
    public DbSet<ChuckNorisJoke> Jokes { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public CookbookContext(DbContextOptions<CookbookContext> options)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        : base(options)
    { }

}

class CookbookContextFactory : IDesignTimeDbContextFactory<CookbookContext>
{
    public CookbookContext CreateDbContext(string[]? args = null)
    {
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        var optionsBuilder = new DbContextOptionsBuilder<CookbookContext>();
        optionsBuilder
            // Uncomment the following line if you want to print generated
            // SQL statements on the console.
            //.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
            .UseSqlServer(configuration["ConnectionStrings:DefaultConnection"]);

        return new CookbookContext(optionsBuilder.Options);
    }

}