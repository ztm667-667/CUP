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

    /// <summary>
    /// Keeps exactly one manager and treats configuration as authoritative for its login name: if the
    /// configured email changed, the existing manager row is renamed rather than duplicated.
    /// </summary>
    public async Task<User> EnsureManagerAsync(string email, string passwordHash, CancellationToken ct = default)
    {
        var byEmail = await db.Users.FirstOrDefaultAsync(x => x.Email == email, ct);
        if (byEmail is not null)
        {
            // Never promote a self-registered account to manager; that would be privilege escalation.
            if (byEmail.Role != Roles.Manager)
            {
                throw new InvalidOperationException(
                    "A non-manager account already uses the configured Admin:Email. Choose another address.");
            }

            return byEmail;
        }

        var manager = await db.Users.FirstOrDefaultAsync(x => x.Role == Roles.Manager, ct);
        if (manager is not null)
        {
            manager.Email = email;
            await db.SaveChangesAsync(ct);
            return manager;
        }

        var created = new User
        {
            Email = email,
            PasswordHash = passwordHash,
            Role = Roles.Manager,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(created);
        await db.SaveChangesAsync(ct);
        return created;
    }
}
