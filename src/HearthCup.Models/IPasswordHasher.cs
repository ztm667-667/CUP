namespace HearthCup.Models;

/// <summary>Password hashing port. The implementation lives in the data tier so the business tier stays framework-free.</summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string passwordHash, string password);
}
