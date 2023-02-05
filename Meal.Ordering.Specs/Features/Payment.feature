Feature: Payment
Background:
	Given an order
		| Order number | State | Serving | Table |
		| 23           | Ready | tray    | 99    |
	And the items in the order are
		| Count | Name      | Category | Prepared |
		| 2     | hamburger | burger   | 2        |

Scenario: No payment
	When no payment happened
	Then then the order payment is 'Waiting'

Scenario: Payment failed
	When payment failed
	Then then the order payment is 'Failed'

Scenario: Payment succeeded
	When payment succeeded
	Then then the order payment is 'Successful'

Scenario: Payment can fail more times
	When payment failed
	When payment failed
	When payment failed
	Then then the order payment is 'Failed'

Scenario: Payment cannot succeeded twice
	When payment succeeded
	When payment succeeded
	Then the error message is 'Payment was already successful!'

Scenario: Payment cannot fail after succeeded
	When payment succeeded
	When payment failed
	Then the error message is 'Payment was already successful!'
