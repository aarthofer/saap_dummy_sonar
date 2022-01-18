namespace Apollo.Core.Domain
{
    [TableName("user")]
    public class User
    {
        public enum UserRole { USER, MANAGER }

        [Column("id", true), PK]
        public int Id { get; set; }

        [Column("username")]
        public string Username { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("role")]
        public UserRole Role { get; set; }

    }
}
