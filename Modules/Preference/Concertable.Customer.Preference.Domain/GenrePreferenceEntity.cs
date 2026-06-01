using System.ComponentModel.DataAnnotations.Schema;

namespace Concertable.Customer.Preference.Domain;

[Table("GenrePreferences")]
public sealed class GenrePreferenceEntity : IIdEntity
{
    public int Id { get; private set; }
    public int PreferenceId { get; set; }
    public Genre Genre { get; set; }
    public PreferenceEntity Preference { get; set; } = null!;
}
