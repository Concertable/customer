using System.Collections.Frozen;

namespace Concertable.Customer.Ticket.Infrastructure.Services.Workflow;

internal sealed class TicketPayeeResolver : ITicketPayee
{
    private readonly FrozenDictionary<ContractType, ITicketPayee> payees;

    public TicketPayeeResolver(ArtistTicketPayee artist, VenueTicketPayee venue)
    {
        payees = new Dictionary<ContractType, ITicketPayee>
        {
            [ContractType.VenueHire] = artist,
            [ContractType.FlatFee] = venue,
            [ContractType.DoorSplit] = venue,
            [ContractType.Versus] = venue,
        }.ToFrozenDictionary();
    }

    public Guid Resolve(ConcertEntity concert, IContract contract) =>
        payees[contract.ContractType].Resolve(concert, contract);
}
