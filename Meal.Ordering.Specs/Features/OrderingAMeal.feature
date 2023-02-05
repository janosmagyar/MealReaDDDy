Feature: Ordering a meal

Scenario: Ordering a meal
	Given the next order number from the central order service will be '111'

	Given the order to pick up at counter
		| Count | Name      | Category |
		| 1     | hamburger | burger   |

	When the customer place the order

	Then the order
		| Order number | State         | Serving | Table |
		| 111          | InPreparation | tray    | empty |

	Then the items in the order are
		| Count | Name      | Category | Prepared |
		| 1     | hamburger | burger   | false    |

Scenario: Ordering a take-away meal
	Given the next order number from the central order service will be '36'

	Given the order to take away
		| Count | Name         | Category |
		| 2     | cheeseburger | burger   |
		| 1     | big fries    | snack    |
		| 1     | mid coke     | drink    |

	When the customer place the order

	Then the order
		| Order number | State         | Serving  | Table |
		| 36           | InPreparation | paperbag | empty |

	Then the items in the order are
		| Count | Name         | Category | Prepared |
		| 2     | cheeseburger | burger   | false    |
		| 1     | big fries    | snack    | false    |
		| 1     | mid coke     | drink    | false    |

Scenario: Ordering a meal to a table
	Given the next order number from the central order service will be '23'

	Given the order serve to table '99'
		| Count | Name      | Category |
		| 1     | hamburger | burger   |

	When the customer place the order

	Then the order
		| Order number | State         | Serving | Table |
		| 23           | InPreparation | tray    | 99    |

	Then the items in the order are
		| Count | Name      | Category | Prepared |
		| 1     | hamburger | burger   | false    |

Scenario: Ordering twice cause an error
	Given the order serve to table '99'
		| Count | Name      | Category |
		| 1     | hamburger | burger   |

	When the customer place the order
	When the customer place the order

	Then the error message is 'Invalid command for state!'

Scenario Outline: Wrong serving
	Given the order to take-away and having a table number with items
		| Count | Name      | Category |
		| 1     | hamburger | burger   |

	When the customer place the order

	Then the error message is 'Invalid serving!'




