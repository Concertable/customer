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

    public Task<bool> InsertAsync(PreferenceEntity preference) => this.TryInsertAsync(preference);

    public override async Task<IReadOnlyList<PreferenceEntity>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Preferences
            .Include(p => p.GenrePreferences)
            .ToListAsync(ct);
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
