namespace DrimCity.WebApi.Domain;

public class Account
{
    public const int LoginMinLength = 3;
    public const int LoginMaxLength = 32;
    public const int PasswordMinLength = 8;
    public const int PasswordMaxLength = 512;

    public Account(int id, string login, string passwordHash, DateTime createdAt)
    {
        Id = id;
        Login = login;
        PasswordHash = passwordHash;
        CreatedAt = createdAt;
    }

    public int Id { get; private set; }

    public string Login { get; private set; }

    public string PasswordHash { get; private set; }

    public DateTime CreatedAt { get; private set; }
}
