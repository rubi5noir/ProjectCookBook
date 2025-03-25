using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ProjectCookBook.Models;
using Dapper;

using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ProjectCookBook.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly string _connexionString;


        /// <summary>
        /// Constructeur de LivresController
        /// </summary>
        /// <param name="configuration">configuration de l'application</param>
        /// <exception cref="Exception"></exception>
        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            // récupération de la chaîne de connexion dans la configuration
            _connexionString = configuration.GetConnectionString("CookBookDataBase")!;

            _logger = logger;

            if (_connexionString == null)
            {
                throw new Exception("Error : Connexion string not found ! ");
            }
        }

        public IActionResult Index()
        {
            string query = "Select * from Recettes " +
                           "LEFT join avis on avis.id_recette = recettes.id " +
                           "order by id asc";
            List<Recette> recettesgrouped;
            List<Recette> recettes;
            try
            {
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    recettes = connexion.Query<Recette, Avis, Recette>(query, (recette, avis) =>
                    {
                        recette.avis.Add(avis);
                        return recette;
                    },
                    splitOn: "id, id_recette").ToList();
                }
            }
            catch (Exception)
            {
                return NotFound();
            }

            recettesgrouped = recettes.GroupBy(R => R.id).Select(g =>
            {
                Recette groupedRecette = g.First();

                groupedRecette.avis = g.SelectMany(R => R.avis).ToList();

                return groupedRecette;
            }).ToList();


            return View(recettesgrouped);
        }

        public IActionResult GetCategories()
        {
            string query = "Select * from Categories";
            List<Categorie> categories;
            try
            {
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    categories = connexion.Query<Categorie>(query).ToList();
                }
            }
            catch (Exception)
            {
                return NotFound();
            }

            return PartialView("_CategorieVignette", categories);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
