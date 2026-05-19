using Concertable.Concert.Domain;
using Concertable.Contract.Contracts;

namespace Concertable.Customer.Ticket.Application.Interfaces;

internal interface ITicketPayee
{
    Guid Resolve(ConcertEntity concert, IContract contract);
}
