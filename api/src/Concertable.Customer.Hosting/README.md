# Concertable.Customer.Hosting

Hosting metadata and Aspire topology for the Concertable Customer service, for use by an AppHost that
composes Customer alongside other services. The package contains no Customer runtime, persistence or
controller implementation.

`AddCustomerWeb` registers the Customer web resource either as a published container image or as a
project reference. `CustomerConstants` names the database, web resource, service name and container
port; `CustomerLocalSpaSurfaces` declares the local SPA surfaces and their auth clients.
