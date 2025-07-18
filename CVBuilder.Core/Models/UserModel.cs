namespace CVBuilder.Core.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }

        public List<CVModel>? CVs { get; set; }
    }
}
