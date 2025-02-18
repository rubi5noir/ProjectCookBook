namespace ProjectCookBook.Models
{
    public class Recette
    {
        private int id { get; }
        private string nom { get; set; }
        private string description { get; set; }
        private TimeOnly temps_preparation { get; set; }
        private TimeOnly temps_cuisson { get; set; }
        private int difficulte { get; set; }




    }
}
