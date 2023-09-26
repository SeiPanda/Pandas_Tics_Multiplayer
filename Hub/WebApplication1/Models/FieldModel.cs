namespace WebApplication1.Models
{
    public class Field
    { 
        public int Id { get; set; }
        public string Sign { get; set; }
        public bool Clicked => !string.IsNullOrWhiteSpace(Sign);
        public bool Win { get; set; }
    }
}