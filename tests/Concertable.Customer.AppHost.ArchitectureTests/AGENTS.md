# Concertable.Customer.AppHost.ArchitectureTests — AppHost composition tests

Build the real Customer AppHost registration graph without starting it or external infrastructure. This
project remains outside the standalone solution until the AppHost's foreign hosting inputs are available as
published artifacts; it must not be weakened or replaced with a service-local fake.

Host coverage and activation rules: the `composition-testing` skill.
