using Concertable.Kernel.Geometry;

namespace Concertable.Customer.Preference.Infrastructure.Services;

internal sealed class PreferenceService : IPreferenceService
{
    private readonly IPreferenceRepository preferenceRepository;
    private readonly ICurrentUser currentUser;
    private readonly IUserModule userModule;
    private readonly IGeometryCalculator geometryCalculator;

    public PreferenceService(
        IPreferenceRepository preferenceRepository,
        ICurrentUser currentUser,
        IUserModule userModule,
        IGeometryCalculator geometryCalculator)
    {
        this.preferenceRepository = preferenceRepository;
        this.currentUser = currentUser;
        this.userModule = userModule;
        this.geometryCalculator = geometryCalculator;
    }

    public async Task<Result<PreferenceDto, CreatePreferenceError>> CreateAsync(
        PreferenceRequest request,
        Guid? userId = null)
    {
        var resolvedUserId = userId ?? currentUser.GetId();
        var existing = await preferenceRepository.GetByUserIdAsync(resolvedUserId);
        if (existing is not null)
            return Result.Failure<PreferenceDto, CreatePreferenceError>(
                new CreatePreferenceError.PreferenceAlreadyExists());

        var preference = PreferenceEntity.Create(resolvedUserId, request.RadiusKm, request.Genres);

        await preferenceRepository.AddAsync(preference);
        await preferenceRepository.SaveChangesAsync();

        return Result.Success<PreferenceDto, CreatePreferenceError>(preference.ToDto());
    }

    public async Task<IReadOnlyList<PreferenceDto>> GetAsync()
    {
        var preferences = await preferenceRepository.GetAllAsync();
        return preferences.ToDtos();
    }

    public async Task<Option<PreferenceDto>> GetByUserIdAsync(Guid userId)
    {
        var preference = await preferenceRepository.GetByUserIdAsync(userId);
        return preference.ToOption().Map(value => value.ToDto());
    }

    public Task<Option<PreferenceDto>> GetByUserAsync() => GetByUserIdAsync(currentUser.GetId());

    public async Task<Result<PreferenceDto, UpdatePreferenceError>> UpdateAsync(
        int id,
        PreferenceRequest request)
    {
        var preference = await preferenceRepository.GetByIdAsync(id);
        if (preference is null)
            return Result.Failure<PreferenceDto, UpdatePreferenceError>(
                new UpdatePreferenceError.PreferenceNotFound());

        if (currentUser.GetId() != preference.UserId)
            return Result.Failure<PreferenceDto, UpdatePreferenceError>(
                new UpdatePreferenceError.PreferenceNotOwned());

        preference.Update(request.RadiusKm, request.Genres);

        preferenceRepository.Update(preference);
        await preferenceRepository.SaveChangesAsync();

        return Result.Success<PreferenceDto, UpdatePreferenceError>(preference.ToDto());
    }

    public async Task<IReadOnlyList<Guid>> GetUserIdsByLocationAndGenresAsync(
        double latitude,
        double longitude,
        IEnumerable<Genre> genres)
    {
        var preferences = await preferenceRepository.GetByMatchingGenresAsync(genres);
        if (preferences.Count == 0) return [];

        var users = await userModule.GetByIdsAsync(preferences.Select(p => p.UserId));
        var usersById = users.ToDictionary(u => u.Id);

        return preferences
            .Select(p => usersById.TryGetValue(p.UserId, out var u) ? (User: u, p.RadiusKm) : default)
            .Where(x => x.User?.Latitude is not null && x.User.Longitude is not null)
            .Where(x => geometryCalculator.IsWithinRadius(
                x.User!.Latitude!.Value,
                x.User.Longitude!.Value,
                latitude,
                longitude,
                (int)x.RadiusKm))
            .Select(x => x.User!.Id)
            .ToList();
    }
}
