using Concertable.Kernel.Errors;

namespace Concertable.Customer.Preference.Application.Errors;

internal sealed record CreatePreferenceError(ErrorDefinition Definition) : IError
{
    public static readonly CreatePreferenceError PreferenceAlreadyExists = new(
        ErrorDefinition.Conflict(
            "preference.already_exists",
            "A preference already exists for this user."));
}
