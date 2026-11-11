namespace RaceDayDB1.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = "";
        public double Distance { get; set; }
        public string EventType { get; set; } = "run";
        public int CategoryId { get; set; }
        public int OrganiserId { get; set; }

        public string? ImageUrl { get; set; } = "";
    }
}