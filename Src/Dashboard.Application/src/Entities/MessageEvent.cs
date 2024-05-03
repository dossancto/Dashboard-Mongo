using MongoDB.Bson;

namespace Dashboard.Application.Entities;

public class MessageEvent
{
    public ObjectId _id { get; set; }

    public EventType EventType { get; set; }

    public long Cost { get; set; }

    public DateTime GeneratedAt { get; set; }
}

public enum EventType
{
    PERSON_CREATED,
    PERSON_DELETED,
    PERSON_UPDATED,
}
