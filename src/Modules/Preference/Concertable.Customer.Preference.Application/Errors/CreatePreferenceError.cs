using Dunet;
using Reunion.Errors;

namespace Concertable.Customer.Preference.Application.Errors;

[Union(EnableImplicitConversions = false)]
internal abstract partial record CreatePreferenceError : IError
{
    private static readonly ErrorDefinitions<CreatePreferenceError> Definitions =
        ErrorDefinition.For<CreatePreferenceError>();

    public ErrorDefinition Definition => this switch
    {
        PreferenceAlreadyExists => Definitions.Conflict<PreferenceAlreadyExists>(
            "A preference already exists for this user.")
    };

    [ErrorCode("preference.already_exists")]
    public partial record PreferenceAlreadyExists;
}
