using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ProjectCookBook.Models;
using BC = BCrypt.Net.BCrypt;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System;

namespace ProjectCookBook.Controllers
{
    [Authorize]
    public class ComptesController : Controller
    {
        private readonly string _connexionString;


        /// <summary>
        /// Constructeur de ComptesController
        /// </summary>
        /// <param name="configuration">configuration de l'application</param>
        /// <exception cref="Exception"></exception>
        public ComptesController(IConfiguration configuration)
        {
            // récupération de la chaîne de connexion dans la configuration
            _connexionString = configuration.GetConnectionString("CookBookDataBase")!;
            // si la chaîne de connexionn'a pas été trouvé => déclenche une exception => code http 500 retourné
            if (_connexionString == null)
            {
                throw new Exception("Error : Connexion string not found ! ");
            }
        }

        /// <summary>
        /// Retourne la View Compte avec les informations de l'utilisateur
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Compte(int id)
        {
            string query = "Select * from Utilisateurs where id = @id";
            Compte account;
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                account = connexion.QuerySingle<Compte>(query, new { id = id }); ;
            }
            return View(account);
        }

        /// <summary>
        /// Retourne la View Admin si l'utilisateur est un admin
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        public IActionResult Admin()
        {
            return View();
        }

        /// <summary>
        /// Retourne la View Sign_Up_In avec le forulaire d'inscription et de connexion
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View("Sign_Up_In");
        }

        /// <summary>
        /// Inscription de l'utilisateur et redirection vers la page de connexion
        /// </summary>
        /// <param name="utilisateur"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public IActionResult SignUp([FromForm] Compte utilisateur)
        {
            string query = "SELECT * FROM Utilisateurs WHERE email = @email";
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                List<Compte> utilisateursDB = connexion.Query<Compte>(query, new { Email = utilisateur.email }).ToList();

                if (utilisateursDB.Count > 0)
                {
                    ViewData["ValidateMessage"] = "email already used";
                    return View("Sign_Up_In");
                }
                else
                {
                    string insertQuery = "INSERT INTO Utilisateurs (nom, prenom, identifiant, email, password) VALUES (@nom ,@prenom , @identifiant, @email, @password)";

                    string HashedPassword = BC.HashPassword(utilisateur.password);

                    int RowsAffected = connexion.Execute(insertQuery, new { nom = utilisateur.nom, prenom = utilisateur.prenom, identifiant = utilisateur.identifiant, email = utilisateur.email, password = HashedPassword });
                    if (RowsAffected == 1)
                    {
                        TempData["ValidateMessage"] = "Your subscribtion is done. Please log in with your credentials.";
                        return RedirectToAction("Login");

                    }
                    else
                    {
                        ViewData["ValidateMessage"] = "Error during the signin process, please try again.";
                        return View("Sign_Up_In");
                    }
                }
            }
        }

        /// <summary>
        /// Connecte l'utilisateur et le redirige vers la page d'accueil ou la page depuis laquelle il a été redirigé
        /// </summary>
        /// <param name="utilisateur"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SignIn([FromForm] Compte utilisateur)
        {
            ModelState.Remove($"nom");
            ModelState.Remove($"prenom");

            if (!ModelState.IsValid)
            {
                ViewData["ValidateMessage"] = "Email ou mot de passe invalide.";
                return View("Sign_Up_In");
            }
            string query = "SELECT id, identifiant, email, password, admin FROM Utilisateurs WHERE email = @email";
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                Compte utilisateurDB;
                try
                {
                    utilisateurDB = connexion.QuerySingle<Compte>(query, new { Email = utilisateur.email });
                }
                catch (InvalidOperationException)
                {
                    Response.StatusCode = 403;
                    ViewData["ValidateMessage"] = "Wrong email or password.";
                    return View("Sign_Up_In");
                }

                if (BC.Verify(utilisateur.password, utilisateurDB.password))
                {
                    List<Claim> claims = new List<Claim>()
                     {
                         new Claim(ClaimTypes.Email, utilisateur.email),
                         new Claim(ClaimTypes.GivenName, utilisateurDB.identifiant),
                         new Claim(ClaimTypes.NameIdentifier, utilisateurDB.id.ToString()),
                         new Claim(ClaimTypes.Role, utilisateurDB.Role)

                     };

                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    AuthenticationProperties properties = new AuthenticationProperties()
                    {
                        AllowRefresh = true,
                        // IsPersistent = utilisateur.keepLoggedIn,
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);

                    if (Request.Form.ContainsKey("ReturnURL"))
                    {
                        return Redirect(Request.Form["ReturnURL"]!);
                    }
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    Response.StatusCode = 403;
                    ViewData["ValidateMessage"] = "Wrong email or password.";
                    return View("Sign_Up_In");
                }

            }
        }

        /// <summary>
        /// Déconnecte l'utilisateur et le redirige vers la page de connexion
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Comptes");
        }
    }
}

