using MongoDB.Driver;
using Microsoft.AspNetCore.Mvc;
using Dashboard.Application.Database.Contexts;
using Dashboard.Application.Entities;
using MongoDB.Bson;

namespace Dashboard.API.Controllers;

public class AddEventMessageRequest
{
    public EventType EventType { get; set; }

    public int Cost { get; set; }

    public MessageEvent ToModel()
    => new()
    {
        _id = ObjectId.Empty,
        GeneratedAt = DateTime.UtcNow,
        Cost = Cost,
        EventType = EventType
    };
}

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpPost("add")]
    public IActionResult Post(AddEventMessageRequest dto)
    {
        var connectionString = "mongodb://admin:examplepassword@localhost:27017/";

        var client = new MongoClient(connectionString);

        var db = ApplicationDbContext.Create(client.GetDatabase("dashboard"));

        var movie = db.MessageEvents.Add(dto.ToModel());

        db.SaveChanges();

        return Ok(movie.Entity);
    }

    [HttpPost("seed")]
    public IActionResult Seed()
    {
        var connectionString = "mongodb://admin:examplepassword@localhost:27017/";

        var client = new MongoClient(connectionString);

        var db = ApplicationDbContext.Create(client.GetDatabase("dashboard"));

        var startDate = DateTime.UtcNow;

        var events = new MessageEvent[50];

        foreach (var i in Enumerable.Range(1, 100_000))
        {
            if (i % 500 == 0)
            {
                startDate = startDate.AddDays(1);
                Console.WriteLine("Good morning princess");
            }

            var p = new MessageEvent
            {
                _id = ObjectId.Empty,
                GeneratedAt = startDate,
                Cost = i,
                EventType = (EventType)(i % 3)
            };

            events[i % 50] = p;

            if (i % 50 == 0)
            {
                db.MessageEvents.AddRange(events);

                db.SaveChanges();
            }

        }

        return Ok("IMPORTED");
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IActionResult Get(DateTime from, DateTime to)
    {
        var connectionString = "mongodb://admin:examplepassword@localhost:27017/";

        var client = new MongoClient(connectionString);

        var db = ApplicationDbContext.Create(client.GetDatabase("dashboard"));

        var movie = db.MessageEvents
          .Select(x => new { x.Cost, x.EventType, x.GeneratedAt })
          .Where(x => x.Cost > 0)
          .Where(x => x.GeneratedAt < to && x.GeneratedAt >= from)
          .ToList() // Run the Query on database
          .GroupBy(x => x.GeneratedAt)
          .Select(x => new
          {
              Day = x.Key,
              Data = x.GroupBy(y => y.EventType)
                        .Select(y => new
                        {
                            EventType = y.Key.ToString(),
                            TotalCost = y.Sum(z => z.Cost),
                            Count = y.Count()
                        })
          })
          ;

        return Ok(movie);
    }
}
