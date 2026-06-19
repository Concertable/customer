using Concertable.Customer.Preference.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Preference.Infrastructure.Repositories;

internal sealed class PreferenceRepository : Repository<PreferenceEntity>, IPreferenceRepository
{
    public PreferenceRepository(PreferenceDbContext context) : base(context) { }

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

    public async Task<IEnumerable<PreferenceEntity>> GetByMatchingGenresAsync(IEnumerable<Genre> genres)
    {
        var target = genres.ToArray();
        return await context.Preferences
            .Where(p => p.GenrePreferences.Any(gp => target.Contains(gp.Genre)))
            .ToListAsync();
    }
}
