using System.ComponentModel.DataAnnotations;

namespace ProjectCookBook.Models
{
    public class Compte
    {
        public int id { get; set; }
        public string identifiant { get; set; }
        public string nom { get; set; }
        public string prenom { get; set; }

        [DataType(DataType.EmailAddress)]
        public string email { get; set; }

        [DataType(DataType.Password)]
        public string password { get; set; }

        public bool admin { get; set; }
        public string Role => admin ? "Admin" : "User";
    }
}
