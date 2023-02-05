Feature: Compensation
Background:
	Given the time in UTC: '2023.01.27 12:33:00'


Scenario Outline: Eligible
	Given a meal ordered with id '9876'
	Given the times goes by '<time>' seconds
	Given a meal is ready with id '9876'

	When processing events

	Then order '9876' is eligible for compensation

Examples:
	| time |
	| 120  |
	| 121  |
	| 300  |

Scenario Outline: Not eligible
	Given a meal ordered with id '12345'
	Given the times goes by '<time>' seconds
	Given a meal is ready with id '12345'

	When processing events

	Then order '12345' is not eligible for compensation
Examples:
	| time |
	| 119  |
	| 95   |
	| 30   |
