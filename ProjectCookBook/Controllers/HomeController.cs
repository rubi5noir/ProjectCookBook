using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ProjectCookBook.Models;
using Dapper;

namespace ProjectCookBook.Controllers
{
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
            string query = "Select * from Recettes";
            List<Recette> recettes;
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                recettes = connexion.Query<Recette>(query).ToList();
            }
            return View(recettes);
        }

        public IActionResult SignUpSignIn()
        {
            return View();
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
