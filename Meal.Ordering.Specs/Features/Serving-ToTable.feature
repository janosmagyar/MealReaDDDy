Feature: Serving - To table

Scenario: Ready, Paid, serve
	Given an order
		| Order number | State | Serving | Table | Payment    |
		| 23           | Ready | tray    | 67    | Successful |

	And the items in the order are
		| Count | Name      | Category | Prepared |
		| 2     | hamburger | burger   | 2        |

	When expeditor serves the meal to the table

	Then the order
		| Order number | State | Serving | Table |
		| 23           | Done  | tray    | 67    |

Scenario Outline: Ready, not  paid, error
	Given an order
		| Order number | State | Serving | Table | Payment   |
		| 23           | Ready | tray    | 45    | <payment> |

	And the items in the order are
		| Count | Name      | Category | Prepared |
		| 2     | hamburger | burger   | 2        |

	When expeditor serves the meal to the table

	Then the error message is "Payment wasn't successful!"

Examples:
	| payment |
	| Failed  |
	| Waiting |

Scenario Outline: Ready, wrong serving, error
	Given an order
		| Order number | State | Serving   | Table | Payment    |
		| 23           | Ready | <serving> | empty | Successful |

	And the items in the order are
		| Count | Name      | Category | Prepared |
		| 2     | hamburger | burger   | 2        |

	When expeditor serves the meal to the table

	Then the error message is "Wrong serving!"
Examples:
	| serving  |
	| paperbag |
	| tray     |
