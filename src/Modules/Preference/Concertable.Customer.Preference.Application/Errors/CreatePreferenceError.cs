using Concertable.Kernel.Errors;
using Dunet;

namespace Concertable.Customer.Preference.Application.Errors;

[Union(EnableImplicitConversions = false)]
internal abstract partial record CreatePreferenceError : IError
{
    public ErrorDefinition Definition => this switch
    {
        PreferenceAlreadyExists => ErrorDefinition.Conflict<PreferenceAlreadyExists>(
            "A preference already exists for this user.")
    };

    [ErrorCode("preference.already_exists")]
    public partial record PreferenceAlreadyExists;
}
