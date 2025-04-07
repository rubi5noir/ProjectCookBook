using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ProjectCookBook.Models;
using Dapper;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Transactions;
using System.Security.Claims;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Authorization;
using System.Data.Common;
using System.Data;
using NpgsqlTypes;

namespace ProjectCookBook.Controllers
{
    [Authorize]
    public class RecettesController : Controller
    {
        private readonly string _connexionString;


        /// <summary>
        /// Constructeur de RecettesController
        /// </summary>
        /// <param name="configuration">configuration de l'application</param>
        /// <exception cref="Exception"></exception>
        public RecettesController(IConfiguration configuration)
        {
            // récupération de la chaîne de connexion dans la configuration
            _connexionString = configuration.GetConnectionString("CookBookDataBase")!;
            // si la chaîne de connexionn'a pas été trouvé => déclenche une exception => code http 500 retourné
            if (_connexionString == null)
            {
                throw new Exception("Error : Connexion string not found ! ");
            }

            /* Transfert des validate message */
        }

        /// <summary>
        /// Retourne la View Recettes avec la liste de toutes les recettes
        /// </summary>
        /// <returns></returns>
        public IActionResult Recettes()
        {
            string query = "Select * from Recettes " +
                           "left join Avis on Avis.id_recette = Recettes.id";
            List<Recette> recettes;
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                recettes = connexion.Query<Recette, Avis, Recette>(query, (recette, avis) =>
                {
                    recette.avis.Add(avis);
                    return recette;
                },
                splitOn: "id, id_recette").ToList();
            }
            return View(recettes);
        }

        /// <summary>
        /// Retourne la View Detail avec les informations de la recette spécifiée
        /// </summary>
        /// <param name="id">id de recette</param>
        /// <returns></returns>
        public IActionResult Detail(int id)
        {

            string query = "SELECT RECETTES.*, CATEGORIES.*, INGREDIENTS.*, INGREDIENTS_RECETTES.quantite, ETAPES.* " +
                           "FROM RECETTES " +
                           "INNER JOIN CATEGORIES_RECETTES ON RECETTES.ID = CATEGORIES_RECETTES.id_RECETTE " +
                           "INNER JOIN CATEGORIES ON CATEGORIES_RECETTES.id_CATEGORIE = CATEGORIES.ID " +
                           "INNER JOIN INGREDIENTS_RECETTES ON RECETTES.ID = INGREDIENTS_RECETTES.id_RECETTE " +
                           "INNER JOIN INGREDIENTS ON INGREDIENTS_RECETTES.id_INGREDIENT = INGREDIENTS.ID " +
                           "INNER JOIN ETAPES ON RECETTES.ID = ETAPES.id_RECETTE " +
                           "WHERE RECETTES.ID=@id";

            string querycreateur = "Select Utilisateurs.* " +
                                   "From recettes " +
                                   "inner join utilisateurs on recettes.id_utilisateur = utilisateurs.id " +
                                   "where recettes.id=@id";

            string queryavis = "Select avis.*, utilisateurs.* " +
                               "From avis " +
                               "inner join utilisateurs on utilisateurs.id = avis.id_utilisateur " +
                               "where avis.id_recette=@id";

            List<Recette> recettes;
            Recette recette;
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                try
                {
                    recettes = connexion.Query<Recette, Categorie, Ingredient, string, Etape, Recette>(query, (recette, categorie, ingredient, quantite, etape) =>
                    {
                        recette.categories.Add(categorie);

                        if (!recette.ingredients.ContainsKey(ingredient))
                        {
                            recette.ingredients.Add(ingredient, quantite);
                        }

                        recette.etapes.Add(etape);
                        recette.id = id;

                        return recette;
                    },
                    new { id = id },
                    splitOn: "id, id, id, quantite, numero").ToList();
                }
                catch (Exception)
                {
                    return NotFound();
                }
                recette = recettes.GroupBy(R => R.id).Select(g =>
                {
                    Recette groupedRecette = g.First();
                    groupedRecette.categories = g.Select(R => R.categories.Single()).ToList();
                    groupedRecette.etapes = g.Select(R => R.etapes.Single()).ToList();
                    groupedRecette.ingredients = g.Select(R => R.ingredients.Single()).ToDictionary();
                    return groupedRecette;
                }).First();

                recette.ingredients = recette.ingredients.GroupBy(ing => ing.Key.id).Select(g =>
                {
                    KeyValuePair<Ingredient, string> groupedIngredient = g.First();
                    return groupedIngredient;
                }).ToDictionary();

                List<Avis> avis;
                try

                {
                    avis = connexion.Query<Avis, Compte, Avis>(queryavis, (avi, utilisateur) =>
                    {
                        avi.utilisateur = utilisateur;
                        return avi;
                    },
                        new { id = id },
                        splitOn: "id_recette, id").ToList();
                }
                catch (Exception)
                {
                    avis = null;
                }

                recette.etapes = recette.etapes.GroupBy(E => E.numero).Select(g =>
                {
                    Etape groupedEtapes = g.First();
                    return groupedEtapes;
                }).ToList();

                recette.categories = recette.categories.GroupBy(C => C.id).Select(g =>
                {
                    Categorie groupedCategories = g.First();
                    return groupedCategories;
                }).ToList();

                recette.avis = avis;

                recette.Createur = connexion.QuerySingle<Compte>(querycreateur, new { id = id });
                TempData["RecetteId"] = recette.id;
                TempData["CreateurId"] = recette.Createur.id;
            }
            return View(recette);
        }

        /// <summary>
        /// Ajoute un avis a la recette spécifiée
        /// </summary>
        /// <param name="recette"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult AjouterAvis(Recette recette)
        {
            if (TempData["RecetteId"] != null)
            {
                recette.id = Convert.ToInt32(TempData["RecetteId"]);
            }
            if (recette.avisnote != null && recette.aviscommentaire != null)
            {
                string query = "insert into avis (id_recette, id_utilisateur, note, commentaire) values (@id_recette, @id_utilisateur, @note, @commentaire)";

                try
                {
                    using (var connection = new NpgsqlConnection(_connexionString))
                    {
                        List<Object> parameters = new List<Object>();
                        parameters.Add(
                            new
                            {
                                id_recette = TempData["RecetteId"],
                                id_utilisateur = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                                note = recette.avisnote,
                                commentaire = recette.aviscommentaire
                            });
                        connection.Execute(query, parameters);
                    }
                }
                catch (Npgsql.PostgresException e)
                {
                    if (e.ErrorCode == -2147467259)
                    {
                        TempData["ValidateMessage"] = "Vous avez déja entrez un commentaire";
                    }
                }

            }
            return RedirectToAction("Detail", new { id = recette.id });
        }

        /// <summary>
        /// Crée une liste d'ingrédients
        /// </summary>
        /// <returns></returns>
        private List<Ingredient> CreationSelectIngredient()
        {
            string queryIngredients = "Select * from Ingredients order by nom asc";
            List<Ingredient> Ingredients;
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                Ingredients = connexion.Query<Ingredient>(queryIngredients).ToList();
            }
            return Ingredients;
        }

        /// <summary>
        /// Crée une liste de catégories
        /// </summary>
        /// <returns></returns>
        private List<Categorie> CreationSelectCategorie()
        {
            string queryCategories = "Select * from Categories order by nom asc";
            List<Categorie> Categories;
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                Categories = connexion.Query<Categorie>(queryCategories).ToList();
            }
            return Categories;
        }


        /// <summary>
        /// Retourne la View Editeur pour ajouter une recette
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Ajouter()
        {
            RecetteFormViewModel r = new RecetteFormViewModel();
            List<Ingredient> ingredients = CreationSelectIngredient();
            List<Categorie> categories = CreationSelectCategorie();

            r.select_ingredients_list = ingredients.Select(i => new SelectListItem
            {
                Value = i.id.ToString(),
                Text = i.nom
            }).ToList();
            ViewBag.select_ingredients = ingredients;

            r.select_categories_list = categories.Select(i => new SelectListItem
            {
                Value = i.id.ToString(),
                Text = i.nom
            }).ToList();
            ViewBag.select_categories = categories;

            r.InitialisationSelects();
            return View("Editeur", r);
        }

        /// <summary>
        /// Ajoute une nouvelle recette a la base de données
        /// </summary>
        /// <param name="recetteacreer"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Ajouter([FromForm] RecetteFormViewModel recetteacreer)
        {
            if (recetteacreer.imgFile is not null)
            {
                /* Suppression de la verification les variables non essentieles */
                foreach (var etape in recetteacreer.etapes)
                {
                    ModelState.Remove($"etapes[{recetteacreer.etapes.IndexOf(etape)}].recette");
                }

                foreach (var ingredient in recetteacreer.ingredients)
                {
                    ModelState.Remove($"ingredients[{recetteacreer.ingredients.IndexOf(ingredient)}].nom");
                }

                foreach (var categorie in recetteacreer.categories)
                {
                    ModelState.Remove($"categories[{recetteacreer.categories.IndexOf(categorie)}].nom");
                }
            }

            if (ModelState.IsValid)
            {
                Recette recette = new Recette();

                /* Ajout des données a la nouvelle recette */
                recette.nom = recetteacreer.nom;
                recette.description = recetteacreer.description;

                /* Etapes */
                for (int emplacement = 0; emplacement < recetteacreer.etapes.Count(); emplacement++)
                {
                    Etape i = new Etape();
                    i.numero = emplacement;
                    i.texte = recetteacreer.etapes[emplacement].texte;
                    recette.etapes.Add(i);
                }

                /* Ingredients */
                for (int emplacement = 0; emplacement < recetteacreer.ingredients.Count(); emplacement++)
                {
                    Ingredient i = new Ingredient();
                    i.id = recetteacreer.ingredients[emplacement].id;
                    i.nom = recetteacreer.ingredients[emplacement].nom;
                    recette.ingredients.Add(i, recetteacreer.ingredients[emplacement].quantite);
                }

                /* Categories */
                for (int emplacement = 0; emplacement < recetteacreer.categories.Count(); emplacement++)
                {
                    Categorie i = new Categorie();
                    i.id = recetteacreer.categories[emplacement].id;
                    i.nom = recetteacreer.categories[emplacement].nom;
                    recette.categories.Add(i);
                }


                recette.temps_preparation = new TimeSpan(recetteacreer.temps_preparation_heures, recetteacreer.temps_preparation_minutes, 0);
                recette.temps_cuisson = new TimeSpan(recetteacreer.temps_cuisson_heures, recetteacreer.temps_cuisson_minutes, 0);
                recette.difficulte = recetteacreer.difficulte;

                recette.imgFile = recetteacreer.imgFile;

                /* Transaction pour l'insertion */
                using (var connection = new NpgsqlConnection(_connexionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        string? filePath = null;

                        //  gestion de la photo

                        if (recette.imgFile != null && recette.imgFile.Length > 0)
                        {
                            filePath = Path.Combine("/Img/Recettes/",
                                Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(recette.imgFile.FileName)).ToString();

                            using (var stream = System.IO.File.Create("wwwroot" + filePath))
                            {
                                recette.imgFile.CopyTo(stream);
                            }
                            recette.img = filePath;
                        }
                        try
                        {
                            /* Insertion de la recette */
                            string insertRecette = "INSERT INTO recettes (nom, description, temps_preparation, temps_cuisson, difficulte, id_utilisateur, img) VALUES (@nom, @description, @temps_preparation, @temps_cuisson, @difficulte, @id_utilisateur, @img) RETURNING id";

                            var parametersRecette = new { nom = recette.nom, description = recette.description, temps_preparation = recette.temps_preparation, temps_cuisson = recette.temps_cuisson, difficulte = recette.difficulte, id_utilisateur = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)), recette.img };
                            recette.id = connection.ExecuteScalarAsync<int>(insertRecette, parametersRecette, transaction).Result;

                            /* Insertion des etapes */
                            string insertEtapes = "INSERT INTO etapes (numero, id_recette, texte) VALUES (@numero, @id_recette, @texte)";

                            List<object> parametersEtapes = new List<object>();
                            var i = 0;
                            foreach (var etape in recette.etapes)
                            {
                                i++;
                                parametersEtapes.Add(
                                    new
                                    {
                                        numero = i,
                                        id_recette = recette.id,
                                        texte = etape.texte
                                    });
                            }
                            connection.Execute(insertEtapes, parametersEtapes, transaction);

                            /* Insertion des ingredients */
                            string insertIngredients = "INSERT INTO ingredients_recettes (id_ingredient, id_recette, quantite) VALUES (@id_ingredient, @id_recette, @quantite)";

                            List<object> parametersIngredients = new List<object>();
                            foreach (var ingredient in recette.ingredients)
                            {
                                parametersIngredients.Add(
                                    new
                                    {
                                        id_ingredient = ingredient.Key.id,
                                        id_recette = recette.id,
                                        quantite = ingredient.Value
                                    });
                            }
                            connection.Execute(insertIngredients, parametersIngredients, transaction);

                            /* insertion des categories */
                            string insertCategories = "INSERT INTO categories_recettes (id_categorie, id_recette) VALUES (@id_categorie, @id_recette)";

                            List<object> parametersCategories = new List<object>();
                            foreach (var categorie in recette.categories)
                            {
                                parametersCategories.Add(
                                    new
                                    {
                                        id_categorie = categorie.id,
                                        id_recette = recette.id,
                                    });
                            }
                            connection.Execute(insertCategories, parametersCategories, transaction);

                            transaction.Commit();
                            return RedirectToAction("Detail", new { id = recette.id });
                        }
                        catch (Exception ex)
                        {
                            // Annuler la transaction en cas d'erreur
                            transaction.Rollback();
                            Console.WriteLine($"Erreur : {ex.Message}");

                            if (filePath != null)
                            {
                                System.IO.File.Delete("wwwroot" + filePath);
                            }

                        }
                    }
                }


            }

            var errors = ModelState.Where(x => x.Value.Errors.Count > 0).ToDictionary
                (
                    k => k.Key,
                    v => v.Value.Errors.Select(e => e.ErrorMessage).ToList()
                );


            var ingredientsKeys = ModelState.Keys.Where(key => key.Contains("ingredients")).ToList();
            if (ingredientsKeys.Any())
            {
                TempData["ingredient"] = "Veuillez vérifier les informations des ingrédients";
            }

            var etapesKeys = ModelState.Keys.Where(key => key.Contains("etapes")).ToList();
            if (etapesKeys.Any())
            {
                TempData["etape"] = "Veuillez vérifier les informations des etapes";
            }

            var categoriesKeys = ModelState.Keys.Where(key => key.Contains("categories")).ToList();
            if (categoriesKeys.Any())
            {
                TempData["categorie"] = "Veuillez vérifier les informations des categories";
            }


            List<Ingredient> ingredients = CreationSelectIngredient();
            List<Categorie> categories = CreationSelectCategorie();

            recetteacreer.select_ingredients_list = ingredients.Select(i => new SelectListItem
            {
                Value = i.id.ToString(),
                Text = i.nom
            }).ToList();
            ViewBag.select_ingredients = ingredients;

            recetteacreer.select_categories_list = categories.Select(i => new SelectListItem
            {
                Value = i.id.ToString(),
                Text = i.nom
            }).ToList();
            ViewBag.select_categories = categories;

            recetteacreer.InitialisationSelects();

            return View("Editeur", recetteacreer);
        }



        /// <summary>
        /// Retourne la View Editeur pour modifier une recette
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Editer(int id)
        {
            RecetteFormViewModel recetteFormViewModel = new RecetteFormViewModel();

            /* Selects */
            List<Ingredient> ingredients = CreationSelectIngredient();
            List<Categorie> categories = CreationSelectCategorie();

            recetteFormViewModel.select_ingredients_list = ingredients.Select(i => new SelectListItem
            {
                Value = i.id.ToString(),
                Text = i.nom
            }).ToList();

            recetteFormViewModel.select_categories_list = categories.Select(i => new SelectListItem
            {
                Value = i.id.ToString(),
                Text = i.nom
            }).ToList();

            string query = "SELECT RECETTES.*, CATEGORIES.*, INGREDIENTS.*, INGREDIENTS_RECETTES.quantite, ETAPES.* " +
                           "FROM RECETTES " +
                           "INNER JOIN CATEGORIES_RECETTES ON RECETTES.ID = CATEGORIES_RECETTES.id_RECETTE " +
                           "INNER JOIN CATEGORIES ON CATEGORIES_RECETTES.id_CATEGORIE = CATEGORIES.ID " +
                           "INNER JOIN INGREDIENTS_RECETTES ON RECETTES.ID = INGREDIENTS_RECETTES.id_RECETTE " +
                           "INNER JOIN INGREDIENTS ON INGREDIENTS_RECETTES.id_INGREDIENT = INGREDIENTS.ID " +
                           "INNER JOIN ETAPES ON RECETTES.ID = ETAPES.id_RECETTE " +
                           "WHERE RECETTES.ID=@id";

            List<Recette> recettes;
            Recette recette;
            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                try
                {
                    recettes = connexion.Query<Recette, Categorie, Ingredient, string, Etape, Recette>(query, (recette, categorie, ingredient, quantite, etape) =>
                    {
                        recette.categories.Add(categorie);

                        if (!recette.ingredients.ContainsKey(ingredient))
                        {
                            recette.ingredients.Add(ingredient, quantite);
                        }

                        recette.etapes.Add(etape);
                        recette.id = id;

                        return recette;
                    },
                    new { id = id },
                    splitOn: "id, id, id, quantite, numero").ToList();
                }
                catch (Exception)
                {
                    return NotFound();
                }

                recette = recettes.GroupBy(R => R.id).Select(g =>
                {
                    Recette groupedRecette = g.First();
                    groupedRecette.categories = g.Select(R => R.categories.Single()).ToList();
                    groupedRecette.etapes = g.Select(R => R.etapes.Single()).ToList();
                    groupedRecette.ingredients = g.Select(R => R.ingredients.Single()).ToDictionary();
                    return groupedRecette;
                }).First();

                recette.ingredients = recette.ingredients.GroupBy(ing => ing.Key.id).Select(g =>
                {
                    KeyValuePair<Ingredient, string> groupedIngredient = g.First();
                    return groupedIngredient;
                }).ToDictionary();

                recette.etapes = recette.etapes.GroupBy(E => E.numero).Select(g =>
                {
                    Etape groupedEtapes = g.First();
                    return groupedEtapes;
                }).ToList();

                recette.categories = recette.categories.GroupBy(C => C.id).Select(g =>
                {
                    Categorie groupedCategories = g.First();
                    return groupedCategories;
                }).ToList();

                recette.id = id;
                recetteFormViewModel.recette = recette;
                recetteFormViewModel.difficulte = recette.difficulte;
                recetteFormViewModel.description = recette.description;
                recetteFormViewModel.nom = recette.nom;
                recetteFormViewModel.img = recette.img;

                recetteFormViewModel.categories = recette.categories.Select(i => new CategorieFormViewModel
                {
                    id = i.id,
                    nom = i.nom
                }).ToList();

                recetteFormViewModel.ingredients = recette.ingredients.Select(i => new IngredientFormViewModel
                {
                    id = i.Key.id,
                    nom = i.Key.nom,
                    quantite = i.Value
                }).ToList();

                recetteFormViewModel.etapes = recette.etapes;

                recetteFormViewModel.temps_preparation_heures = recette.temps_preparation.Hours;
                recetteFormViewModel.temps_preparation_minutes = recette.temps_preparation.Minutes;
                recetteFormViewModel.temps_cuisson_heures = recette.temps_cuisson.Hours;
                recetteFormViewModel.temps_cuisson_minutes = recette.temps_cuisson.Minutes;

                ViewBag.select_ingredients = recetteFormViewModel.select_ingredients_list;
                ViewBag.select_categories = recetteFormViewModel.select_categories_list;

                string querycreateur = "Select Utilisateurs.* " +
                                   "From recettes " +
                                   "inner join utilisateurs on recettes.id_utilisateur = utilisateurs.id " +
                                   "where recettes.id=@id";

                recette.Createur = connexion.QuerySingle<Compte>(querycreateur, new { id = id });
                recetteFormViewModel.createur_id = recette.Createur.id;

                recetteFormViewModel.InitialisationSelects();

                if (User.FindFirstValue(ClaimTypes.Role) == "Admin" || recetteFormViewModel.createur_id == int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
                {
                    return View("Editeur", recetteFormViewModel);
                }

            }

            return Forbid();

        }

        /// <summary>
        /// Modifie une recette dans la base de données
        /// </summary>
        /// <param name="recetteamodifier"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Editer(RecetteFormViewModel recetteamodifier)
        {
            if (User.FindFirstValue(ClaimTypes.Role) != "Admin" && recetteamodifier.createur_id != int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                return Forbid();
            }
            recetteamodifier.recette = new Recette();

            /* Suppression de la verification les variables non essentieles */
            foreach (var etape in recetteamodifier.etapes)
            {
                ModelState.Remove($"etapes[{recetteamodifier.etapes.IndexOf(etape)}].recette");
            }

            foreach (var ingredient in recetteamodifier.ingredients)
            {
                ModelState.Remove($"ingredients[{recetteamodifier.ingredients.IndexOf(ingredient)}].nom");
            }

            foreach (var categorie in recetteamodifier.categories)
            {
                ModelState.Remove($"categories[{recetteamodifier.categories.IndexOf(categorie)}].nom");
            }

            if (ModelState.IsValid)
            {
                Recette recette = new Recette();

                /* Ajout des données a la nouvelle recette */
                recette.id = (int)recetteamodifier.id;
                recette.nom = recetteamodifier.nom;
                recette.description = recetteamodifier.description;
                recette.img = recetteamodifier.img;
                recette.Createur = new Compte();
                recette.Createur.id = (int)recetteamodifier.createur_id;

                /* Etapes */
                for (int emplacement = 0; emplacement < recetteamodifier.etapes.Count(); emplacement++)
                {
                    Etape i = new Etape();
                    i.numero = emplacement;
                    i.texte = recetteamodifier.etapes[emplacement].texte;
                    recette.etapes.Add(i);
                }

                /* Ingredients */
                for (int emplacement = 0; emplacement < recetteamodifier.ingredients.Count(); emplacement++)
                {
                    Ingredient i = new Ingredient();
                    i.id = recetteamodifier.ingredients[emplacement].id;
                    i.nom = recetteamodifier.ingredients[emplacement].nom;
                    recette.ingredients.Add(i, recetteamodifier.ingredients[emplacement].quantite);
                }

                /* Categories */
                for (int emplacement = 0; emplacement < recetteamodifier.categories.Count(); emplacement++)
                {
                    Categorie i = new Categorie();
                    i.id = recetteamodifier.categories[emplacement].id;
                    i.nom = recetteamodifier.categories[emplacement].nom;
                    recette.categories.Add(i);
                }


                recette.temps_preparation = new TimeSpan(recetteamodifier.temps_preparation_heures, recetteamodifier.temps_preparation_minutes, 0);
                recette.temps_cuisson = new TimeSpan(recetteamodifier.temps_cuisson_heures, recetteamodifier.temps_cuisson_minutes, 0);
                recette.difficulte = recetteamodifier.difficulte;
                recette.img = recetteamodifier.img;

                if (recetteamodifier.imgFile != null)
                {
                    recette.imgFile = recetteamodifier.imgFile;
                }

                /* Transaction pour l'update */
                using (var connection = new NpgsqlConnection(_connexionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        string? filePath = null;

                        //  gestion de la photo

                        if (recette.imgFile != null && recette.imgFile.Length > 0)
                        {
                            filePath = Path.Combine("/Img/recettes/",
                                Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(recette.imgFile.FileName)).ToString();

                            using (var stream = System.IO.File.Create("wwwroot" + filePath))
                            {
                                recette.imgFile.CopyTo(stream);
                            }

                            recette.img = filePath;
                        }
                        try
                        {
                            /* Update de la recette */
                            string updateRecette = "UPDATE recettes " +
                            "set nom = @nom, " +
                            "description = @description, " +
                            "temps_preparation = @temps_preparation, " +
                            "temps_cuisson = @temps_cuisson, " +
                            "difficulte = @difficulte, " +
                            "id_utilisateur = @id_utilisateur, " +
                            "img = @img " +
                            "where id = @id";

                            var parametersRecette = new { nom = recette.nom, description = recette.description, temps_preparation = recette.temps_preparation, temps_cuisson = recette.temps_cuisson, difficulte = recette.difficulte, id_utilisateur = recette.Createur.id, recette.img, id = recette.id };
                            connection.Execute(updateRecette, parametersRecette, transaction);

                            // Supprimer les anciennes étapes associées à la recette
                            string deleteEtapes = "DELETE FROM etapes " +
                                "WHERE id_recette = @id_recette";
                            connection.Execute(deleteEtapes, new { id_recette = recette.id }, transaction);

                            // Réinsérer les étapes mises à jour
                            string insertEtapes = "INSERT INTO etapes (numero, id_recette, texte) " +
                                                  "VALUES (@numero, @id_recette, @texte)";

                            var i = 0;
                            List<object> parametersEtapes = new List<object>();
                            foreach (var etape in recette.etapes)
                            {
                                i++;
                                parametersEtapes.Add(new
                                {
                                    numero = i,
                                    id_recette = recette.id,
                                    texte = etape.texte
                                });
                            }

                            connection.Execute(insertEtapes, parametersEtapes, transaction);


                            // Supprimer tous les ingrédients liés à la recette
                            string deleteIngredients = "DELETE FROM ingredients_recettes " +
                                "WHERE id_recette = @id_recette";
                            connection.Execute(deleteIngredients, new { id_recette = recette.id }, transaction);

                            // Réinsérer les nouveaux ingrédients
                            string insertIngredients = "INSERT INTO ingredients_recettes (id_ingredient, id_recette, quantite) " +
                                                       "VALUES (@id_ingredient, @id_recette, @quantite)";

                            List<object> parametersIngredients = new List<object>();
                            foreach (var ingredient in recette.ingredients)
                            {
                                parametersIngredients.Add(new
                                {
                                    id_ingredient = ingredient.Key.id,
                                    id_recette = recette.id,
                                    quantite = ingredient.Value
                                });
                            }
                            connection.Execute(insertIngredients, parametersIngredients, transaction);


                            // Suppression des anciennes catégories
                            string deleteCategories = "DELETE FROM categories_recettes " +
                                "WHERE id_recette = @id_recette";
                            connection.Execute(deleteCategories, new { id_recette = recette.id }, transaction);

                            // Insertion des nouvelles catégories
                            string insertCategories = "INSERT INTO categories_recettes (id_categorie, id_recette) " +
                                "VALUES (@id_categorie, @id_recette)";

                            List<object> parametersCategories = new List<object>();
                            foreach (var categorie in recette.categories)
                            {
                                parametersCategories.Add(new
                                {
                                    id_categorie = categorie.id,
                                    id_recette = recette.id
                                });
                            }
                            connection.Execute(insertCategories, parametersCategories, transaction);

                            if (recette.img != recetteamodifier.img)
                            {
                                // Récupérer le chemin physique du fichier
                                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", recetteamodifier.img);

                                System.IO.File.Delete("wwwroot" + path); // Supprimer le fichier
                            }

                            transaction.Commit();

                        }
                        catch (Exception ex)
                        {
                            // Annuler la transaction en cas d'erreur
                            transaction.Rollback();
                            Console.WriteLine($"Erreur : {ex.Message}");

                            if (filePath != null)
                            {
                                System.IO.File.Delete("wwwroot" + filePath);
                            }
                        }
                    }

                }

                return RedirectToAction("Detail", new { id = recette.id });
            }
            else
            {
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0).ToDictionary
                    (
                        k => k.Key,
                        v => v.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    );


                var ingredientsKeys = ModelState.Keys.Where(key => key.Contains("ingredients")).ToList();
                if (ingredientsKeys.Any())
                {
                    TempData["ingredient"] = "Veuillez vérifier les informations des ingrédients";
                }

                var etapesKeys = ModelState.Keys.Where(key => key.Contains("etapes")).ToList();
                if (etapesKeys.Any())
                {
                    TempData["etape"] = "Veuillez vérifier les informations des etapes";
                }

                var categoriesKeys = ModelState.Keys.Where(key => key.Contains("categories")).ToList();
                if (categoriesKeys.Any())
                {
                    TempData["categorie"] = "Veuillez vérifier les informations des categories";
                }


                List<Ingredient> ingredients = CreationSelectIngredient();
                List<Categorie> categories = CreationSelectCategorie();

                recetteamodifier.select_ingredients_list = ingredients.Select(i => new SelectListItem
                {
                    Value = i.id.ToString(),
                    Text = i.nom
                }).ToList();
                ViewBag.select_ingredients = ingredients;

                recetteamodifier.select_categories_list = categories.Select(i => new SelectListItem
                {
                    Value = i.id.ToString(),
                    Text = i.nom
                }).ToList();
                ViewBag.select_categories = categories;

                recetteamodifier.InitialisationSelects();

                return View("Editeur", recetteamodifier);
            }
        }

        public IActionResult PageSearch()
        {
            RecetteRechercheViewModel recetteRechercheViewModel = new RecetteRechercheViewModel();
            recetteRechercheViewModel.ingredients = CreationSelectIngredient();
            recetteRechercheViewModel.categories = CreationSelectCategorie();
            
            return View("Search", recetteRechercheViewModel);
        }

        /// <summary>
        /// Passerelle depuis les vignettes categorie et la page de recherche
        /// </summary>
        /// <param name="Search_Recipe"></param>
        /// <returns></returns>
        public IActionResult SearchOnClick(string Search_Recipe = "")
        {
            return Search(Search_Recipe); // Réutilise la logique du POST
        }

        /// <summary>
        /// Retourne la View Search selon la recherche spécifiée
        /// </summary>
        /// <param name="Search_Recipe"></param>
        /// <returns></returns>
        public IActionResult Search(string Search_Recipe)
        {
            string queryrechercheutilisateur = "SELECT DISTINCT recettes.id " +
                "FROM RECETTES " +
                "INNER JOIN UTILISATEURS ON UTILISATEURS.ID = ID_UTILISATEUR " +
                "INNER JOIN CATEGORIES_RECETTES ON CATEGORIES_RECETTES.ID_RECETTE = recettes.ID " +
                "INNER JOIN CATEGORIES ON categories.id = CATEGORIES_RECETTES.ID_CATEGORIE " +
                "INNER JOIN INGREDIENTS_RECETTES ON INGREDIENTS_RECETTES.ID_RECETTE = recettes.ID " +
                "INNER JOIN INGREDIENTS ON INGREDIENTS.ID = ID_INGREDIENT " +
                "WHERE RECETTES.NOM LIKE @recherche " +
                "OR UTILISATEURS.IDENTIFIANT LIKE @recherche " +
                "OR CATEGORIES.NOM LIKE @recherche " +
                "OR INGREDIENTS.NOM LIKE @recherche";

            string queryrecettes = "Select * from Recettes " +
                           "LEFT join avis on avis.id_recette = recettes.id " +
                           "where recettes.id = ANY(@ids) " +
                           "order by id asc";

            List<int> recettes_ids = new List<int>();
            List<Recette> recettesgrouped;
            List<Recette> recettes;

            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                try
                {
                    recettes_ids = connexion.Query<int>(queryrechercheutilisateur, new { recherche = "%" + Search_Recipe + "%" }).ToList();

                    List<object> parametersRecettes = new List<object>();
                    foreach (var id in recettes_ids)
                    {
                        parametersRecettes.Add(
                            new
                            {
                                id = id
                            });
                    }

                    recettes = connexion.Query<Recette, Avis, Recette>(queryrecettes, (recette, avis) =>
                    {
                        recette.avis.Add(avis);
                        return recette;
                    },
                    new { ids = recettes_ids.ToArray() },
                    splitOn: "id, id_recette").ToList();
                }
                catch (Exception)
                {
                    return NotFound();
                }
            }

            recettesgrouped = recettes.GroupBy(R => R.id).Select(g =>
                        {
                            Recette groupedRecette = g.First();

                            groupedRecette.avis = g.SelectMany(R => R.avis).ToList();

                            return groupedRecette;
                        }).ToList();

            RecetteRechercheViewModel recetteRechercheViewModel = new RecetteRechercheViewModel();
            recetteRechercheViewModel.ingredients = CreationSelectIngredient();
            recetteRechercheViewModel.categories = CreationSelectCategorie();
            recetteRechercheViewModel.recettes = recettesgrouped;
            recetteRechercheViewModel.recherche = Search_Recipe;

            return Json(recetteRechercheViewModel);
        }

        /// <summary>
        /// Retourne la View Search selon les filtres spécifiés
        /// </summary>
        /// <param name="recetteRechercheViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult RechercheFiltrer(RecetteRechercheViewModel recetteRechercheViewModel)
        {
            string queryrechercheutilisateur = "SELECT recettes.id " +
                "FROM RECETTES " +
                "INNER JOIN UTILISATEURS ON UTILISATEURS.ID = ID_UTILISATEUR " +
                "INNER JOIN CATEGORIES_RECETTES ON CATEGORIES_RECETTES.ID_RECETTE = recettes.ID " +
                "INNER JOIN CATEGORIES ON categories.id = CATEGORIES_RECETTES.ID_CATEGORIE " +
                "INNER JOIN INGREDIENTS_RECETTES ON INGREDIENTS_RECETTES.ID_RECETTE = recettes.ID " +
                "INNER JOIN INGREDIENTS ON INGREDIENTS.ID = ID_INGREDIENT " +
                "WHERE (RECETTES.NOM LIKE @recherche " +
                "OR UTILISATEURS.IDENTIFIANT LIKE @recherche " +
                "OR CATEGORIES.NOM LIKE @recherche " +
                "OR INGREDIENTS.NOM LIKE @recherche)";

            if (recetteRechercheViewModel.FilterIngredients.Any())
            {
                queryrechercheutilisateur += " AND ingredients.id = ANY(@ingredients)";
            }

            if (recetteRechercheViewModel.FilterCategorie.Any())
            {
                queryrechercheutilisateur += " AND categories.id = ANY(@categories)";
            }

            if (recetteRechercheViewModel.FilterIngredients.Any() || recetteRechercheViewModel.FilterCategorie.Any())
            {
                queryrechercheutilisateur += " GROUP BY recettes.id";

                if (recetteRechercheViewModel.FilterIngredients.Any() && recetteRechercheViewModel.FilterCategorie.Any())
                {
                    queryrechercheutilisateur += " Having count(distinct ingredients.id) = @ingredientCount AND count(distinct categories.id) = @categorieCount";
                }

                else if (recetteRechercheViewModel.FilterIngredients.Any())
                {
                    queryrechercheutilisateur += " Having count(distinct ingredients.id) = @ingredientCount";
                }

                else if (recetteRechercheViewModel.FilterCategorie.Any())
                {
                    queryrechercheutilisateur += " Having count(distinct categories.id) = @categorieCount";
                }
            }

            string queryrecettes = "Select * from Recettes " +
                       "LEFT join avis on avis.id_recette = recettes.id " +
                       "where recettes.id = ANY(@ids) " +
                       "order by id asc";

            List<int> recettes_ids = new List<int>();
            List<Recette> recettesgrouped;
            List<Recette> recettes;

            using (var connexion = new NpgsqlConnection(_connexionString))
            {
                try
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("recherche", "%" + recetteRechercheViewModel.recherche + "%");
                    if (recetteRechercheViewModel.FilterIngredients.Any())
                    {
                        parameters.Add("ingredients", recetteRechercheViewModel.FilterIngredients.ToArray());
                        parameters.Add("ingredientCount", recetteRechercheViewModel.FilterIngredients.Count);
                    }

                    if (recetteRechercheViewModel.FilterCategorie.Any())
                    {
                        parameters.Add("categories", recetteRechercheViewModel.FilterCategorie.ToArray());
                        parameters.Add("categorieCount", recetteRechercheViewModel.FilterCategorie.Count);
                    }

                    recettes_ids = connexion.Query<int>(queryrechercheutilisateur, parameters).ToList();

                    List<object> parametersRecettes = new List<object>();
                    foreach (var id in recettes_ids)
                    {
                        parametersRecettes.Add(
                            new
                            {
                                id = id
                            });
                    }

                    recettes = connexion.Query<Recette, Avis, Recette>(queryrecettes, (recette, avis) =>
                    {
                        recette.avis.Add(avis);
                        return recette;
                    },
                    new { ids = recettes_ids.ToArray() },
                    splitOn: "id, id_recette").ToList();
                }
                catch (Exception)
                {
                    return NotFound();
                }
            }

            recettesgrouped = recettes.GroupBy(R => R.id).Select(g =>
            {
                Recette groupedRecette = g.First();

                groupedRecette.avis = g.SelectMany(R => R.avis).ToList();

                return groupedRecette;
            }).ToList();

            recetteRechercheViewModel.ingredients = CreationSelectIngredient();
            recetteRechercheViewModel.categories = CreationSelectCategorie();
            recetteRechercheViewModel.recettes = recettesgrouped;

            return View("Search", recetteRechercheViewModel);
        }

        /// <summary>
        /// Retourne la View MesRecettes avec les recettes de l'utilisateur connecté
        /// </summary>
        /// <returns></returns>
        public IActionResult MesRecettes()
        {
            string query = "Select * from Recettes " +
                           "left join avis on avis.id_recette = recettes.id " +
                           "Where recettes.id_utilisateur = @createur " +
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
                    new { createur = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)) },
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

        /// <summary>
        /// Retourne le formulaire d'ajout d'ingrédient
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult AjoutIngredient()
        {
            Ingredient ingredient = new Ingredient();
            return View(ingredient);
        }

        /// <summary>
        /// Ajoute un ingrédient à la base de données
        /// </summary>
        /// <param name="ingredient"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult AjoutIngredient([FromForm] Ingredient ingredient)
        {
            string query = "INSERT INTO Ingredients (nom) VALUES (@nom)";
            try
            {
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    int RowsAffected = connexion.Execute(query, new { nom = ingredient.nom });
                    if (RowsAffected == 1)
                    {
                        TempData["ValidateMessage"] = "Ingrédient ajouter";
                        return RedirectToAction("AjoutIngredient");
                    }
                    else
                    {
                        TempData["ValidateMessage"] = "Erreur lors de l'ajout de l'ingrédient.";
                        return View(ingredient);
                    }
                }
            }
            catch (NpgsqlException e)
            {
                if (e.HResult == -2147467259)
                {
                    TempData["ValidateMessage"] = "Ingrédient déja existant dans la base de donnée";
                }
            }
            return View(ingredient);
        }

        /// <summary>
        /// Retourne le formulaire d'ajout de catégorie
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult AjoutCategorie()
        {
            Categorie categorie = new Categorie();
            return View(categorie);
        }

        /// <summary>
        /// Ajoute une catégorie à la base de données
        /// </summary>
        /// <param name="categorie"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult AjoutCategorie([FromForm] Categorie categorie)
        {
            string query = "INSERT INTO Categories (nom) VALUES (@nom)";

            try
            {
                using (var connexion = new NpgsqlConnection(_connexionString))
                {
                    int RowsAffected = connexion.Execute(query, new { nom = categorie.nom });
                    if (RowsAffected == 1)
                    {
                        TempData["ValidateMessage"] = "Catégorie ajouter";
                        return RedirectToAction("AjoutCategorie");
                    }
                    else
                    {
                        ViewData["ValidateMessage"] = "Erreur lors de l'ajout de la catégorie.";
                        return View(categorie);
                    }
                }
            }

            catch (NpgsqlException e)
            {
                if (e.HResult == -2147467259)
                {
                    TempData["ValidateMessage"] = "Catégorie déja existante dans la base de donnée";
                }
                return View(categorie);
            }
        }
    }
}

