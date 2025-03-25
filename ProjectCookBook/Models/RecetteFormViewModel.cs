using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ProjectCookBook.Models
{
    public class RecetteFormViewModel
    {
        public Recette? recette { get; set; }

        public int? id { get; set; }

        public int? createur_id { get; set; }

        [Required(ErrorMessage = "Veuillez ajouter un nom a votre recette")]
        public string nom { get; set; }

        [Required(ErrorMessage = "Veuillez ajouter une description a votre recette")]
        public string description { get; set; }

        /* Ingredients */

        public List<IngredientFormViewModel> ingredients { get; set; } = new List<IngredientFormViewModel>();
        public List<SelectListItem> select_ingredients_list { get; set; } = new List<SelectListItem>();

        /* Temps */

        /* Temps Preparation */
        public int temps_preparation_heures { get; set; }
        public int temps_preparation_minutes { get; set; }
        public List<SelectListItem>? select_temps_preparation_heures_list { get; set; }
        public List<SelectListItem>? select_temps_preparation_minutes_list { get; set; }

        /* Temps Cuisson */
        public int temps_cuisson_heures { get; set; }
        public int temps_cuisson_minutes { get; set; }
        public List<SelectListItem>? select_temps_cuisson_heures_list { get; set; }
        public List<SelectListItem>? select_temps_cuisson_minutes_list { get; set; }


        /* Difficulte */
        public int difficulte { get; set; }
        public List<SelectListItem> select_difficulte_list { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "Selectionner une difficulté"},
            new SelectListItem { Value = "1", Text = "Beginer" },
            new SelectListItem { Value = "2", Text = "Average_Cook" },
            new SelectListItem { Value = "3", Text = "Everyday_Cook" },
            new SelectListItem { Value = "4", Text = "Pro_Cook" }
        };

        /* Etape */
        public List<Etape> etapes { get; set; } = new List<Etape>();


        /* Categories */
        public List<CategorieFormViewModel> categories { get; set; } = new List<CategorieFormViewModel>();
        public List<SelectListItem> select_categories_list { get; set; } = new List<SelectListItem>();

        /* Images */
        public string? img { get; set; } // pour l'affichage

        public IFormFile? imgFile { get; set; } // pour le formulaire



        /* Fonctions */
        public void InitialisationSelects()
        {
            InitialisationSelectsTimeCook();
            InitialisationTimeHeating();
        }

        public void InitialisationSelectsTimeCook()
        {
            select_temps_preparation_heures_list = new List<SelectListItem>();
            for (int i = 0; i < 361; i++)
            {
                select_temps_preparation_heures_list.Add(new SelectListItem { Value = $"{i}", Text = $"{i}" });
            }

            select_temps_preparation_minutes_list = new List<SelectListItem>();
            for (int i = 0; i < 60; i++)
            {
                select_temps_preparation_minutes_list.Add(new SelectListItem { Value = $"{i}", Text = $"{i}" });
            }
        }

        public void InitialisationTimeHeating()
        {
            select_temps_cuisson_heures_list = new List<SelectListItem>();
            for (int i = 0; i < 361; i++)
            {
                select_temps_cuisson_heures_list.Add(new SelectListItem { Value = $"{i}", Text = $"{i}" });
            }

            select_temps_cuisson_minutes_list = new List<SelectListItem>();
            for (int i = 0; i < 60; i++)
            {
                select_temps_cuisson_minutes_list.Add(new SelectListItem { Value = $"{i}", Text = $"{i}" });
            }
        }
    }
}
