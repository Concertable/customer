using Concertable.Kernel.Errors;

namespace Concertable.Customer.Preference.Application.Errors;

internal sealed record UpdatePreferenceError(ErrorDefinition Definition) : IError
{
    public static readonly UpdatePreferenceError PreferenceNotFound = new(
        ErrorDefinition.NotFound(
            "preference.not_found",
            "Preference not found."));

    public static readonly UpdatePreferenceError PreferenceNotOwned = new(
        ErrorDefinition.Forbidden(
            "preference.not_owned",
            "You do not own this preference."));
}
