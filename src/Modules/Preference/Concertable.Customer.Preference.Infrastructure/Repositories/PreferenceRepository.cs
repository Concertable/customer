using Concertable.Customer.Preference.Infrastructure.Data;
using Concertable.DataAccess.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Preference.Infrastructure.Repositories;

internal sealed class PreferenceRepository : Repository<PreferenceEntity>, IPreferenceRepository
{
    private readonly PreferenceDbContext context;

    public PreferenceRepository(PreferenceDbContext context) : base(context)
    {
        this.context = context;
    }

    public async Task<bool> InsertAsync(PreferenceEntity preference)
    {
        context.Preferences.Add(preference);

        try
        {
            await context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex) when (ex.IsDuplicateKey())
        {
            ex.DiscardFailedChanges();
            return false;
        }
    }

    public override async Task<IEnumerable<PreferenceEntity>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Preferences
            .Include(p => p.GenrePreferences)
            .ToListAsync(ct);
    }

    public override async Task<PreferenceEntity?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await context.Preferences
            .Include(p => p.GenrePreferences)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<PreferenceEntity?> GetByUserIdAsync(Guid id)
    {
        return await context.Preferences
            .Include(p => p.GenrePreferences)
            .FirstOrDefaultAsync(p => p.UserId == id);
    }

    public async Task<IReadOnlyList<PreferenceEntity>> GetByMatchingGenresAsync(IEnumerable<Genre> genres)
    {
        var target = genres.ToArray();
        return await context.Preferences
            .Where(p => p.GenrePreferences.Any(gp => target.Contains(gp.Genre)))
            .ToListAsync();
    }
}
