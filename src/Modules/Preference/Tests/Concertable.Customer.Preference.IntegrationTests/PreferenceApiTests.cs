using System.Net;
using System.Text.Json;
using Concertable.Contracts.Enums;
using Concertable.Customer.Preference.Application.DTOs;
using Concertable.Customer.Preference.Application.Interfaces;
using Concertable.Customer.Preference.Application.Requests;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit.Abstractions;

namespace Concertable.Customer.Preference.IntegrationTests;

[Collection("Integration")]
public sealed class PreferenceApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public PreferenceApiTests(ApiFixture fixture, ITestOutputHelper output)
    {
        this.fixture = fixture;
        fixture.AttachOutput(output);
    }

    public Task InitializeAsync() => fixture.ResetAsync();

    public Task DisposeAsync()
    {
        fixture.DetachOutput();
        return Task.CompletedTask;
    }

    #region GetByUser

    [Fact]
    public async Task GetByUser_ExistingPreference_Returns200WithPreference()
    {
        var user = fixture.SeedState.Customer1;
        var client = fixture.CreateClient(user);

        var response = await client.GetAsync("/api/preference/user");

        await response.ShouldBe(HttpStatusCode.OK);
        var preference = (await response.Content.ReadAsync<PreferenceDto>()).ShouldNotBeNull();
        preference.UserId.ShouldBe(user.Id);
        preference.RadiusKm.ShouldBe(10);
        preference.Genres.ShouldBe([Genre.Rock]);
    }

    [Fact]
    public async Task GetByUser_MissingPreference_Returns204()
    {
        var user = fixture.SeedState.Customer3;
        await RemovePreferenceAsync(user.Id);
        var client = fixture.CreateClient(user);

        var response = await client.GetAsync("/api/preference/user");

        await response.ShouldBe(HttpStatusCode.NoContent);
    }

    #endregion

    #region Create

    [Fact]
    public async Task Create_ExistingPreference_Returns409()
    {
        var client = fixture.CreateClient(fixture.SeedState.Customer1);

        var response = await client.PostAsync("/api/preference", NewRequest());

        await response.ShouldBe(HttpStatusCode.Conflict);
        await AssertProblemDetailsAsync(
            response,
            HttpStatusCode.Conflict,
            "Conflict",
            "A preference already exists for this user.",
            "preference.already_exists");
    }

    [Fact]
    public async Task Create_MissingPreference_Returns201WithBody()
    {
        var user = fixture.SeedState.Customer3;
        await RemovePreferenceAsync(user.Id);
        var client = fixture.CreateClient(user);

        var response = await client.PostAsync("/api/preference", NewRequest());

        await response.ShouldBe(HttpStatusCode.Created);
        var createdPreference = (await response.Content.ReadAsync<PreferenceDto>()).ShouldNotBeNull();
        createdPreference.UserId.ShouldBe(user.Id);
        createdPreference.RadiusKm.ShouldBe(30);
        createdPreference.Genres.Order().ShouldBe([Genre.Rock, Genre.Jazz]);

        var getResponse = await client.GetAsync("/api/preference/user");
        await getResponse.ShouldBe(HttpStatusCode.OK);
        var preference = (await getResponse.Content.ReadAsync<PreferenceDto>()).ShouldNotBeNull();
        preference.UserId.ShouldBe(user.Id);
        preference.RadiusKm.ShouldBe(30);
        preference.Genres.Order().ShouldBe([Genre.Rock, Genre.Jazz]);
    }

    #endregion

    #region Update

    [Fact]
    public async Task Update_MissingPreference_Returns404()
    {
        var client = fixture.CreateClient(fixture.SeedState.Customer1);

        var response = await client.PutAsync("/api/preference/2147483647", NewRequest());

        await response.ShouldBe(HttpStatusCode.NotFound);
        await AssertProblemDetailsAsync(
            response,
            HttpStatusCode.NotFound,
            "Not Found",
            "Preference not found.",
            "preference.not_found");
    }

    [Fact]
    public async Task Update_ForeignPreference_Returns403()
    {
        var preferenceId = await GetPreferenceIdAsync(fixture.SeedState.Customer2.Id);
        var client = fixture.CreateClient(fixture.SeedState.Customer1);

        var response = await client.PutAsync($"/api/preference/{preferenceId}", NewRequest());

        await response.ShouldBe(HttpStatusCode.Forbidden);
        await AssertProblemDetailsAsync(
            response,
            HttpStatusCode.Forbidden,
            "Forbidden",
            "You do not own this preference.",
            "preference.not_owned");
    }

    [Fact]
    public async Task Update_OwnPreference_Returns200WithUpdatedPreference()
    {
        var user = fixture.SeedState.Customer1;
        var preferenceId = await GetPreferenceIdAsync(user.Id);
        var client = fixture.CreateClient(user);

        var response = await client.PutAsync(
            $"/api/preference/{preferenceId}",
            NewRequest());

        await response.ShouldBe(HttpStatusCode.OK);
        var preference = (await response.Content.ReadAsync<PreferenceDto>()).ShouldNotBeNull();
        preference.Id.ShouldBe(preferenceId);
        preference.UserId.ShouldBe(user.Id);
        preference.RadiusKm.ShouldBe(30);
        preference.Genres.Order().ShouldBe([Genre.Rock, Genre.Jazz]);
    }

    #endregion

    private async Task<int> GetPreferenceIdAsync(Guid userId)
    {
        using var scope = fixture.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPreferenceRepository>();
        var preference = (await repository.GetByUserIdAsync(userId)).ShouldNotBeNull();
        return preference.Id;
    }

    private async Task RemovePreferenceAsync(Guid userId)
    {
        using var scope = fixture.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPreferenceRepository>();
        var preference = (await repository.GetByUserIdAsync(userId)).ShouldNotBeNull();
        repository.Remove(preference);
        await repository.SaveChangesAsync();
    }

    private static PreferenceRequest NewRequest() => new()
    {
        RadiusKm = 30,
        Genres = [Genre.Rock, Genre.Jazz]
    };

    private static async Task AssertProblemDetailsAsync(
        HttpResponseMessage response,
        HttpStatusCode status,
        string title,
        string detail,
        string code)
    {
        using var document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync());
        var problem = document.RootElement;

        problem.GetProperty("status").GetInt32().ShouldBe((int)status);
        problem.GetProperty("title").GetString().ShouldBe(title);
        problem.GetProperty("detail").GetString().ShouldBe(detail);
        problem.GetProperty("code").GetString().ShouldBe(code);
    }
}
