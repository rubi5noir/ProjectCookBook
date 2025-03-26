using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProjectCookBook.Models
{
    public class RecetteRechercheViewModel
    {
        public List<Recette> recettes { get; set; } = new List<Recette>();

        public string recherche { get; set; }


        public List<Ingredient> ingredients { get; set; } = new List<Ingredient>();
        public List<int> FilterIngredients { get; set; } = new List<int>();

        public List<Categorie> categories { get; set; } = new List<Categorie>();
        public List<int> FilterCategorie { get; set; } = new List<int>();

        public List<SelectListItem> noteselectlist = new List<SelectListItem> {
            new SelectListItem { Value = "0", Text = "Note"},
            new SelectListItem { Value = "1", Text = "0" },
            new SelectListItem { Value = "2", Text = "1" },
            new SelectListItem { Value = "3", Text = "2" },
            new SelectListItem { Value = "4", Text = "3" },
            new SelectListItem { Value = "5", Text = "4" },
            new SelectListItem { Value = "6", Text = "5" }
            };

        public int note { get; set; }
    }
}
