# app/web/customer — Technical Debt

## LOW

### The write-boundary pattern's pre-validation form state is named `XBuffer`, not `XDraft`

`useAddReview.ts`'s `ReviewBuffer` names the controlled-input state before it's parsed into a request
`XBuffer`. "Buffer" reads as a binary/IO concept; "draft" is the clearer, unambiguous name for
"user-entered values not yet validated or committed."

Renamed in the admin console's own copy of this pattern (`InviteDraft`, `ResolveDraft` —
`app/web/admin/src/features/{admins,moderation}/`) when this was raised; this entry is what's left.
Sibling debt in `app/shared` (`ReportBuffer`) and `app/web/b2b/shared` (`InviteBuffer`,
`OrganizationBuffer`) covers the rest of the codebase.

**Resolves when:** `ReviewBuffer` → `ReviewDraft` (type and the hook's parameter name), matching the
admin console's already-renamed pattern.
