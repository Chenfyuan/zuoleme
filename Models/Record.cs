namespace zuoleme.Models
{
    public class Record
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Note { get; set; }

        public Record()
        {
            Id = Guid.NewGuid();
            Timestamp = DateTime.Now;
        }
    }
}
