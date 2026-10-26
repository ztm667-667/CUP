namespace HearthCup.Models;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email, CancellationToken ct = default);

    Task<User?> FindByIdAsync(int id, CancellationToken ct = default);

    Task<bool> ExistsAsync(string email, CancellationToken ct = default);

    Task<int> AddAsync(User user, CancellationToken ct = default);

    Task<User> EnsureManagerAsync(string email, string passwordHash, CancellationToken ct = default);
}
