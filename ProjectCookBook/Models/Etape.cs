namespace ProjectCookBook.Models
{
    public class Etape
    {
        public int numero { get; set; }
        public string texte { get; set; }

        public Recette recette { get; set; }
    }
}
