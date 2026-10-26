using HearthCup.DataAccess.Data;
using HearthCup.Models;
using Microsoft.EntityFrameworkCore;

namespace HearthCup.DataAccess.Repository;

public sealed class UserRepository(ApplicationDbContext db) : IUserRepository
{
    public Task<User?> FindByEmailAsync(string email, CancellationToken ct = default) =>
        db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email, ct);

    public Task<User?> FindByIdAsync(int id, CancellationToken ct = default) =>
        db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<bool> ExistsAsync(string email, CancellationToken ct = default) =>
        db.Users.AnyAsync(x => x.Email == email, ct);

    public async Task<int> AddAsync(User user, CancellationToken ct = default)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        return user.Id;
    }

    /// <summary>Idempotent: creates the configured manager once, then leaves whatever exists alone.</summary>
    public async Task<User> EnsureManagerAsync(string email, string passwordHash, CancellationToken ct = default)
    {
        var existing = await db.Users.FirstOrDefaultAsync(x => x.Email == email, ct);
        if (existing is not null) return existing;

        var manager = new User
        {
            Email = email,
            PasswordHash = passwordHash,
            Role = Roles.Manager,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(manager);
        await db.SaveChangesAsync(ct);
        return manager;
    }
}
