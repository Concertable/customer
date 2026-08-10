using Dunet;
using Reunion.Errors;

namespace Concertable.Customer.Preference.Application.Errors;

[Union(EnableImplicitConversions = false)]
internal abstract partial record UpdatePreferenceError : IError
{
    public ErrorDefinition Definition => this switch
    {
        PreferenceNotFound => ErrorDefinition.NotFound<PreferenceNotFound>(),
        PreferenceNotOwned =>
            ErrorDefinition.Forbidden<PreferenceNotOwned>("You do not own this preference.")
    };

    [ErrorCode("preference.not_found")]
    public partial record PreferenceNotFound;

    [ErrorCode("preference.not_owned")]
    public partial record PreferenceNotOwned;
}
