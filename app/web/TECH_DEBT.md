# app/web/customer — Technical Debt

## LOW

### Hand-rolled `useState` + manual zod `safeParse` per field, where `react-hook-form` is now the standard

`useAddReview.ts`'s `ReviewBuffer` hand-rolls the write-boundary pattern: a `useState` per field, a
manually named pre-validation type ("buffer" also reads as a binary/IO concept), and a facade hook
exposing `validate`/`submit` that calls `schema.safeParse` by hand. `react-hook-form` +
`@hookform/resolvers/zod` does this same job with less code and no separate draft type at all.

Migrated in the admin console's own copy of this pattern (`InviteForm`/`useInviteAdmin`,
`ResolveReportDialog`/`useResolveReport` — `app/web/admin/src/features/{admins,moderation}/`) when this
was raised; that's the model to follow. Sibling debt in `app/shared` (`useReportMessage`) and
`app/web/b2b/shared` (`useInviteMember`, `useOrganization`) covers the rest of the codebase.

**Resolves when:** `useAddReview` uses `useForm` + `zodResolver` the same way, `ReviewBuffer` and the
manual `validate`/`safeParse` call disappear entirely rather than getting renamed.
