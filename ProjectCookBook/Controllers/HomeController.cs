using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ProjectCookBook.Models;
using Dapper;

using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace ProjectCookBook.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly string _connexionString;


        /// <summary>
        /// Constructeur de HomeController
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
            RecetteHomeViewModel recetteHomeViewModel = new RecetteHomeViewModel();

            string queryrecettes = "Select * from Recettes " +
                           "LEFT join avis on avis.id_recette = recettes.id " +
                           "order by id asc";

            string querymesrecettes = "Select * from Recettes " +
                           "left join avis on avis.id_recette = recettes.id " +
                           "Where recettes.id_utilisateur = @createur " +
                           "order by id asc " +
                           "limit 25";

            List<Recette> mesrecettesgrouped;
            List<Recette> mesrecettes;
            List<Categorie> categories = GetCategories();
            try
            {
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    mesrecettes = connexion.Query<Recette, Avis, Recette>(querymesrecettes, (recette, avis) =>
                    {
                        recette.avis.Add(avis);
                        return recette;
                    },
                    new { createur = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)) },
                    splitOn: "id, id_recette").ToList();
                }
            }
            catch (Exception)
            {
                return NotFound();
            }

            mesrecettesgrouped = mesrecettes.GroupBy(R => R.id).Select(g =>
            {
                Recette groupedRecette = g.First();

                groupedRecette.avis = g.SelectMany(R => R.avis).ToList();

                return groupedRecette;
            }).ToList();

            recetteHomeViewModel.mesrecettes = mesrecettesgrouped;

            List<Recette> recettesgrouped;
            List<Recette> recettes;
            try
            {
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    recettes = connexion.Query<Recette, Avis, Recette>(queryrecettes, (recette, avis) =>
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

            recetteHomeViewModel.recettes = recettesgrouped;
            recetteHomeViewModel.categories = categories;

            return View(recetteHomeViewModel);
        }

        public List<Categorie> GetCategories()
        {
            string query = "Select * from Categories " +
                "limit 15";
            List<Categorie> categories;

                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    categories = connexion.Query<Categorie>(query).ToList();
                }

            return categories;
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

        [Route("/Home/ErrorHandler/{statusCode}")]
        public IActionResult ErrorHandler(int statusCode)
        {
            return statusCode switch
            {
                403 => View("AccessDenied"),
                404 => View("NotFound"),
                _ => View("AutresErreurs")
            };
        }
    }
}
