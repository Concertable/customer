Feature: Third-party load-on-use
  Non-essential third parties must not load on the anonymous landing page: Stripe.js
  loads only at checkout and Google Maps only where a map or search is shown, so no
  Stripe fraud cookie and no maps request fires before the user needs the feature (UK PECR).

  Scenario: The anonymous landing page contacts no payment or maps third party
    Given a visitor is on the customer landing page
    Then Stripe.js is not requested
    And Google Maps is not requested
    And no Stripe fraud cookie is set

  Scenario: The find page loads Google Maps on use but still not Stripe
    Given a visitor is on the find page
    Then Google Maps is requested
    And Stripe.js is not requested
    And no Stripe fraud cookie is set
