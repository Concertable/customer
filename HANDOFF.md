# Handoff: Customer frontend fold into customer-next

Originating machine hit 100% disk (2.6GB free) mid-verification — moving this to a machine with room.

## Done
- `git-filter-repo` combined `api/Concertable.Customer` + `app/web/customer` + `app/mobile/customer` +
  `app/customer/shared` from `Concertable/concertable`'s `main` into this fresh history (976 commits).
  Backend lands at repo root (`src/`, `tests/`, etc.), frontend under `app/`.
- carve-fe package.json repointing applied and committed (commit after the filter-repo run) — the three
  frontend package.jsons now point `@concertable/*` deps at the published alpha feed instead of a
  monorepo workspace hoist, following `app/scripts/carve-fe.mjs`'s technique from the original repo.

## NOT yet verified (do this first)
1. `npm install` from repo root — was hitting `ECONNRESET` against the public registry on the origin
   machine (likely disk-pressure-adjacent, not necessarily a real registry problem — retry clean here).
2. `app/web/customer` builds/typechecks standalone off the published feed (no monorepo root `node_modules`
   in scope).
3. Backend still builds at 0 errors, same closure as the existing proof (do not regress).
4. `app/mobile/customer` — installs and the workspace resolves; full build may not be runnable outside
   the original CI, confirming resolution is enough if so — say so explicitly, don't skip silently.

## Once verified
Force-push this to replace `customer-next`'s `main`:
`git push --force origin HEAD:main` (this repo is a private staging repo with no other consumers — safe).
Then report: folder shape chosen (already fixed by this commit — don't re-decide), backend build result,
frontend build/typecheck result, mobile verification result, commit count, and confirm
`git ls-remote origin main` matches local HEAD after the push.

## Context
Full original brief: `CODEX_FOLD_CUSTOMER_FRONTEND.md` in `Concertable/concertable`'s root (may not be
available on this machine — this file is the self-contained summary).
Progress ledger: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PROGRESS.md` in that same repo,
Stage 7 entry.
