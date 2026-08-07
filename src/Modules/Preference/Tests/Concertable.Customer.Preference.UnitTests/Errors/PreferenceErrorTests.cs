using Concertable.Customer.Preference.Application.Errors;
using Concertable.Kernel.Errors;

namespace Concertable.Customer.Preference.UnitTests.Errors;

public sealed class PreferenceErrorTests
{
    [Fact]
    public void PreferenceAlreadyExists_Definition_IsStable()
    {
        var definition = new CreatePreferenceError.PreferenceAlreadyExists().Definition;

        Assert.Equal("preference.already_exists", definition.Code);
        Assert.Equal("A preference already exists for this user.", definition.Message);
        Assert.Equal(ErrorKind.Conflict, definition.Kind);
    }

    [Fact]
    public void PreferenceNotFound_Definition_IsStable()
    {
        var definition = new UpdatePreferenceError.PreferenceNotFound().Definition;

        Assert.Equal("preference.not_found", definition.Code);
        Assert.Equal("Preference not found.", definition.Message);
        Assert.Equal(ErrorKind.NotFound, definition.Kind);
    }

    [Fact]
    public void PreferenceNotOwned_Definition_IsStable()
    {
        var definition = new UpdatePreferenceError.PreferenceNotOwned().Definition;

        Assert.Equal("preference.not_owned", definition.Code);
        Assert.Equal("You do not own this preference.", definition.Message);
        Assert.Equal(ErrorKind.Forbidden, definition.Kind);
    }
}
