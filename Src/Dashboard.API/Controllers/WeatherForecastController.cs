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
public class WeatherForecastController(MongoClient _client) : ControllerBase
{

    [HttpPost("add")]
    public IActionResult Post(AddEventMessageRequest dto)
    {
        var db = ApplicationDbContext.Create(_client.GetDatabase("dashboard"));

        var movie = db.MessageEvents.Add(dto.ToModel());

        db.SaveChanges();

        return Ok(movie.Entity);
    }

    [HttpPost("seed")]
    public IActionResult Seed()
    {
        var db = ApplicationDbContext.Create(_client.GetDatabase("dashboard"));

        var startDate = DateTime.UtcNow;

        var events = new MessageEvent[50];

        foreach (var i in Enumerable.Range(1, 100))
        {
            if (i % 5 == 0)
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

            var remainItens = events[..(i % 50)];

            if (i == 100 && remainItens.Any())
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
        var db = ApplicationDbContext.Create(_client.GetDatabase("dashboard"));

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

        return Ok(new
        {
            Data = movie,
            Stats = new
            {
                TotalRecords = movie.Sum(x => x.Data.Sum(y => y.Count))
            }
        });
    }
}
