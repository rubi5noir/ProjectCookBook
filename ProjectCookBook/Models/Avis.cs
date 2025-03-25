namespace ProjectCookBook.Models
{
    public class Avis
    {
        public int note { get; set; }
        public string commentaire { get; set; }

        public Recette recette { get; set; }
        public Compte utilisateur { get; set; }
    }
}
