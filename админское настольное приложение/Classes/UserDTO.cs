namespace TestDriveAPI.Classes
{
    public class UserDTO
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Patronymic { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public DateOnly BirthDate { get; set; }

        public int IdRole { get; set; }
    }
}
