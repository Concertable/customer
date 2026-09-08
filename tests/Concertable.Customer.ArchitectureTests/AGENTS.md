# Concertable.Customer.ArchitectureTests — architecture tests

**Assertions over Customer's code structure only** — which assemblies the composition root may reference.
Nothing here builds or boots a host: whether a Customer host would refuse to start on the configuration its
app model supplies is `Concertable.Customer.StartupTests`.
