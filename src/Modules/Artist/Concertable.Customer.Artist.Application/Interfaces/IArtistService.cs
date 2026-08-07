using Concertable.Customer.Artist.Application.DTOs;
using Concertable.Kernel.Functional;

namespace Concertable.Customer.Artist.Application.Interfaces;

internal interface IArtistService
{
    Task<Option<ArtistDetails>> GetDetailsByIdAsync(int artistId);
}
