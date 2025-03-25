using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel.DataAnnotations;
using static System.Net.Mime.MediaTypeNames;

namespace ProjectCookBook.Models
{
    public class Recette
    {
        public int id { get; set; }
        public Compte? Createur { get; set; }
        public string? nom { get; set; }
        public string? description { get; set; }
        public TimeSpan temps_preparation { get; set; }
        public TimeSpan temps_cuisson { get; set; }
        public int difficulte { get; set; }


        public Dictionary<Ingredient, string> ingredients { get; set; } = new Dictionary<Ingredient, string>();

        public List<Etape> etapes { get; set; } = new List<Etape>();

        public List<Categorie> categories { get; set; } = new List<Categorie>();

        public Compte? utilisateur { get; set; }

        public List<Avis> avis { get; set; } = new List<Avis>();


        public string? img { get; set; } // pour l'affichage

        public IFormFile imgFile { get; set; } // pour le formulaire

        /* Nouvel avis */
        public string aviscommentaire { get; set; }
        public int avisnote { get; set; }
        public List<SelectListItem> noteselectlist = new List<SelectListItem> {
            new SelectListItem { Value = "0", Text = "0" },
            new SelectListItem { Value = "1", Text = "1" },
            new SelectListItem { Value = "2", Text = "2" },
            new SelectListItem { Value = "3", Text = "3" },
            new SelectListItem { Value = "4", Text = "4" },
            new SelectListItem { Value = "5", Text = "5" }
            };
    }
}
