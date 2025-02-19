namespace ProjectCookBook.Models
{
    public class Account
    {
        public int id { get; }
        public string identifiant { get; set; }
        public string nom { get; set; }
        public string prenom { get; set; }
        public string email { get; set; }
        public string password { get; set; }
    }
}
