# Customer / Marketplace Legal & Compliance Requirements (leads)

Status (2026-06-01): **future leads, not a backlog yet.** The Customer marketplace is a
**later, optional add-on** to B2B — B2B is deployed and sold standalone first. These are the
fan / ticket-buyer obligations recorded now so the picture is complete; none are urgent
until the marketplace is actually being built.

**These are a separate system from B2B.** Keep them here — do not merge into B2B's
[`../Concertable.B2B/Modules/Contract/LEGAL_REQUIREMENTS.md`](../Concertable.B2B/Modules/Contract/LEGAL_REQUIREMENTS.md).
B2B covers venue↔artist; this covers fan→seller.

---

## 0. Foundational decision — VAT posture for ticket sales: Agent / marketplace-facilitator

**Decided 2026-06-01 (accountant sign-off before launch of the marketplace).**

When tickets are sold to fans, Concertable is an **agent / marketplace facilitator**, *not*
the principal:

- The **venue is the merchant of record and seller**; we facilitate via Stripe Connect
  (destination/direct charges into the venue's connected account).
- We account for VAT **only on our own booking/service fee**, never on the full ticket face
  value.
- Most target segment-2 venues sit below the £90k VAT threshold, so they often charge £0 VAT
  on tickets — being the principal would bolt 20% onto every ticket and make us
  uncompetitive vs DICE/Skiddle.

HMRC judges this on **substance, not labels**. To legitimately remain an agent, by
construction:

1. Name the **venue** as the seller on the ticket and the fan-facing receipt.
2. Show the **venue's** ticket T&Cs at purchase; the purchase contract is fan-to-venue.
3. Route the charge to the **venue's** Stripe connected account (direct/destination charge),
   never to a Concertable balance, except our own fee.
4. Keep refund authority and consumer obligations contractually with the venue (we provide
   the mechanism; the venue is the counterparty).

If we ever set ticket prices unilaterally, hold ticket funds as our own, or present
"Concertable" as the seller, HMRC can deem us principal and we inherit 20% output VAT on
full ticket value. Don't drift into that.

---

## Leads (activate when building the marketplace)

### A. Ticket-buyer refund rights
**Legal basis:** Consumer Rights Act 2015; Consumer Contracts (Information, Cancellation and
Additional Charges) Regulations 2013; Digital Markets, Competition and Consumers Act 2024.
- Full refund on a **cancelled** event.
- Refund-or-accept choice on a **materially changed** event (headliner swap, date/venue
  change).
- Live-event tickets are **exempt** from the CCRs 14-day cooling-off right — but only if the
  ticket T&Cs state it. The terms must say so explicitly.
- Wire to the B2B cancellation event (B2B doc item 5) so a cancelled concert refunds both
  the venue↔artist escrow *and* the fans.

### B. DMCCA 2024 — all-in pricing
**Legal basis:** Digital Markets, Competition and Consumers Act 2024 (in force April 2025).
Any booking/service fee must be in the **headline ticket price** shown to the fan, not added
at checkout (no drip pricing). Audit the purchase flow.

### C. DMCCA 2024 — fake-review controls
**Legal basis:** as above.
Reviews exist on the B2B side (`ArtistReviewsController` / `ArtistReviewsService`). If review
data is surfaced to fans on the marketplace, reviews must be genuine and we must take
reasonable steps to detect/remove fake or incentivised reviews — e.g. only allow reviews
from verified attendees, log provenance. This is now a legal duty, not a nicety.

### D. Secondary-ticketing rules (only if resale is ever added)
**Legal basis:** Consumer Rights Act 2015 ss.90–95; DMCCA 2024; pending resale price-cap
legislation.
If fans can resell, the platform must disclose face value, seat/area, restrictions, and
whether the seller is a business or a fan — plus forthcoming caps on resale price and service
fees. Almost certainly out of scope; recorded for completeness.

### E. Fan PII — consent + GDPR retention/erasure
**Legal basis:** UK GDPR; PECR.
The marketplace introduces a new PII population (fans). Cookie consent, marketing consent,
retention policy, and right-to-erasure all apply on the Customer service, mirroring B2B
items 6–7 but for fans. Preserve purchase/financial records through erasure (anonymise the
person, keep the statutory record).
