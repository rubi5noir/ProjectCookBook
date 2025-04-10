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
        public double avisnote { get; set; }

    }
}
