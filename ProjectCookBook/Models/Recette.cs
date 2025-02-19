namespace ProjectCookBook.Models
{
    public class Recette
    {
        public int id { get; }
        public string nom { get; set; }
        public string description { get; set; }
        public TimeSpan temps_preparation { get; set; }
        public TimeSpan temps_cuisson { get; set; }
        public int difficulte { get; set; }
    }
}
