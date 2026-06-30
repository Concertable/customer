# Concertable.Customer

The **Customer** service of [Concertable](https://github.com/Concertable/concertable) — the
consumer-facing side of the marketplace, where customers browse concerts, buy tickets, and leave
reviews. It is a *data service*: it owns its data and talks to other data services (B2B, Search)
only through `*.Contracts` integration events, never their runtime. It depends on the **Auth** and
**Payment** adapter services at runtime.

## Canonical source vs. this mirror

Development happens in the **monorepo** ([`Concertable/concertable`](https://github.com/Concertable/concertable)),
under `api/Concertable.Customer/`. That folder is **automatically mirrored** to the read-only repo
[`Concertable/concertable-customer`](https://github.com/Concertable/concertable-customer) on every
push to `master`. **Don't open PRs against the mirror** — nothing flows back from it.

## Building standalone

The deployable closure consumes Concertable's shared platform and cross-service contracts as NuGet
`PackageReference`s from the private org feed `https://nuget.pkg.github.com/Concertable`. Restoring
them needs a GitHub [personal access token](https://github.com/settings/tokens) with the
**`read:packages`** scope, exported as `GITHUB_PACKAGES_TOKEN` (the `nuget.config` reads it):

```sh
export GITHUB_PACKAGES_TOKEN=<your read:packages PAT>
dotnet build Concertable.Customer.Web/Concertable.Customer.Web.csproj
```

Building the host project pulls the whole deployable closure. (In the monorepo's CI the same
variable is supplied by the workflow's `GITHUB_TOKEN`; standalone, you export your own PAT.)
