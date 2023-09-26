namespace WebApplication1.Models
{
    public class Player
        { 
            public string Id { get; set; }
            public string Name {get; set;}
           public int Wins { get; set; }
           public string Sign { get; set; }
           public bool CurrentTurn { get; set; }
        }
}