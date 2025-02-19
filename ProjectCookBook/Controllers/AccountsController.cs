using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ProjectCookBook.Models;

namespace ProjectCookBook.Controllers
{
    public class AccountsController : Controller
    {
        private readonly string _connexionString;


        /// <summary>
        /// Constructeur de LivresController
        /// </summary>
        /// <param name="configuration">configuration de l'application</param>
        /// <exception cref="Exception"></exception>
        public AccountsController(IConfiguration configuration)
        {
            // récupération de la chaîne de connexion dans la configuration
            _connexionString = configuration.GetConnectionString("CookBookDataBase")!;
            // si la chaîne de connexionn'a pas été trouvé => déclenche une exception => code http 500 retourné
            if (_connexionString == null)
            {
                throw new Exception("Error : Connexion string not found ! ");
            }
        }

        public IActionResult Account(int id)
        {
            string query = "Select * from Utilisateurs where id = @id";
            Account account;
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                account = connexion.QuerySingle<Account>(query, new { id = id }); ;
            }
            return View(account);
        }
    }
}
