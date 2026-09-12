# Concertable.Customer.Ticket.Contracts

The ticket contract surface published by the Concertable Customer service: the `TicketPurchasedEvent`
integration event, the `SendTicketEmailCommand` command, the `ITicketModule` facade and the module's
display names.

Contracts only: no Customer runtime, persistence or handler implementation. A ticket is minted by
Customer on a succeeded payment, not returned synchronously by the purchase call.
