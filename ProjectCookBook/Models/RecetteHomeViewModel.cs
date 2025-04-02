namespace ProjectCookBook.Models
{
    public class RecetteHomeViewModel
    {
        public List<Recette> recettes { get; set; } = new List<Recette>();
        public List<Recette> mesrecettes { get; set; } = new List<Recette>();
        public List<Categorie> categories { get; set; } = new List<Categorie>();
    }
}
