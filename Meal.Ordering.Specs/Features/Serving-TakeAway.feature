Feature: Serving - TakeAway

Scenario: Ready, Paid, Taken away
	Given an order
		| Order number | State | Serving  | Table | Payment    |
		| 23           | Ready | paperbag | empty | Successful |

	And the items in the order are
		| Count | Name      | Category | Prepared |
		| 2     | hamburger | burger   | 2        |

	When customer takes the order

	Then the order
		| Order number | State | Serving  | Table |
		| 23           | Done  | paperbag | empty |

Scenario Outline: Ready, not  paid, error
	Given an order
		| Order number | State | Serving  | Table | Payment   |
		| 23           | Ready | paperbag | empty | <payment> |

	And the items in the order are
		| Count | Name      | Category | Prepared |
		| 2     | hamburger | burger   | 2        |

	When customer takes the order

	Then the error message is "Payment wasn't successful!"

Examples:
	| payment |
	| Failed  |
	| Waiting |

Scenario: Ready, wrong serving, error
	Given an order
		| Order number | State | Serving | Table | Payment    |
		| 23           | Ready | tray    | empty | Successful |

	And the items in the order are
		| Count | Name      | Category | Prepared |
		| 2     | hamburger | burger   | 2        |

	When customer takes the order

	Then the error message is "Wrong serving!"
