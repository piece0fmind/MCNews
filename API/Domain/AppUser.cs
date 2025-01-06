
namespace API.Domain
{
    public class AppUser 
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; } 
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public bool IsActive { get; set; }

    }
}
